﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using SysExpr = System.Linq.Expressions.Expression;

namespace FastExpressionCompiler.LightExpression;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>Facade for constructing Expression.</summary>
public abstract class Expression
{
  /// <summary>Expression node type.</summary>
  public abstract ExpressionType NodeType { get; }

  /// <summary>All expressions should have a Type.</summary>
  public abstract Type Type { get; }

  internal struct LightAndSysExpr
  {
    public Expression LightExpr;
    public SysExpr SysExpr;
  }

  public SysExpr ToExpression()
  {
    var exprsConverted = new LiveCountArray<LightAndSysExpr>(Tools.Empty<LightAndSysExpr>());
    return ToExpression(ref exprsConverted);
  }

  /// <summary>Converts back to the respective System Expression
  /// by first checking if `this` expression is already contained in the `exprsConverted` collection</summary>
  internal SysExpr ToExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    var i = exprsConverted.Count - 1;
    while (i != -1 && !ReferenceEquals(exprsConverted.Items[i].LightExpr, this)) --i;
    if (i != -1)
      return exprsConverted.Items[i].SysExpr;

    var sysExpr = CreateSysExpression(ref exprsConverted);

    ref var item = ref exprsConverted.PushSlot();
    item.LightExpr = this;
    item.SysExpr = sysExpr;

    return sysExpr;
  }

  internal abstract SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted);

  /// <summary>
  /// Tries to print the expression in its constructing syntax - helpful to get it from debug and put into code to test,
  /// e.g. <code><![CDATA[ Lambda(New(typeof(X).GetTypeInfo().DeclaredConstructors.ToArray()[1]), Parameter(typeof(X), "x")) ]]></code>.
  /// 
  /// NOTE: It is trying hard but the Parameter expression are not consolidated into one. Hopefully R# will help you to re-factor them into a single variable.
  /// </summary>
  public string CodeString => ToCodeString(new StringBuilder(1024), 2).ToString();

  /// <summary>Code printer with the provided configuration</summary>
  public abstract StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2);

  /// <summary>Converts to Expression and outputs its as string</summary>
  public override string ToString() => ToExpression().ToString();

  /// <summary>Reduces the Expression to simple ones</summary>
  public virtual Expression Reduce() => this;

  internal static SysExpr[] ToExpressions(IReadOnlyList<Expression> exprs, ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    if (exprs.Count == 0)
      return Tools.Empty<SysExpr>();

    if (exprs.Count == 1)
      return new[] { exprs[0].ToExpression(ref exprsConverted) };

    var result = new SysExpr[exprs.Count];
    for (var i = 0; i < result.Length; ++i)
      result[i] = exprs[i].ToExpression(ref exprsConverted);
    return result;
  }

  public static ParameterExpression Parameter(Type type, string name = null) =>
    new ParameterExpression(type.IsByRef ? type.GetElementType() : type, name, type.IsByRef);

  public static ParameterExpression Variable(Type type, string name = null) => Parameter(type, name);

  public static readonly ConstantExpression NullConstant = new TypedConstantExpression(null, typeof(object));
  public static readonly ConstantExpression FalseConstant = new ConstantExpression(false);
  public static readonly ConstantExpression TrueConstant = new ConstantExpression(true);
  public static readonly ConstantExpression ZeroConstant = new ConstantExpression(0);

  public static ConstantExpression Constant(bool value) =>
    value ? TrueConstant : FalseConstant;

  public static ConstantExpression Constant(int value) =>
    value == 0 ? ZeroConstant : new TypedConstantExpression<int>(value);

  public static ConstantExpression Constant<T>(T value) =>
    new TypedConstantExpression<T>(value);

  public static ConstantExpression Constant(object value)
  {
    if (value == null)
      return NullConstant;

    if (value is bool b)
      return b ? TrueConstant : FalseConstant;

    if (value is int n)
      return n == 0 ? ZeroConstant : new TypedConstantExpression<int>(n);

    return new ConstantExpression(value);
  }

  public static ConstantExpression Constant(object value, Type type) =>
    new TypedConstantExpression(value, type);

  public static NewExpression New(Type type)
  {
    if (type.IsValueType())
      return new NewValueTypeExpression(type);

    foreach (var x in type.GetTypeInfo().DeclaredConstructors)
      if (x.GetParameters().Length == 0)
        return new NewExpression(x);

    throw new ArgumentException($"The type {type} is missing the default constructor");
  }

  public static NewExpression New(ConstructorInfo ctor, params Expression[] arguments) =>
    arguments == null || arguments.Length == 0 ? new NewExpression(ctor) : new ManyArgumentsNewExpression(ctor, arguments);

  public static NewExpression New(ConstructorInfo ctor, IEnumerable<Expression> arguments)
  {
    var args = arguments.AsReadOnlyList();
    return args == null || args.Count == 0 ? new NewExpression(ctor) : new ManyArgumentsNewExpression(ctor, args);
  }

  public static NewExpression New(ConstructorInfo ctor) => new NewExpression(ctor);

  public static NewExpression New(ConstructorInfo ctor, Expression arg) =>
    new OneArgumentNewExpression(ctor, arg);

  public static NewExpression New(ConstructorInfo ctor, Expression arg0, Expression arg1) =>
    new TwoArgumentsNewExpression(ctor, arg0, arg1);

  public static NewExpression New(ConstructorInfo ctor, Expression arg0, Expression arg1, Expression arg2) =>
    new ThreeArgumentsNewExpression(ctor, arg0, arg1, arg2);

  public static NewExpression New(ConstructorInfo ctor,
    Expression arg0, Expression arg1, Expression arg2, Expression arg3) =>
    new FourArgumentsNewExpression(ctor, arg0, arg1, arg2, arg3);

  public static NewExpression New(ConstructorInfo ctor,
    Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) =>
    new FiveArgumentsNewExpression(ctor, arg0, arg1, arg2, arg3, arg4);

  public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
  {
    if (arguments == null || arguments.Length == 0)
      return new MethodCallExpression(method);
    return new ManyArgumentsMethodCallExpression(method, arguments);
  }

  public static MethodCallExpression Call(MethodInfo method, IEnumerable<Expression> arguments)
  {
    var args = arguments.AsReadOnlyList();
    if (args == null || args.Count == 0)
      return new MethodCallExpression(method);
    return new ManyArgumentsMethodCallExpression(method, args);
  }

  public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments)
  {
    if (arguments == null || arguments.Length == 0)
      return new InstanceMethodCallExpression(instance, method);
    return new InstanceManyArgumentsMethodCallExpression(instance, method, arguments);
  }

  public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
  {
    var args = arguments.AsReadOnlyList();
    if (args == null || args.Count == 0)
      return new InstanceMethodCallExpression(instance, method);
    return new InstanceManyArgumentsMethodCallExpression(instance, method, args);
  }

  public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments)
  {
    if (arguments == null || arguments.Length == 0)
      return new MethodCallExpression(type.FindMethod(methodName, typeArguments, arguments, isStatic: true));
    return new ManyArgumentsMethodCallExpression(type.FindMethod(methodName, typeArguments, arguments, isStatic: true), arguments);
  }

  public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, IEnumerable<Expression> arguments)
  {
    var args = arguments.AsReadOnlyList();
    if (args == null || args.Count == 0)
      return new MethodCallExpression(type.FindMethod(methodName, typeArguments, args, isStatic: true));
    return new ManyArgumentsMethodCallExpression(type.FindMethod(methodName, typeArguments, args, isStatic: true), args);
  }

  public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments)
  {
    if (arguments == null || arguments.Length == 0)
      return new InstanceMethodCallExpression(instance, instance.Type.FindMethod(methodName, typeArguments, arguments));
    return new InstanceManyArgumentsMethodCallExpression(instance, instance.Type.FindMethod(methodName, typeArguments, arguments), arguments);
  }

  public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, IEnumerable<Expression> arguments)
  {
    var args = arguments.AsReadOnlyList();
    if (args == null || args.Count == 0)
      return new InstanceMethodCallExpression(instance, instance.Type.FindMethod(methodName, typeArguments, args));
    return new InstanceManyArgumentsMethodCallExpression(instance, instance.Type.FindMethod(methodName, typeArguments, args), args);
  }

  public static MethodCallExpression Call(MethodInfo method) =>
    new MethodCallExpression(method);

  public static MethodCallExpression Call(Expression instance, MethodInfo method) =>
    instance == null
      ? new MethodCallExpression(method)
      : new InstanceMethodCallExpression(instance, method);

  public static MethodCallExpression Call(MethodInfo method, Expression argument) =>
    new OneArgumentMethodCallExpression(method, argument);

  public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression argument) =>
    instance == null
      ? new OneArgumentMethodCallExpression(method, argument)
      : new InstanceOneArgumentMethodCallExpression(instance, method, argument);

  public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1) =>
    new TwoArgumentsMethodCallExpression(method, arg0, arg1);

  public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1) =>
    instance == null
      ? new TwoArgumentsMethodCallExpression(method, arg0, arg1)
      : new InstanceTwoArgumentsMethodCallExpression(instance, method, arg0, arg1);

  public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) =>
    new ThreeArgumentsMethodCallExpression(method, arg0, arg1, arg2);

  public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2) =>
    instance == null
      ? new ThreeArgumentsMethodCallExpression(method, arg0, arg1, arg2)
      : new InstanceThreeArgumentsMethodCallExpression(instance, method, arg0, arg1, arg2);

  public static MethodCallExpression Call(MethodInfo method,
    Expression arg0, Expression arg1, Expression arg2, Expression arg3) =>
    new FourArgumentsMethodCallExpression(method, arg0, arg1, arg2, arg3);

  public static MethodCallExpression Call(Expression instance, MethodInfo method,
    Expression arg0, Expression arg1, Expression arg2, Expression arg3) =>
    instance == null
      ? new FourArgumentsMethodCallExpression(method, arg0, arg1, arg2, arg3)
      : new InstanceFourArgumentsMethodCallExpression(instance, method, arg0, arg1, arg2, arg3);

  public static MethodCallExpression Call(MethodInfo method,
    Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) =>
    new FiveArgumentsMethodCallExpression(method, arg0, arg1, arg2, arg3, arg4);

  public static MethodCallExpression Call(Expression instance, MethodInfo method,
    Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) =>
    instance == null
      ? new FiveArgumentsMethodCallExpression(method, arg0, arg1, arg2, arg3, arg4)
      : new InstanceFiveArgumentsMethodCallExpression(instance, method, arg0, arg1, arg2, arg3, arg4);

  public static Expression CallIfNotNull(Expression instance, MethodInfo method) =>
    CallIfNotNull(instance, method, Tools.Empty<Expression>());

  public static Expression CallIfNotNull(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
  {
    var instanceVar = Parameter(instance.Type, "x");
    return Block(
      instanceVar,
      Assign(instanceVar, instance),
      Condition(
        Equal(instanceVar, Constant(null, instance.Type)),
        Constant(null),
        Call(instanceVar, method, arguments),
        method.ReturnType));
  }

  public static MemberExpression Property(PropertyInfo property) =>
    new PropertyExpression(null, property);

  public static MemberExpression Property(Expression instance, PropertyInfo property) =>
    new PropertyExpression(instance, property);

  public static MemberExpression Property(Expression expression, string propertyName) =>
    Property(expression, expression.Type.FindProperty(propertyName)
                         ?? throw new ArgumentException($"Declared property with the name '{propertyName}' is not found in '{expression.Type}'", nameof(propertyName)));

  public static IndexExpression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) =>
    new IndexExpression(instance, indexer, arguments);

  public static IndexExpression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) =>
    new IndexExpression(instance, indexer, arguments.AsReadOnlyList());

  public static MemberExpression PropertyOrField(Expression expression, string propertyName) =>
    expression.Type.FindProperty(propertyName) != null ?
      (MemberExpression)new PropertyExpression(expression, expression.Type.FindProperty(propertyName)
                                                           ?? throw new ArgumentException($"Declared property with the name '{propertyName}' is not found in '{expression.Type}'", nameof(propertyName))) :
      new FieldExpression(expression, expression.Type.FindField(propertyName)
                                      ?? throw new ArgumentException($"Declared field with the name '{propertyName}' is not found '{expression.Type}'", nameof(propertyName)));

  public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member)
  {
    if (member is FieldInfo field)
      return Field(expression, field);
    if (member is PropertyInfo property)
      return Property(expression, property);
    throw new ArgumentException($"Member is not field or property: {member}", nameof(member));
  }

  public static IndexExpression MakeIndex(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) =>
    indexer != null ? Property(instance, indexer, arguments) : ArrayAccess(instance, arguments);

  public static IndexExpression ArrayAccess(Expression array, params Expression[] indexes) =>
    new IndexExpression(array, null, indexes);

  public static IndexExpression ArrayAccess(Expression array, IEnumerable<Expression> indexes) =>
    new IndexExpression(array, null, indexes.AsReadOnlyList());

  public static MemberExpression Field(FieldInfo field) =>
    new FieldExpression(null, field);

  public static MemberExpression Field(Expression instance, FieldInfo field) =>
    new FieldExpression(instance, field);

  public static MemberExpression Field(Expression instance, string fieldName) =>
    new FieldExpression(instance, instance.Type.FindField(fieldName));

  /// <summary>Creates a UnaryExpression that represents a bitwise complement operation.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to Not and the Operand property set to the specified value.</returns>
  public static UnaryExpression Not(Expression expression) =>
    new UnaryExpression(ExpressionType.Not, expression);

  /// <summary>Creates a UnaryExpression that represents an explicit reference or boxing conversion where null is supplied if the conversion fails.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <param name="type">A Type to set the Type property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to TypeAs and the Operand and Type properties set to the specified values.</returns>
  public static UnaryExpression TypeAs(Expression expression, Type type) =>
    new TypedUnaryExpression(ExpressionType.TypeAs, expression, type);

  public static TypeBinaryExpression TypeEqual(Expression operand, Type type) =>
    new TypeBinaryExpression(ExpressionType.TypeEqual, operand, type);

  public static TypeBinaryExpression TypeIs(Expression operand, Type type) =>
    new TypeBinaryExpression(ExpressionType.TypeIs, operand, type);

  /// <summary>Creates a UnaryExpression that represents an expression for obtaining the length of a one-dimensional array.</summary>
  /// <param name="array">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to ArrayLength and the Operand property equal to array.</returns>
  public static UnaryExpression ArrayLength(Expression array) =>
    new TypedUnaryExpression<int>(ExpressionType.ArrayLength, array);

  /// <summary>Creates a UnaryExpression that represents a type conversion operation.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <param name="type">A Type to set the Type property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to Convert and the Operand and Type properties set to the specified values.</returns>
  public static UnaryExpression Convert(Expression expression, Type type) =>
    new TypedUnaryExpression(ExpressionType.Convert, expression, type);

  /// <summary>Creates a UnaryExpression that represents a type conversion operation.</summary>
  /// <typeparam name="TTo">A Type to set the Type property equal to.</typeparam>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to Convert and the Operand and Type properties set to the specified values.</returns>
  public static UnaryExpression Convert<TTo>(Expression expression) =>
    new TypedUnaryExpression<TTo>(ExpressionType.Convert, expression);

  /// <summary>Creates a UnaryExpression that represents a conversion operation for which the implementing method is specified.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <param name="type">A Type to set the Type property equal to.</param>
  /// <param name="method">A MethodInfo to set the Method property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to Convert and the Operand, Type, and Method properties set to the specified values.</returns>
  public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method) =>
    new ConvertWithMethodUnaryExpression(ExpressionType.Convert, expression, type, method);

  /// <summary>Creates a UnaryExpression that represents a conversion operation that throws an exception if the target type is overflowed.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <param name="type">A Type to set the Type property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to ConvertChecked and the Operand and Type properties set to the specified values.</returns>
  public static UnaryExpression ConvertChecked(Expression expression, Type type) =>
    new TypedUnaryExpression(ExpressionType.ConvertChecked, expression, type);

  /// <summary>Creates a UnaryExpression that represents a conversion operation that throws an exception if the target type is overflowed and for which the implementing method is specified.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <param name="type">A Type to set the Type property equal to.</param>
  /// <param name="method">A MethodInfo to set the Method property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to ConvertChecked and the Operand, Type, and Method properties set to the specified values.</returns>
  public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method) =>
    new ConvertWithMethodUnaryExpression(ExpressionType.ConvertChecked, expression, type, method);

  /// <summary>Creates a UnaryExpression that represents the decrementing of the expression by 1.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the decremented expression.</returns>
  public static UnaryExpression Decrement(Expression expression) =>
    new UnaryExpression(ExpressionType.Decrement, expression);

  /// <summary>Creates a UnaryExpression that represents the incrementing of the expression value by 1.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the incremented expression.</returns>
  public static UnaryExpression Increment(Expression expression) =>
    new UnaryExpression(ExpressionType.Increment, expression);

  /// <summary>Returns whether the expression evaluates to false.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>An instance of UnaryExpression.</returns>
  public static UnaryExpression IsFalse(Expression expression) =>
    new TypedUnaryExpression<bool>(ExpressionType.IsFalse, expression);

  /// <summary>Returns whether the expression evaluates to true.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>An instance of UnaryExpression.</returns>
  public static UnaryExpression IsTrue(Expression expression) =>
    new TypedUnaryExpression<bool>(ExpressionType.IsTrue, expression);

  /// <summary>Creates a UnaryExpression, given an operand, by calling the appropriate factory method.</summary>
  /// <param name="unaryType">The ExpressionType that specifies the type of unary operation.</param>
  /// <param name="operand">An Expression that represents the operand.</param>
  /// <param name="type">The Type that specifies the type to be converted to (pass null if not applicable).</param>
  /// <returns>The UnaryExpression that results from calling the appropriate factory method.</returns>
  public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) =>
    type == null
      ? new UnaryExpression(unaryType, operand)
      : new TypedUnaryExpression(unaryType, operand, type);

  /// <summary>Creates a UnaryExpression that represents an arithmetic negation operation.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to Negate and the Operand property set to the specified value.</returns>
  public static UnaryExpression Negate(Expression expression) =>
    new UnaryExpression(ExpressionType.Negate, expression);

  /// <summary>Creates a UnaryExpression that represents an arithmetic negation operation that has overflow checking.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to NegateChecked and the Operand property set to the specified value.</returns>
  public static UnaryExpression NegateChecked(Expression expression) =>
    new UnaryExpression(ExpressionType.NegateChecked, expression);

  /// <summary>Returns the expression representing the ones complement.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>An instance of UnaryExpression.</returns>
  public static UnaryExpression OnesComplement(Expression expression) =>
    new UnaryExpression(ExpressionType.OnesComplement, expression);

  /// <summary>Creates a UnaryExpression that increments the expression by 1 and assigns the result back to the expression.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the resultant expression.</returns>
  public static UnaryExpression PreIncrementAssign(Expression expression) =>
    new UnaryExpression(ExpressionType.PreIncrementAssign, expression);

  /// <summary>Creates a UnaryExpression that represents the assignment of the expression followed by a subsequent increment by 1 of the original expression.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the resultant expression.</returns>
  public static UnaryExpression PostIncrementAssign(Expression expression) =>
    new UnaryExpression(ExpressionType.PostIncrementAssign, expression);

  /// <summary>Creates a UnaryExpression that decrements the expression by 1 and assigns the result back to the expression.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the resultant expression.</returns>
  public static UnaryExpression PreDecrementAssign(Expression expression) =>
    new UnaryExpression(ExpressionType.PreDecrementAssign, expression);

  /// <summary>Creates a UnaryExpression that represents the assignment of the expression followed by a subsequent decrement by 1 of the original expression.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the resultant expression.</returns>
  public static UnaryExpression PostDecrementAssign(Expression expression) =>
    new UnaryExpression(ExpressionType.PostDecrementAssign, expression);

  /// <summary>Creates a UnaryExpression that represents an expression that has a constant value of type Expression.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to Quote and the Operand property set to the specified value.</returns>
  public static UnaryExpression Quote(Expression expression) =>
    new UnaryExpression(ExpressionType.Quote, expression);

  /// <summary>Creates a UnaryExpression that represents a unary plus operation.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to UnaryPlus and the Operand property set to the specified value.</returns>
  public static UnaryExpression UnaryPlus(Expression expression) =>
    new UnaryExpression(ExpressionType.UnaryPlus, expression);

  /// <summary>Creates a UnaryExpression that represents an explicit unboxing.</summary>
  /// <param name="expression">An Expression to set the Operand property equal to.</param>
  /// <param name="type">A Type to set the Type property equal to.</param>
  /// <returns>A UnaryExpression that has the NodeType property equal to unbox and the Operand and Type properties set to the specified values.</returns>
  public static UnaryExpression Unbox(Expression expression, Type type) =>
    new TypedUnaryExpression(ExpressionType.Unbox, expression, type);

  public static LambdaExpression Lambda(Expression body) =>
    new LambdaExpression(Tools.GetFuncOrActionType(Tools.Empty<Type>(), body.Type),
      body, body.Type);

  public static LambdaExpression Lambda(Type delegateType, Expression body) =>
    new LambdaExpression(delegateType, body, body.Type);

  public static LambdaExpression Lambda(Type delegateType, Expression body, Type returnType) =>
    new LambdaExpression(delegateType, body, returnType);

  public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) =>
    Lambda(Tools.GetFuncOrActionType(Tools.GetParamTypes(parameters), body.Type), body, parameters, body.Type);

  public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters) =>
    Lambda(delegateType, body, parameters, GetDelegateReturnType(delegateType));

  public static LambdaExpression Lambda(Type delegateType, Expression body, ParameterExpression[] parameters, Type returnType) =>
    parameters == null || parameters.Length == 0
      ? new LambdaExpression(delegateType, body, returnType)
      : new ManyParametersLambdaExpression(delegateType, body, parameters, returnType);

  public static Expression<TDelegate> Lambda<TDelegate>(Expression body) =>
    new Expression<TDelegate>(body, typeof(TDelegate).FindDelegateInvokeMethod().ReturnType);

  public static Expression<TDelegate> Lambda<TDelegate>(Expression body, Type returnType) =>
    new Expression<TDelegate>(body, returnType);

  public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) =>
    Lambda<TDelegate>(body, parameters, GetDelegateReturnType(typeof(TDelegate)));

  public static Expression<TDelegate> Lambda<TDelegate>(Expression body, ParameterExpression[] parameters, Type returnType) =>
    parameters == null || parameters.Length == 0
      ? new Expression<TDelegate>(body, returnType)
      : new ManyParametersExpression<TDelegate>(body, parameters, returnType);

  /// <summary>
  /// <paramref name="name"/> is ignored for now, the method is just for compatibility with SysExpression
  /// </summary>
  public static Expression<TDelegate> Lambda<TDelegate>(Expression body, string name, params ParameterExpression[] parameters) where TDelegate : class =>
    new ManyParametersExpression<TDelegate>(body, parameters, GetDelegateReturnType(typeof(TDelegate)));

  private static Type GetDelegateReturnType(Type delType)
  {
    var typeInfo = delType.GetTypeInfo();
    if (typeInfo.IsGenericType)
    {
      var typeArguments = typeInfo.GenericTypeArguments;
      var index = typeArguments.Length - 1;
      var typeDef = typeInfo.GetGenericTypeDefinition();
      if (typeDef == FuncTypes[index])
        return typeArguments[index];

      if (typeDef == ActionTypes[index])
        return typeof(void);
    }
    else if (delType == typeof(Action))
      return typeof(void);

    return delType.FindDelegateInvokeMethod().ReturnType;
  }

  internal static readonly Type[] FuncTypes =
  {
    typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>),
    typeof(Func<,,,,,>), typeof(Func<,,,,,,>), typeof(Func<,,,,,,,>)
  };

  internal static readonly Type[] ActionTypes =
  {
    typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>),
    typeof(Action<,,,,>), typeof(Action<,,,,,>), typeof(Action<,,,,,,>)
  };

  /// <summary>Creates a BinaryExpression that represents applying an array index operator to an array of rank one.</summary>
  /// <param name="array">A Expression to set the Left property equal to.</param>
  /// <param name="index">A Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to ArrayIndex and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression ArrayIndex(Expression array, Expression index) =>
    new ArrayIndexExpression(array, index, array.Type.GetElementType());

  public static MemberAssignment Bind(MemberInfo member, Expression expression) =>
    new MemberAssignment(member, expression);

  public static MemberInitExpression MemberInit(NewExpression newExpr, params MemberBinding[] bindings) =>
    new MemberInitExpression(newExpr, bindings);

  /// <summary>Does not present in System Expression. Enables member assignment on existing instance expression.</summary>
  public static MemberInitExpression MemberInit(Expression instanceExpr, params MemberBinding[] assignments) =>
    new MemberInitExpression(instanceExpr, assignments);

  public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers) =>
    new NewArrayExpression(ExpressionType.NewArrayInit, type.MakeArrayType(), initializers);

  public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds) =>
    new NewArrayExpression(ExpressionType.NewArrayBounds,
      bounds.Length == 1 ? type.MakeArrayType() : type.MakeArrayType(bounds.Length),
      bounds);

  /// <summary>Creates a BinaryExpression that represents an assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Assign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Assign(Expression left, Expression right) =>
    new AssignBinaryExpression(left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents raising an expression to a power and assigning the result back to the expression.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to PowerAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression PowerAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.PowerAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an addition assignment operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to AddAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression AddAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.AddAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an addition assignment operation that has overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to AddAssignChecked and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression AddAssignChecked(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.AddAssignChecked, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise AND assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to AndAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression AndAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.AndAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise XOR assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to ExclusiveOrAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.ExclusiveOrAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise left-shift assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to LeftShiftAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression LeftShiftAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.LeftShiftAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a remainder assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to ModuloAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression ModuloAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.ModuloAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise OR assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to OrAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression OrAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.OrAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise right-shift assignment operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to RightShiftAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression RightShiftAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.RightShiftAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a subtraction assignment operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to SubtractAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression SubtractAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.SubtractAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a subtraction assignment operation that has overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to SubtractAssignChecked and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression SubtractAssignChecked(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.SubtractAssignChecked, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a multiplication assignment operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to MultiplyAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression MultiplyAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.MultiplyAssign, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a multiplication assignment operation that has overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to MultiplyAssignChecked and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.MultiplyAssignChecked, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a division assignment operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to DivideAssign and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression DivideAssign(Expression left, Expression right) =>
    new AssignBinaryExpression(ExpressionType.DivideAssign, left, right, left.Type);

  public static InvocationExpression Invoke(LambdaExpression expression, Expression arg0) =>
    new InvocationExpression(expression, new[] { arg0 }, expression.ReturnType);

  public static InvocationExpression Invoke(Expression expression, Expression arg0) =>
    new InvocationExpression(expression, new[] { arg0 },
      (expression as LambdaExpression)?.ReturnType ?? expression.Type.FindDelegateInvokeMethod().ReturnType);

  public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> args) =>
    new InvocationExpression(expression, args.AsReadOnlyList(),
      (expression as LambdaExpression)?.ReturnType ?? expression.Type.FindDelegateInvokeMethod().ReturnType);

  public static InvocationExpression Invoke(Expression lambda, params Expression[] args) =>
    Invoke(lambda, (IEnumerable<Expression>)args);

  public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) =>
    new ConditionalExpression(test, ifTrue, ifFalse);

  public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) =>
    new ConditionalExpression(test, ifTrue, ifFalse, type);

  public static ConditionalExpression IfThen(Expression test, Expression ifTrue) =>
    Condition(test, ifTrue, Empty(), typeof(void));

  public static DefaultExpression Empty() => new DefaultExpression(typeof(void));

  public static DefaultExpression Default(Type type) =>
    type == typeof(void) ? Empty() : new DefaultExpression(type);

  public static ConditionalExpression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse) =>
    Condition(test, ifTrue, ifFalse, typeof(void));

  /// <summary>Creates a BinaryExpression that represents an arithmetic addition operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Add and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Add(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Add, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic addition operation that has overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to AddChecked and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression AddChecked(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.AddChecked, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise XOR operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to ExclusiveOr and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression ExclusiveOr(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.ExclusiveOr, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise left-shift operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to LeftShift and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression LeftShift(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.LeftShift, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic remainder operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Modulo and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Modulo(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Modulo, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise OR operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Or and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Or(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Or, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise right-shift operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to RightShift and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression RightShift(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.RightShift, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic subtraction operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Subtract and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Subtract(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Subtract, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic subtraction operation that has overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to SubtractChecked and the Left, Right, and Method properties set to the specified values.</returns>
  public static BinaryExpression SubtractChecked(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.SubtractChecked, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic multiplication operation that does not have overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Multiply and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Multiply(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Multiply, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic multiplication operation that has overflow checking.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to MultiplyChecked and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression MultiplyChecked(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.MultiplyChecked, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an arithmetic division operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Divide and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Divide(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Divide, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents raising a number to a power.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Power and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Power(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Power, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a bitwise AND operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to And, and the Left and Right properties are set to the specified values.</returns>
  public static BinaryExpression And(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.And, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a conditional AND operation that evaluates the second operand only if the first operand evaluates to true.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to AndAlso and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression AndAlso(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.AndAlso, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a conditional OR operation that evaluates the second operand only if the first operand evaluates to false.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to OrElse and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression OrElse(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.OrElse, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an equality comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Equal and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Equal(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Equal, left, right, typeof(bool));

  /// <summary>Creates a BinaryExpression that represents a "greater than" numeric comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to GreaterThan and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression GreaterThan(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.GreaterThan, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a "greater than or equal" numeric comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to GreaterThanOrEqual and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.GreaterThanOrEqual, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a "less than" numeric comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to LessThan and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression LessThan(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.LessThan, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents a " less than or equal" numeric comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to LessThanOrEqual and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression LessThanOrEqual(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.LessThanOrEqual, left, right, left.Type);

  /// <summary>Creates a BinaryExpression that represents an inequality comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to NotEqual and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression NotEqual(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.NotEqual, left, right, typeof(bool));

  public static BlockExpression Block(params Expression[] expressions) =>
    Block(expressions[expressions.Length - 1].Type, Tools.Empty<ParameterExpression>(), expressions);

  public static BlockExpression Block(IReadOnlyList<Expression> expressions) =>
    Block(Tools.Empty<ParameterExpression>(), expressions);

  public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions) =>
    Block(variables.AsReadOnlyList(), new List<Expression>(expressions));

  public static BlockExpression Block(IReadOnlyList<ParameterExpression> variables, IReadOnlyList<Expression> expressions) =>
    Block(expressions[expressions.Count - 1].Type, variables, expressions);

  public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, params Expression[] expressions) =>
    new BlockExpression(type, variables.AsReadOnlyList(), expressions.AsReadOnlyList());

  public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) =>
    new BlockExpression(type, variables.AsReadOnlyList(), expressions.AsReadOnlyList());

  public static Expression Block(ParameterExpression variable, Expression expression1, Expression expression2) =>
    expression2.NodeType == ExpressionType.Throw || expression1.NodeType == ExpressionType.Throw
      ? (Expression)Block(new[] { variable }, expression1, expression2)
      : new OneVariableTwoExpressionBlockExpression(variable, expression1, expression2);

  /// <summary>
  /// Creates a LoopExpression with the given body and (optional) break target.
  /// </summary>
  /// <param name="body">The body of the loop.</param>
  /// <param name="break">The break target used by the loop body, if required.</param>
  /// <returns>The created LoopExpression.</returns>
  public static LoopExpression Loop(Expression body, LabelTarget @break = null) =>
    @break == null ? new LoopExpression(body, null, null) : new LoopExpression(body, @break, null);

  /// <summary>
  /// Creates a LoopExpression with the given body.
  /// </summary>
  /// <param name="body">The body of the loop.</param>
  /// <param name="break">The break target used by the loop body.</param>
  /// <param name="continue">The continue target used by the loop body.</param>
  /// <returns>The created LoopExpression.</returns>
  public static LoopExpression Loop(Expression body, LabelTarget @break, LabelTarget @continue) =>
    new LoopExpression(body, @break, @continue);

  public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers) =>
    new TryExpression(body, null, handlers);

  public static TryExpression TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers) =>
    new TryExpression(body, @finally, handlers);

  public static TryExpression TryFinally(Expression body, Expression @finally) =>
    new TryExpression(body, @finally, null);

  public static CatchBlock Catch(ParameterExpression variable, Expression body) =>
    new CatchBlock(variable, body, null, variable.Type);

  public static CatchBlock Catch(Type test, Expression body) =>
    new CatchBlock(null, body, null, test);

  /// <summary>Creates a UnaryExpression that represents a throwing of an exception.</summary>
  /// <param name="value">An Expression to set the Operand property equal to.</param>
  /// <returns>A UnaryExpression that represents the exception.</returns>
  public static UnaryExpression Throw(Expression value) => Throw(value, typeof(void));

  /// <summary>Creates a UnaryExpression that represents a throwing of an exception with a given type.</summary>
  /// <param name="value">An Expression to set the Operand property equal to.</param>
  /// <param name="type">The Type of the expression.</param>
  /// <returns>A UnaryExpression that represents the exception.</returns>
  public static UnaryExpression Throw(Expression value, Type type) =>
    new TypedUnaryExpression(ExpressionType.Throw, value, type);

  public static LabelExpression Label(LabelTarget target, Expression defaultValue = null) =>
    new LabelExpression(target, defaultValue);

  public static LabelTarget Label(Type type = null, string name = null) =>
    SysExpr.Label(type ?? typeof(void), name);

  public static LabelTarget Label(string name) =>
    SysExpr.Label(typeof(void), name);

  /// <summary>Creates a BinaryExpression, given the left and right operands, by calling an appropriate factory method.</summary>
  /// <param name="binaryType">The ExpressionType that specifies the type of binary operation.</param>
  /// <param name="left">An Expression that represents the left operand.</param>
  /// <param name="right">An Expression that represents the right operand.</param>
  /// <returns>The BinaryExpression that results from calling the appropriate factory method.</returns>
  public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
  {
    switch (binaryType)
    {
      case ExpressionType.AddAssign:
      case ExpressionType.AddAssignChecked:
      case ExpressionType.AndAssign:
      case ExpressionType.Assign:
      case ExpressionType.DivideAssign:
      case ExpressionType.ExclusiveOrAssign:
      case ExpressionType.LeftShiftAssign:
      case ExpressionType.ModuloAssign:
      case ExpressionType.MultiplyAssign:
      case ExpressionType.MultiplyAssignChecked:
      case ExpressionType.OrAssign:
      case ExpressionType.PowerAssign:
      case ExpressionType.RightShiftAssign:
      case ExpressionType.SubtractAssign:
      case ExpressionType.SubtractAssignChecked:
        return new AssignBinaryExpression(binaryType, left, right, left.Type);
      case ExpressionType.ArrayIndex:
        return ArrayIndex(left, right);
      case ExpressionType.Coalesce:
        return Coalesce(left, right);
      default:
        return new SimpleBinaryExpression(binaryType, left, right, left.Type);
    }
  }

  public static GotoExpression MakeGoto(GotoExpressionKind kind, LabelTarget target, Expression value, Type type = null) =>
    new GotoExpression(kind, target, value, type ?? typeof(void));

  public static GotoExpression Break(LabelTarget target, Expression value = null, Type type = null) =>
    MakeGoto(GotoExpressionKind.Break, target, value, type);

  public static GotoExpression Continue(LabelTarget target, Type type = null) =>
    MakeGoto(GotoExpressionKind.Continue, target, null, type);

  /// <summary>Creates a BinaryExpression that represents a reference equality comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Equal and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression ReferenceEqual(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Equal, left, right, typeof(bool));

  /// <summary>Creates a BinaryExpression that represents a reference inequality comparison.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to NotEqual and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression ReferenceNotEqual(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.NotEqual, left, right, typeof(bool));

  public static GotoExpression Return(LabelTarget target, Expression value = null, Type type = null) =>
    MakeGoto(GotoExpressionKind.Return, target, value, type);

  public static GotoExpression Goto(LabelTarget target, Expression value = null, Type type = null) =>
    MakeGoto(GotoExpressionKind.Goto, target, value, type);

  public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, params SwitchCase[] cases) =>
    new SwitchExpression(defaultBody.Type, switchValue, defaultBody, null, cases);

  public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) =>
    new SwitchExpression(defaultBody.Type, switchValue, defaultBody, comparison, cases);

  public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) =>
    new SwitchExpression(type, switchValue, defaultBody, comparison, cases);

  public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) =>
    new SwitchExpression(type, switchValue, defaultBody, comparison, cases.AsArray());

  public static SwitchExpression Switch(Expression switchValue, params SwitchCase[] cases) =>
    new SwitchExpression(null, switchValue, null, null, cases);

  public static SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues) =>
    new SwitchCase(body, testValues);

  public static SwitchCase SwitchCase(Expression body, params Expression[] testValues) =>
    new SwitchCase(body, testValues);

  /// <summary>Creates a BinaryExpression that represents a coalescing operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Coalesce and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Coalesce(Expression left, Expression right) =>
    new SimpleBinaryExpression(ExpressionType.Coalesce, left, right, GetCoalesceType(left.Type, right.Type));

  /// <summary>Creates a BinaryExpression that represents a coalescing operation.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <param name="type">Result type</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Coalesce and the Left and Right properties set to the specified values.</returns>
  public static BinaryExpression Coalesce(Expression left, Expression right, Type type) =>
    new SimpleBinaryExpression(ExpressionType.Coalesce, left, right, type);

  /// <summary>Creates a BinaryExpression that represents a coalescing operation, given a conversion function.</summary>
  /// <param name="left">An Expression to set the Left property equal to.</param>
  /// <param name="right">An Expression to set the Right property equal to.</param>
  /// <param name="conversion">A LambdaExpression to set the Conversion property equal to.</param>
  /// <returns>A BinaryExpression that has the NodeType property equal to Coalesce and the Left, Right and Conversion properties set to the specified values.</returns>
  public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion) =>
    conversion == null ?
      new SimpleBinaryExpression(ExpressionType.Coalesce, left, right, GetCoalesceType(left.Type, right.Type)) :
      (BinaryExpression)new CoalesceConversionBinaryExpression(left, right, conversion);

  private static Type GetCoalesceType(Type left, Type right)
  {
    var leftTypeInfo = left.GetTypeInfo();
    if (leftTypeInfo.IsGenericType && leftTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
      left = leftTypeInfo.GenericTypeArguments[0];

    if (right == left)
      return left;

    if (leftTypeInfo.IsAssignableFrom(right.GetTypeInfo()) ||
        right.IsImplicitlyBoxingConvertibleTo(left) ||
        right.IsImplicitlyNumericConvertibleTo(left))
      return left;

    if (right.GetTypeInfo().IsAssignableFrom(leftTypeInfo) ||
        left.IsImplicitlyBoxingConvertibleTo(right) ||
        left.IsImplicitlyNumericConvertibleTo(right))
      return right;

    throw new ArgumentException($"Unable to coalesce arguments of left type of {left} and right type of {right}.");
  }
}

internal static class TypeTools
{
  internal static int GetFirstIndex<T>(this IReadOnlyList<T> source, T item)
  {
    if (source.Count != 0)
      for (var i = 0; i < source.Count; ++i)
        if (ReferenceEquals(source[i], item))
          return i;
    return -1;
  }

  internal static bool IsImplicitlyBoxingConvertibleTo(this Type source, Type target) =>
    source.GetTypeInfo().IsValueType &&
    (target == typeof(object) ||
     target == typeof(ValueType)) ||
    source.GetTypeInfo().IsEnum && target == typeof(Enum);

  internal static PropertyInfo FindProperty(this Type type, string propertyName)
  {
    var properties = type.GetTypeInfo().DeclaredProperties.AsArray();
    for (var i = 0; i < properties.Length; i++)
      if (properties[i].Name == propertyName)
        return properties[i];

    return type.GetTypeInfo().BaseType?.FindProperty(propertyName);
  }

  internal static FieldInfo FindField(this Type type, string fieldName)
  {
    var fields = type.GetTypeInfo().DeclaredFields.AsArray();
    for (var i = 0; i < fields.Length; i++)
      if (fields[i].Name == fieldName)
        return fields[i];

    return type.GetTypeInfo().BaseType?.FindField(fieldName);
  }

  internal static MethodInfo FindMethod(this Type type,
    string methodName, Type[] typeArgs, IReadOnlyList<Expression> args, bool isStatic = false)
  {
    var methods = type.GetTypeInfo().DeclaredMethods.AsArray();
    for (var i = 0; i < methods.Length; i++)
    {
      var m = methods[i];
      if (isStatic == m.IsStatic && methodName == m.Name)
      {
        typeArgs = typeArgs ?? Type.EmptyTypes;
        var mTypeArgs = m.GetGenericArguments();

        if (typeArgs.Length == mTypeArgs.Length &&
            (typeArgs.Length == 0 || AreTypesTheSame(typeArgs, mTypeArgs)))
        {
          args = args ?? Tools.Empty<Expression>();
          var pars = m.GetParameters();
          if (args.Count == pars.Length &&
              (args.Count == 0 || AreArgExpressionsAndParamsOfTheSameType(args, pars)))
            return m;
        }
      }
    }

    return type.GetTypeInfo().BaseType?.FindMethod(methodName, typeArgs, args, isStatic);
  }

  private static bool AreTypesTheSame(Type[] source, Type[] target)
  {
    for (var i = 0; i < source.Length; i++)
      if (source[i] != target[i])
        return false;
    return true;
  }

  private static bool AreArgExpressionsAndParamsOfTheSameType(IReadOnlyList<Expression> args, ParameterInfo[] pars)
  {
    for (var i = 0; i < pars.Length; i++)
      if (pars[i].ParameterType != args[i].Type)
        return false;
    return true;
  }

  public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> xs)
  {
    if (xs is IReadOnlyList<T> list)
      return list;
    return xs == null ? null : new List<T>(xs);
  }

  internal static bool IsImplicitlyNumericConvertibleTo(this Type source, Type target)
  {
    if (source == typeof(Char))
      return
        target == typeof(UInt16) ||
        target == typeof(Int32) ||
        target == typeof(UInt32) ||
        target == typeof(Int64) ||
        target == typeof(UInt64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(SByte))
      return
        target == typeof(Int16) ||
        target == typeof(Int32) ||
        target == typeof(Int64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(Byte))
      return
        target == typeof(Int16) ||
        target == typeof(UInt16) ||
        target == typeof(Int32) ||
        target == typeof(UInt32) ||
        target == typeof(Int64) ||
        target == typeof(UInt64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(Int16))
      return
        target == typeof(Int32) ||
        target == typeof(Int64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(UInt16))
      return
        target == typeof(Int32) ||
        target == typeof(UInt32) ||
        target == typeof(Int64) ||
        target == typeof(UInt64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(Int32))
      return
        target == typeof(Int64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(UInt32))
      return
        target == typeof(UInt32) ||
        target == typeof(UInt64) ||
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(Int64) ||
        source == typeof(UInt64))
      return
        target == typeof(Single) ||
        target == typeof(Double) ||
        target == typeof(Decimal);

    if (source == typeof(Single))
      return target == typeof(Double);

    return false;
  }
}


/// <summary>Converts the object of known type into the valid C# code representation</summary>
public static class ExpressionCodePrinter
{
  internal static StringBuilder AppendLineIdent(this StringBuilder sb, int lineIdent) =>
    sb.AppendLine().Append(' ', lineIdent);

  internal static StringBuilder AppendLineIdent(this StringBuilder sb, Expression expr, int lineIdent,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.AppendLineIdent(lineIdent);
    if (expr == null)
      sb.Append("null");
    else
      expr.ToCodeString(sb, lineIdent + identSpaces, stripNamespace, printType, identSpaces);
    return sb;
  }

  internal static StringBuilder AppendLineIdent<T>(this StringBuilder sb, IReadOnlyList<T> exprs, int lineIdent,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2) where T : Expression
  {
    if (exprs.Count == 0)
      return sb.Append("new ").Append(typeof(T).Name).Append("[0]");

    for (var i = 0; i < exprs.Count; i++)
    {
      if (i > 0)
        sb.Append(',');
      sb.AppendLineIdent(exprs[i], lineIdent, stripNamespace, printType, identSpaces);
    }

    return sb;
  }
}

/// <summary>Converts the object of known type into the valid C# code representation</summary>
public static class CodePrinter
{
  /// <summary>Converts the `typeof(<paramref name="type"/>)` into the proper C# representation.</summary>
  public static StringBuilder AppendTypeof(this StringBuilder sb, Type type, bool stripNamespace = false, Func<Type, string, string> printType = null) =>
    type == null
      ? sb.Append("null")
      : sb.Append("typeof(").Append(type.ToCode(stripNamespace, printType)).Append(')');

  /// <summary>Converts the <paramref name="type"/> into the proper C# representation.</summary>
  public static string ToCode(this Type type, bool stripNamespace = false, Func<Type, string, string> printType = null)
  {
    var isArray = type.IsArray;
    if (isArray)
      type = type.GetElementType();

    var typeString = stripNamespace ? type.Name : type.FullName ?? type.Name;

    typeString = typeString.Replace('+', '.');

    var typeInfo = type.GetTypeInfo();
    if (!typeInfo.IsGenericType)
      return printType?.Invoke(type, typeString) ?? typeString;

    var s = new StringBuilder(typeString.Substring(0, typeString.IndexOf('`')));
    s.Append('<');

    var genericArgs = typeInfo.GetGenericTypeParametersOrArguments();
    if (typeInfo.IsGenericTypeDefinition)
      s.Append(',', genericArgs.Length - 1);
    else
    {
      for (var i = 0; i < genericArgs.Length; i++)
      {
        if (i > 0)
          s.Append(", ");
        s.Append(genericArgs[i].ToCode(stripNamespace, printType));
      }
    }

    s.Append('>');

    if (isArray)
      s.Append("[]");

    typeString = s.ToString();
    return printType?.Invoke(type, typeString) ?? typeString;
  }

  /// Prints valid C# Boolean
  public static string ToCode(this bool x) => x ? "true" : "false";

  /// Prints valid C# String escaping the things
  public static string ToCode(this string x) =>
    x == null
      ? "null"
      : $"\"{x.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")}\"";

  /// Prints valid c# Enum literal
  public static string ToEnumValueCode(this Type enumType, object x)
  {
    var enumTypeInfo = enumType.GetTypeInfo();
    if (enumTypeInfo.IsGenericType && enumTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
    {
      if (x == null)
        return "null";
      enumType = GetGenericTypeParametersOrArguments(enumTypeInfo)[0];
    }

    return $"{enumType.ToCode()}.{Enum.GetName(enumType, x)}";
  }

  private static Type[] GetGenericTypeParametersOrArguments(this TypeInfo typeInfo) =>
    typeInfo.IsGenericTypeDefinition ? typeInfo.GenericTypeParameters : typeInfo.GenericTypeArguments;

  public interface IObjectToCode
  {
    string ToCode(object x, bool stripNamespace = false, Func<Type, string, string> printType = null);
  }

  /// Prints many code items as array initializer.
  public static string ToCommaSeparatedCode(this IEnumerable items, IObjectToCode notRecognizedToCode,
    bool stripNamespace = false, Func<Type, string, string> printType = null)
  {
    var s = new StringBuilder();
    var count = 0;
    foreach (var item in items)
    {
      if (count++ != 0)
        s.Append(", ");
      s.Append(item.ToCode(notRecognizedToCode, stripNamespace, printType));
    }
    return s.ToString();
  }

  /// <summary>Prints many code items as array initializer.</summary>
  public static string ToArrayInitializerCode(this IEnumerable items, Type itemType, IObjectToCode notRecognizedToCode,
    bool stripNamespace = false, Func<Type, string, string> printType = null) =>
    $"new {itemType.ToCode(stripNamespace, printType)}[]{{{items.ToCommaSeparatedCode(notRecognizedToCode, stripNamespace, printType)}}}";

  /// <summary>
  /// Prints a valid C# for known <paramref name="x"/>,
  /// otherwise uses passed <paramref name="notRecognizedToCode"/> or falls back to `ToString()`.
  /// </summary>
  public static string ToCode(this object x, IObjectToCode notRecognizedToCode,
    bool stripNamespace = false, Func<Type, string, string> printType = null)
  {
    if (x == null)
      return "null";

    if (x is bool b)
      return b.ToCode();

    if (x is string s)
      return s.ToCode();

    if (x is Type t)
      return t.ToCode(stripNamespace, printType);

    var xTypeInfo = x.GetType().GetTypeInfo();
    if (xTypeInfo.IsEnum)
      return x.GetType().ToEnumValueCode(x);

    if (x is IEnumerable e)
    {
      var elemType = xTypeInfo.IsArray
        ? xTypeInfo.GetElementType()
        : xTypeInfo.GetGenericTypeParametersOrArguments().GetFirst();
      if (elemType != null)
        return e.ToArrayInitializerCode(elemType, notRecognizedToCode);
    }

    if (xTypeInfo.IsPrimitive)
      return x.ToString();

    return notRecognizedToCode?.ToCode(x, stripNamespace, printType) ?? x.ToString();
  }
}

public class UnaryExpression : Expression
{
  public override ExpressionType NodeType { get; }

  public override Type Type => Operand.Type;
  public readonly Expression Operand;

  public virtual MethodInfo Method => null;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    switch (NodeType)
    {
      case ExpressionType.ArrayLength:
        return SysExpr.ArrayLength(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.Convert:
        return SysExpr.Convert(Operand.ToExpression(ref exprsConverted), Type, Method);
      case ExpressionType.Decrement:
        return SysExpr.Decrement(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.Increment:
        return SysExpr.Increment(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.IsFalse:
        return SysExpr.IsFalse(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.IsTrue:
        return SysExpr.IsTrue(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.Negate:
        return SysExpr.Negate(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.NegateChecked:
        return SysExpr.NegateChecked(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.OnesComplement:
        return SysExpr.OnesComplement(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.PostDecrementAssign:
        return SysExpr.PostDecrementAssign(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.PostIncrementAssign:
        return SysExpr.PostIncrementAssign(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.PreDecrementAssign:
        return SysExpr.PreDecrementAssign(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.PreIncrementAssign:
        return SysExpr.PreIncrementAssign(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.Quote:
        return SysExpr.Quote(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.UnaryPlus:
        return SysExpr.UnaryPlus(Operand.ToExpression(ref exprsConverted));
      case ExpressionType.Unbox:
        return SysExpr.Unbox(Operand.ToExpression(ref exprsConverted), Type);
      case ExpressionType.Throw:
        return SysExpr.Throw(Operand.ToExpression(ref exprsConverted), Type);
      default:
        throw new NotSupportedException("Cannot convert Expression to Expression of type " + NodeType);
    }
  }

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    var name = Enum.GetName(typeof(ExpressionType), NodeType);
    sb.Append(name).Append('(');
    sb.AppendLineIdent(Operand, lineIdent, stripNamespace, printType, identSpaces);

    if (NodeType == ExpressionType.Convert ||
        NodeType == ExpressionType.Unbox ||
        NodeType == ExpressionType.Throw)
    {
      sb.Append(',');
      sb.AppendLineIdent(lineIdent).AppendTypeof(Type, stripNamespace, printType);
    }

    if (NodeType == ExpressionType.Convert && Method != null)
    {
      sb.Append(',');
      var methodIndex = Method.DeclaringType.GetTypeInfo().GetDeclaredMethods(Method.Name).AsArray().GetFirstIndex(Method);
      sb.AppendLineIdent(lineIdent).AppendTypeof(Method.DeclaringType, stripNamespace, printType)
        .Append(".GetTypeInfo().GetDeclaredMethods(\"").Append(Method.Name).Append("\")[").Append(methodIndex).Append("]");
    }

    return sb.Append(')');
  }

  public UnaryExpression(ExpressionType nodeType, Expression operand)
  {
    NodeType = nodeType;
    Operand = operand;
  }
}

public class TypedUnaryExpression : UnaryExpression
{
  public override Type Type { get; }

  public TypedUnaryExpression(ExpressionType nodeType, Expression operand, Type type) : base(nodeType, operand) =>
    Type = type;
}

public sealed class TypedUnaryExpression<T> : UnaryExpression
{
  public override Type Type => typeof(T);

  public TypedUnaryExpression(ExpressionType nodeType, Expression operand) : base(nodeType, operand) { }
}

public sealed class ConvertWithMethodUnaryExpression : TypedUnaryExpression
{
  public override MethodInfo Method { get; }
  public override Type Type => Method.ReturnType;

  public ConvertWithMethodUnaryExpression(ExpressionType nodeType, Expression operand, MethodInfo method)
    : base(nodeType, operand, method.ReturnType) =>
    Method = method;

  public ConvertWithMethodUnaryExpression(ExpressionType nodeType, Expression operand, Type type, MethodInfo method)
    : base(nodeType, operand, type) =>
    Method = method;
}

public abstract class BinaryExpression : Expression
{
  public override ExpressionType NodeType { get; }
  public override Type Type { get; }

  public readonly Expression Left, Right;

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    var name = Enum.GetName(typeof(ExpressionType), NodeType);
    sb.Append(name).Append('(');
    sb.AppendLineIdent(Left, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Right, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }

  protected BinaryExpression(ExpressionType nodeType, Expression left, Expression right, Type type)
  {
    NodeType = nodeType;

    Left = left;
    Right = right;

    if (nodeType == ExpressionType.Equal ||
        nodeType == ExpressionType.NotEqual ||
        nodeType == ExpressionType.GreaterThan ||
        nodeType == ExpressionType.GreaterThanOrEqual ||
        nodeType == ExpressionType.LessThan ||
        nodeType == ExpressionType.LessThanOrEqual ||
        nodeType == ExpressionType.And ||
        nodeType == ExpressionType.AndAlso ||
        nodeType == ExpressionType.Or ||
        nodeType == ExpressionType.OrElse)
    {
      Type = typeof(bool);
    }
    else
      Type = type;
  }
}

public class TypeBinaryExpression : Expression
{
  public override ExpressionType NodeType { get; }
  public override Type Type { get; }

  public Type TypeOperand { get; }

  public readonly Expression Expression;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.TypeIs(Expression.ToExpression(ref exprsConverted), TypeOperand);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("TypeIs(");
    sb.AppendLineIdent(Expression, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(lineIdent).AppendTypeof(TypeOperand, stripNamespace, printType);
    return sb.Append(')');
  }

  internal TypeBinaryExpression(ExpressionType nodeType, Expression expression, Type typeOperand)
  {
    NodeType = nodeType;
    Expression = expression;
    Type = typeof(bool);
    TypeOperand = typeOperand;
  }
}

public sealed class SimpleBinaryExpression : BinaryExpression
{
  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    switch (NodeType)
    {
      case ExpressionType.Add:
        return SysExpr.Add(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Subtract:
        return SysExpr.Subtract(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Multiply:
        return SysExpr.Multiply(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Divide:
        return SysExpr.Divide(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Power:
        return SysExpr.Power(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Coalesce:
        return SysExpr.Coalesce(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.And:
        return SysExpr.And(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.AndAlso:
        return SysExpr.AndAlso(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Or:
        return SysExpr.Or(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.OrElse:
        return SysExpr.OrElse(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.Equal:
        return SysExpr.Equal(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.NotEqual:
        return SysExpr.NotEqual(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.GreaterThan:
        return SysExpr.GreaterThan(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.GreaterThanOrEqual:
        return SysExpr.GreaterThanOrEqual(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.LessThan:
        return SysExpr.LessThan(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.LessThanOrEqual:
        return SysExpr.LessThanOrEqual(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      default:
        throw new NotSupportedException($"Not a valid {NodeType} for arithmetic or boolean binary expression.");
    }
  }

  internal SimpleBinaryExpression(ExpressionType nodeType, Expression left, Expression right, Type type)
    : base(nodeType, left, right, type) { }
}

public class CoalesceConversionBinaryExpression : BinaryExpression
{
  public readonly LambdaExpression Conversion;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Coalesce(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted), Conversion.ToLambdaExpression());

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Coalesce(");
    sb.AppendLineIdent(Left, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Right, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Conversion, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }

  internal CoalesceConversionBinaryExpression(Expression left, Expression right, LambdaExpression conversion)
    : base(ExpressionType.Coalesce, left, right, null)
  {
    Conversion = conversion;
  }
}

public sealed class ArrayIndexExpression : BinaryExpression
{
  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.ArrayIndex(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));

  internal ArrayIndexExpression(Expression left, Expression right, Type type)
    : base(ExpressionType.ArrayIndex, left, right, type) { }
}

public sealed class AssignBinaryExpression : BinaryExpression
{
  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    switch (NodeType)
    {
      case ExpressionType.Assign:
        return SysExpr.Assign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.AddAssign:
        return SysExpr.AddAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.AddAssignChecked:
        return SysExpr.AddAssignChecked(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.SubtractAssign:
        return SysExpr.SubtractAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.SubtractAssignChecked:
        return SysExpr.SubtractAssignChecked(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.MultiplyAssign:
        return SysExpr.MultiplyAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.MultiplyAssignChecked:
        return SysExpr.MultiplyAssignChecked(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.DivideAssign:
        return SysExpr.DivideAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.PowerAssign:
        return SysExpr.PowerAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.AndAssign:
        return SysExpr.AndAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      case ExpressionType.OrAssign:
        return SysExpr.OrAssign(Left.ToExpression(ref exprsConverted), Right.ToExpression(ref exprsConverted));
      default:
        throw new NotSupportedException($"Not a valid {NodeType} for Assign binary expression.");
    }
  }

  internal AssignBinaryExpression(Expression left, Expression right, Type type)
    : base(ExpressionType.Assign, left, right, type) { }

  internal AssignBinaryExpression(ExpressionType expressionType, Expression left, Expression right, Type type)
    : base(expressionType, left, right, type) { }
}

public sealed class MemberInitExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.MemberInit;
  public override Type Type => Expression.Type;

  public NewExpression NewExpression => Expression as NewExpression;

  public readonly Expression Expression;
  public readonly IReadOnlyList<MemberBinding> Bindings;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.MemberInit((System.Linq.Expressions.NewExpression)NewExpression.ToExpression(ref exprsConverted),
      BindingsToExpressions(Bindings, ref exprsConverted));

  internal static System.Linq.Expressions.MemberBinding[] BindingsToExpressions(
    IReadOnlyList<MemberBinding> ms, ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    if (ms.Count == 0)
      return Tools.Empty<System.Linq.Expressions.MemberBinding>();

    if (ms.Count == 1)
      return new[] { ms[0].ToMemberBinding(ref exprsConverted) };

    var result = new System.Linq.Expressions.MemberBinding[ms.Count];
    for (var i = 0; i < result.Length; ++i)
      result[i] = ms[i].ToMemberBinding(ref exprsConverted);
    return result;
  }

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("MemberInit(");
    sb.AppendLineIdent(Expression, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(lineIdent);

    for (var i = 0; i < Bindings.Count; i++)
    {
      if (i > 0)
        sb.Append(','); // insert the comma before the 2nd binding
      sb.AppendLineIdent(lineIdent);
      Bindings[i].ToCodeString(sb, lineIdent + identSpaces, stripNamespace, printType, identSpaces);
    }

    return sb.Append(')');
  }

  internal MemberInitExpression(NewExpression newExpression, MemberBinding[] bindings)
    : this((Expression)newExpression, bindings) { }

  internal MemberInitExpression(Expression expression, MemberBinding[] bindings)
  {
    Expression = expression;
    Bindings = bindings ?? Tools.Empty<MemberBinding>();
  }
}

public sealed class ParameterExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Parameter;
  public override Type Type { get; }

  // todo: we need the version without this members
  public readonly string Name;
  public readonly bool IsByRef;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Parameter(IsByRef ? Type.MakeByRefType() : Type, Name);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Parameter(").AppendTypeof(Type, stripNamespace, printType);

    if (IsByRef)
      sb.Append(".MakeByRefType()");

    if (Name != null)
      sb.Append(",\"").Append(Name).Append('"');

    return sb.Append(')');
  }

  internal static System.Linq.Expressions.ParameterExpression[] ToParameterExpressions(
    IReadOnlyList<ParameterExpression> ps, ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    if (ps.Count == 0)
      return Tools.Empty<System.Linq.Expressions.ParameterExpression>();

    if (ps.Count == 1)
      return new[] { (System.Linq.Expressions.ParameterExpression)ps[0].ToExpression(ref exprsConverted) };

    var result = new System.Linq.Expressions.ParameterExpression[ps.Count];
    for (var i = 0; i < result.Length; ++i)
      result[i] = (System.Linq.Expressions.ParameterExpression)ps[i].ToExpression(ref exprsConverted);
    return result;
  }

  internal ParameterExpression(Type type, string name, bool isByRef)
  {
    Type = type;
    Name = name;
    IsByRef = isByRef;
  }
}

public class ConstantExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Constant;
  public override Type Type => Value.GetType();

  public readonly object Value;

  internal ConstantExpression(object value) => Value = value;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> _) => SysExpr.Constant(Value, Type);

  /// <summary>
  /// Change the method to convert the <see cref="Value"/> to code as you want it globally.
  /// You may try to use `ObjectToCode` from `https://www.nuget.org/packages/ExpressionToCodeLib`
  /// </summary>
  public static CodePrinter.IObjectToCode ValueToCode = new ValueToDefault();

  private class ValueToDefault : CodePrinter.IObjectToCode
  {
    public string ToCode(object x, bool stripNamespace = false, Func<Type, string, string> printType = null) =>
      $"default({x.GetType().ToCode(stripNamespace, printType)})";
  }

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Constant(");

    if (Value == null)
    {
      sb.Append("null");
      if (Type != typeof(object))
        sb.Append(',').AppendTypeof(Type, stripNamespace, printType);
    }
    else if (Value is Type t) // todo: move this to ValueToCode, we should output `typeof(T)` anyway
    {
      sb.AppendTypeof(t, stripNamespace, printType);
    }
    else
    {
      sb.Append(Value.ToCode(ValueToCode, stripNamespace, printType));

      if (Value.GetType() != Type)
        sb.Append(',').AppendTypeof(Type, stripNamespace, printType);
    }

    return sb.Append(')');
  }

  /// <summary>I want to see the actual Value not the default one</summary>
  public override string ToString() => $"Constant({Value}, typeof({Type.ToCode()}))";
}


public sealed class TypedConstantExpression : ConstantExpression
{
  public override Type Type { get; }

  internal TypedConstantExpression(object value, Type type) : base(value) => Type = type;
}

public sealed class TypedConstantExpression<T> : ConstantExpression
{
  public override Type Type => typeof(T);

  internal TypedConstantExpression(T value) : base(value) { }
}

public abstract class ArgumentsExpression : Expression
{
  public readonly IReadOnlyList<Expression> Arguments;

  protected ArgumentsExpression(IReadOnlyList<Expression> arguments) => Arguments = arguments ?? Tools.Empty<Expression>();
}

public class NewExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.New;
  public override Type Type => Constructor.DeclaringType;

  public readonly ConstructorInfo Constructor;

  public virtual int FewArgumentCount => 0;
  public virtual IReadOnlyList<Expression> Arguments => Tools.Empty<Expression>();

  internal NewExpression(ConstructorInfo constructor) => Constructor = constructor;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.New(Constructor, ToExpressions(Arguments, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    var args = Arguments;
    sb.Append("New(/*").Append(args.Count).Append(" args*/");
    var ctorIndex = Constructor.DeclaringType.GetTypeInfo().DeclaredConstructors.ToArray().GetFirstIndex(Constructor);
    sb.AppendLineIdent(lineIdent).AppendTypeof(Type, stripNamespace, printType)
      .Append(".GetTypeInfo().DeclaredConstructors.ToArray()[").Append(ctorIndex).Append("],");
    sb.AppendLineIdent(args, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }
}

public sealed class NewValueTypeExpression : NewExpression
{
  public override Type Type { get; }

  internal NewValueTypeExpression(Type type) : base(null) => Type = type;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) => SysExpr.New(Type);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2) =>
    sb.Append("New(").AppendTypeof(Type, stripNamespace, printType).Append(')');
}

public sealed class OneArgumentNewExpression : NewExpression
{
  public readonly Expression Argument;
  public override IReadOnlyList<Expression> Arguments => new[] { Argument };
  public override int FewArgumentCount => 1;

  internal OneArgumentNewExpression(ConstructorInfo constructor, Expression argument) : base(constructor) =>
    Argument = argument;
}

public sealed class TwoArgumentsNewExpression : NewExpression
{
  public readonly Expression Argument0;
  public readonly Expression Argument1;

  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1 };
  public override int FewArgumentCount => 2;

  internal TwoArgumentsNewExpression(ConstructorInfo constructor,
    Expression argument0, Expression argument1) : base(constructor)
  {
    Argument0 = argument0;
    Argument1 = argument1;
  }
}

public sealed class ThreeArgumentsNewExpression : NewExpression
{
  public readonly Expression Argument0;
  public readonly Expression Argument1;
  public readonly Expression Argument2;

  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1, Argument2 };
  public override int FewArgumentCount => 3;

  internal ThreeArgumentsNewExpression(ConstructorInfo constructor,
    Expression argument0, Expression argument1, Expression argument2) : base(constructor)
  {
    Argument0 = argument0;
    Argument1 = argument1;
    Argument2 = argument2;
  }
}

public sealed class FourArgumentsNewExpression : NewExpression
{
  public readonly Expression Argument0;
  public readonly Expression Argument1;
  public readonly Expression Argument2;
  public readonly Expression Argument3;

  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1, Argument2, Argument3 };
  public override int FewArgumentCount => 4;

  internal FourArgumentsNewExpression(ConstructorInfo constructor,
    Expression argument0, Expression argument1, Expression argument2, Expression argument3) : base(constructor)
  {
    Argument0 = argument0;
    Argument1 = argument1;
    Argument2 = argument2;
    Argument3 = argument3;
  }
}

public sealed class FiveArgumentsNewExpression : NewExpression
{
  public readonly Expression Argument0;
  public readonly Expression Argument1;
  public readonly Expression Argument2;
  public readonly Expression Argument3;
  public readonly Expression Argument4;

  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1, Argument2, Argument3, Argument4 };
  public override int FewArgumentCount => 5;

  internal FiveArgumentsNewExpression(ConstructorInfo constructor,
    Expression argument0, Expression argument1, Expression argument2, Expression argument3, Expression argument4) : base(constructor)
  {
    Argument0 = argument0;
    Argument1 = argument1;
    Argument2 = argument2;
    Argument3 = argument3;
    Argument4 = argument4;
  }
}

public sealed class ManyArgumentsNewExpression : NewExpression
{
  public override IReadOnlyList<Expression> Arguments { get; }
  public override int FewArgumentCount => -1;

  internal ManyArgumentsNewExpression(ConstructorInfo constructor, IReadOnlyList<Expression> arguments) : base(constructor) =>
    Arguments = arguments;
}

public sealed class NewArrayExpression : ArgumentsExpression
{
  public override ExpressionType NodeType { get; }
  public override Type Type { get; }

  // I made it a ICollection for now to use Arguments as input, without changing Arguments type
  public IReadOnlyList<Expression> Expressions => Arguments;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    NodeType == ExpressionType.NewArrayInit
      // ReSharper disable once AssignNullToNotNullAttribute
      ? SysExpr.NewArrayInit(Type.GetElementType(), ToExpressions(Arguments, ref exprsConverted))
      // ReSharper disable once AssignNullToNotNullAttribute
      : SysExpr.NewArrayBounds(Type.GetElementType(), ToExpressions(Arguments, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append(NodeType == ExpressionType.NewArrayInit ? "NewArrayInit(" : "NewArrayBounds(");
    sb.AppendLineIdent(lineIdent).AppendTypeof(Type.GetElementType(), stripNamespace, printType);
    sb.AppendLineIdent(Arguments, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }

  internal NewArrayExpression(ExpressionType expressionType, Type arrayType, IReadOnlyList<Expression> elements) : base(elements)
  {
    NodeType = expressionType;
    Type = arrayType;
  }
}

public class MethodCallExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Call;
  public override Type Type => Method.ReturnType;

  public virtual Expression Object => null;
  public virtual IReadOnlyList<Expression> Arguments => Tools.Empty<Expression>();
  public virtual int FewArgumentCount => 0;

  public readonly MethodInfo Method;

  internal MethodCallExpression(MethodInfo method) => Method = method;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Call(Object?.ToExpression(ref exprsConverted), Method,
      ToExpressions(Arguments, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Call(");
    sb.AppendLineIdent(Object, lineIdent, stripNamespace, printType, identSpaces).Append(',');

    var methodIndex = Method.DeclaringType.GetTypeInfo().GetDeclaredMethods(Method.Name).AsArray().GetFirstIndex(Method);
    sb.AppendLineIdent(lineIdent).AppendTypeof(Method.DeclaringType, stripNamespace, printType)
      .Append(".GetTypeInfo().GetDeclaredMethods(\"").Append(Method.Name).Append("\")[").Append(methodIndex).Append("],");

    sb.AppendLineIdent(Arguments, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }
}

public sealed class InstanceMethodCallExpression : MethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceMethodCallExpression(Expression instance, MethodInfo method) : base(method) =>
    Object = instance;
}

public class ManyArgumentsMethodCallExpression : MethodCallExpression
{
  public override IReadOnlyList<Expression> Arguments { get; }
  public override int FewArgumentCount => -1;

  internal ManyArgumentsMethodCallExpression(MethodInfo method, IReadOnlyList<Expression> arguments) : base(method) =>
    Arguments = arguments;
}

public sealed class InstanceManyArgumentsMethodCallExpression : ManyArgumentsMethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceManyArgumentsMethodCallExpression(Expression instance, MethodInfo method, IReadOnlyList<Expression> arguments)
    : base(method, arguments) => Object = instance;
}

public class OneArgumentMethodCallExpression : MethodCallExpression
{
  public override IReadOnlyList<Expression> Arguments => new[] { Argument };
  public override int FewArgumentCount => 1;

  public readonly Expression Argument;

  internal OneArgumentMethodCallExpression(MethodInfo method, Expression argument) : base(method) =>
    Argument = argument;
}

public sealed class InstanceOneArgumentMethodCallExpression : OneArgumentMethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceOneArgumentMethodCallExpression(Expression instance, MethodInfo method, Expression argument)
    : base(method, argument) => Object = instance;
}

public class TwoArgumentsMethodCallExpression : MethodCallExpression
{
  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1 };
  public override int FewArgumentCount => 2;

  public readonly Expression Argument0;
  public readonly Expression Argument1;

  internal TwoArgumentsMethodCallExpression(MethodInfo method, Expression argument0, Expression argument1) : base(method)
  {
    Argument0 = argument0;
    Argument1 = argument1;
  }
}

public sealed class InstanceTwoArgumentsMethodCallExpression : TwoArgumentsMethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceTwoArgumentsMethodCallExpression(Expression instance, MethodInfo method,
    Expression argument0, Expression argument1) : base(method, argument0, argument1) => Object = instance;
}

public class ThreeArgumentsMethodCallExpression : MethodCallExpression
{
  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1, Argument2 };
  public override int FewArgumentCount => 3;

  public readonly Expression Argument0;
  public readonly Expression Argument1;
  public readonly Expression Argument2;

  internal ThreeArgumentsMethodCallExpression(MethodInfo method,
    Expression argument0, Expression argument1, Expression argument2) : base(method)
  {
    Argument0 = argument0;
    Argument1 = argument1;
    Argument2 = argument2;
  }
}

public sealed class InstanceThreeArgumentsMethodCallExpression : ThreeArgumentsMethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceThreeArgumentsMethodCallExpression(Expression instance, MethodInfo method,
    Expression argument0, Expression argument1, Expression argument2)
    : base(method, argument0, argument1, argument2) => Object = instance;
}

public class FourArgumentsMethodCallExpression : MethodCallExpression
{
  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1, Argument2, Argument3 };
  public override int FewArgumentCount => 4;

  public readonly Expression Argument0;
  public readonly Expression Argument1;
  public readonly Expression Argument2;
  public readonly Expression Argument3;

  internal FourArgumentsMethodCallExpression(MethodInfo method,
    Expression argument0, Expression argument1, Expression argument2, Expression argument3) : base(method)
  {
    Argument0 = argument0;
    Argument1 = argument1;
    Argument2 = argument2;
    Argument3 = argument3;
  }
}

public sealed class InstanceFourArgumentsMethodCallExpression : FourArgumentsMethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceFourArgumentsMethodCallExpression(Expression instance, MethodInfo method,
    Expression argument0, Expression argument1, Expression argument2, Expression argument3)
    : base(method, argument0, argument1, argument2, argument3) => Object = instance;
}

public class FiveArgumentsMethodCallExpression : MethodCallExpression
{
  public override IReadOnlyList<Expression> Arguments => new[] { Argument0, Argument1, Argument2, Argument3, Argument4 };
  public override int FewArgumentCount => 5;

  public readonly Expression Argument0;
  public readonly Expression Argument1;
  public readonly Expression Argument2;
  public readonly Expression Argument3;
  public readonly Expression Argument4;

  internal FiveArgumentsMethodCallExpression(MethodInfo method,
    Expression argument0, Expression argument1, Expression argument2, Expression argument3, Expression argument4)
    : base(method)
  {
    Argument0 = argument0;
    Argument1 = argument1;
    Argument2 = argument2;
    Argument3 = argument3;
    Argument4 = argument4;
  }
}

public sealed class InstanceFiveArgumentsMethodCallExpression : FiveArgumentsMethodCallExpression
{
  public override Expression Object { get; }

  internal InstanceFiveArgumentsMethodCallExpression(Expression instance, MethodInfo method,
    Expression argument0, Expression argument1, Expression argument2, Expression argument3, Expression argument4)
    : base(method, argument0, argument1, argument2, argument3, argument4) => Object = instance;
}

public abstract class MemberExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.MemberAccess;
  public readonly MemberInfo Member;
  public readonly Expression Expression;

  protected MemberExpression(Expression expression, MemberInfo member)
  {
    Expression = expression;
    Member = member;
  }
}

// todo: specialize to 2 classes - with and without object expression
public sealed class PropertyExpression : MemberExpression
{
  public override Type Type => PropertyInfo.PropertyType;
  public PropertyInfo PropertyInfo => (PropertyInfo)Member;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Property(Expression?.ToExpression(ref exprsConverted), PropertyInfo);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Property(");
    sb.AppendLineIdent(Expression, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(lineIdent).AppendTypeof(PropertyInfo.DeclaringType, stripNamespace, printType)
      .Append(".GetTypeInfo().GetDeclaredProperty(\"").Append(PropertyInfo.Name).Append("\")");
    return sb.Append(')');
  }

  internal PropertyExpression(Expression instance, PropertyInfo property) :
    base(instance, property)
  { }
}

// todo: specialize to 2 classes - with and without object expression
public sealed class FieldExpression : MemberExpression
{
  public override Type Type => FieldInfo.FieldType;
  public FieldInfo FieldInfo => (FieldInfo)Member;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Field(Expression?.ToExpression(ref exprsConverted), FieldInfo);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Field(");
    sb.AppendLineIdent(Expression, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(lineIdent).AppendTypeof(FieldInfo.DeclaringType, stripNamespace, printType)
      .Append(".GetTypeInfo().GetDeclaredField(\"").Append(FieldInfo.Name).Append("\")");
    return sb.Append(')');
  }

  internal FieldExpression(Expression instance, FieldInfo field)
    : base(instance, field) { }
}

public abstract class MemberBinding
{
  public readonly MemberInfo Member;

  public abstract MemberBindingType BindingType { get; }

  public string CodeString => ToCodeString(new StringBuilder(128), 0).ToString();

  public abstract StringBuilder ToCodeString(StringBuilder sb, int lineIdent,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2);

  internal abstract System.Linq.Expressions.MemberBinding ToMemberBinding(ref LiveCountArray<Expression.LightAndSysExpr> exprsConverted);

  internal MemberBinding(MemberInfo member)
  {
    Member = member;
  }
}

public sealed class MemberAssignment : MemberBinding
{
  public readonly Expression Expression;

  public override MemberBindingType BindingType => MemberBindingType.Assignment;

  internal override System.Linq.Expressions.MemberBinding ToMemberBinding(ref LiveCountArray<Expression.LightAndSysExpr> exprsConverted) =>
    SysExpr.Bind(Member, Expression.ToExpression(ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Bind(");

    if (Member is FieldInfo)
      sb.AppendLineIdent(lineIdent).AppendTypeof(Member.DeclaringType, stripNamespace, printType)
        .Append(".GetTypeInfo().GetDeclaredField(\"").Append(Member.Name).Append("\"),");
    else // or the property to assign
      sb.AppendLineIdent(lineIdent).AppendTypeof(Member.DeclaringType, stripNamespace, printType)
        .Append(".GetTypeInfo().GetDeclaredProperty(\"").Append(Member.Name).Append("\"),");

    sb.AppendLineIdent(Expression, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(")");
  }

  internal MemberAssignment(MemberInfo member, Expression expression) : base(member)
  {
    Expression = expression;
  }
}

// todo: Split into the single argument and no arguments overloads
public sealed class InvocationExpression : ArgumentsExpression
{
  public override ExpressionType NodeType => ExpressionType.Invoke;
  public override Type Type { get; }

  public readonly Expression Expression;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Invoke(Expression.ToExpression(ref exprsConverted), ToExpressions(Arguments, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Invoke(");
    sb.AppendLineIdent(Expression, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Arguments, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(")");
  }

  internal InvocationExpression(Expression expression, IReadOnlyList<Expression> arguments, Type type) : base(arguments)
  {
    Expression = expression;
    Type = type;
  }
}

public sealed class DefaultExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Default;
  public override Type Type { get; }

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    Type == typeof(void) ? SysExpr.Empty() : SysExpr.Default(Type);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2) =>
    Type == typeof(void) ? sb.Append("Empty()") : sb.Append("Default(").AppendTypeof(Type, stripNamespace, printType).Append(')');

  internal DefaultExpression(Type type) => Type = type;
}

public sealed class ConditionalExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Conditional;
  public override Type Type => _type ?? IfTrue.Type;

  public readonly Expression Test;
  public readonly Expression IfTrue;
  public readonly Expression IfFalse;
  private readonly Type _type;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    _type == null
      ? SysExpr.Condition(Test.ToExpression(ref exprsConverted), IfTrue.ToExpression(ref exprsConverted), IfFalse.ToExpression(ref exprsConverted))
      : SysExpr.Condition(Test.ToExpression(ref exprsConverted), IfTrue.ToExpression(ref exprsConverted), IfFalse.ToExpression(ref exprsConverted), _type);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Condition(");
    sb.AppendLineIdent(Test, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(IfTrue, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(IfFalse, lineIdent, stripNamespace, printType, identSpaces);

    if (_type != null)
      sb.Append(',').AppendLineIdent(lineIdent).AppendTypeof(_type, stripNamespace, printType);

    return sb.Append(')');
  }

  internal ConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse, Type type = null)
  {
    Test = test;
    IfTrue = ifTrue;
    IfFalse = ifFalse;
    _type = type;
  }
}

/// <summary>For indexer property or array access.</summary>
public sealed class IndexExpression : ArgumentsExpression
{
  public override ExpressionType NodeType => ExpressionType.Index;
  public override Type Type => Indexer != null ? Indexer.PropertyType : Object.Type.GetElementType();

  public readonly Expression Object;
  public readonly PropertyInfo Indexer;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.MakeIndex(Object.ToExpression(ref exprsConverted), Indexer, ToExpressions(Arguments, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("MakeIndex(");
    sb.AppendLineIdent(Object, lineIdent, stripNamespace, printType, identSpaces);

    var propIndex = Indexer.DeclaringType.GetTypeInfo().DeclaredProperties.AsArray().GetFirstIndex(Indexer);
    sb.AppendLineIdent(lineIdent).AppendTypeof(Indexer.DeclaringType)
      .Append(".GetTypeInfo().DeclaredProperties.ToArray()[").Append(propIndex).Append("],");

    sb.AppendLineIdent(Arguments, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }

  internal IndexExpression(Expression @object, PropertyInfo indexer, IReadOnlyList<Expression> arguments)
    : base(arguments)
  {
    Object = @object;
    Indexer = indexer;
  }
}

/// <summary>Optimized version for the specific block structure</summary> 
public sealed class OneVariableTwoExpressionBlockExpression : Expression
{
  public static explicit operator BlockExpression(OneVariableTwoExpressionBlockExpression x) =>
    Block(new[] { x.Variable }, x.Expression1, x.Expression2);

  public override ExpressionType NodeType => ExpressionType.Block;
  public override Type Type => Expression2.Type;

  public new readonly ParameterExpression Variable;
  public readonly Expression Expression1;
  public readonly Expression Expression2;
  public Expression Result => Expression2;

  internal OneVariableTwoExpressionBlockExpression(ParameterExpression variable, Expression expression1, Expression expression2)
  {
    Variable = variable;
    Expression1 = expression1;
    Expression2 = expression2;
  }

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    ((BlockExpression)this).CreateSysExpression(ref exprsConverted);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2) =>
    ((BlockExpression)this).ToCodeString(sb, lineIdent, stripNamespace, printType, identSpaces);
}

public sealed class BlockExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Block;
  public override Type Type { get; }

  public readonly IReadOnlyList<ParameterExpression> Variables;
  public readonly IReadOnlyList<Expression> Expressions;
  public Expression Result => Expressions[Expressions.Count - 1];

  internal BlockExpression(Type type, IReadOnlyList<ParameterExpression> variables, IReadOnlyList<Expression> expressions)
  {
    Variables = variables ?? Tools.Empty<ParameterExpression>();
    Expressions = expressions ?? Tools.Empty<Expression>();
    Type = type;
  }

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Block(Type,
      ParameterExpression.ToParameterExpressions(Variables, ref exprsConverted),
      ToExpressions(Expressions, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Block(");
    sb.AppendLineIdent(lineIdent).AppendTypeof(Type, stripNamespace, printType).Append(',');

    if (Variables.Count == 0)
      sb.AppendLineIdent(lineIdent).Append("new ParameterExpression[0],");
    else
    {
      sb.AppendLineIdent(lineIdent).Append("new ParameterExpression[]{");
      sb.AppendLineIdent(Variables, lineIdent, stripNamespace, printType, identSpaces);
      sb.AppendLineIdent(lineIdent).Append("},");
    }

    sb.AppendLineIdent(Expressions, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }
}

public sealed class LoopExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Loop;

  public override Type Type => typeof(void);

  public readonly Expression Body;
  public readonly LabelTarget BreakLabel;
  public readonly LabelTarget ContinueLabel;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    BreakLabel == null ? SysExpr.Loop(Body.ToExpression(ref exprsConverted)) :
    ContinueLabel == null ? SysExpr.Loop(Body.ToExpression(ref exprsConverted), BreakLabel) :
    SysExpr.Loop(Body.ToExpression(ref exprsConverted), BreakLabel, ContinueLabel);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Loop(");
    sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces);

    if (BreakLabel != null)
    {
      sb.Append(',');
      sb.AppendLineIdent(lineIdent).Append("Label(\"break\")");
    }

    if (ContinueLabel != null)
    {
      sb.Append(',');
      sb.AppendLineIdent(lineIdent).Append("Label(\"continue\")");
    }

    return sb.Append(')');
  }

  internal LoopExpression(Expression body, LabelTarget breakLabel, LabelTarget continueLabel)
  {
    Body = body;
    BreakLabel = breakLabel;
    ContinueLabel = continueLabel;
  }
}

public sealed class TryExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Try;
  public override Type Type => Body.Type;

  public readonly Expression Body;
  public IReadOnlyList<CatchBlock> Handlers => _handlers;
  private readonly CatchBlock[] _handlers;
  public readonly Expression Finally;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    Finally == null ?
      SysExpr.TryCatch(Body.ToExpression(ref exprsConverted),
        ToCatchBlocks(_handlers, ref exprsConverted)) :
      Handlers == null ?
        SysExpr.TryFinally(Body.ToExpression(ref exprsConverted),
          Finally.ToExpression(ref exprsConverted)) :
        SysExpr.TryCatchFinally(Body.ToExpression(ref exprsConverted),
          Finally.ToExpression(ref exprsConverted), ToCatchBlocks(_handlers, ref exprsConverted));

  private static System.Linq.Expressions.CatchBlock ToCatchBlock(
    ref CatchBlock cb, ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.MakeCatchBlock(cb.Test,
      (System.Linq.Expressions.ParameterExpression)cb.Variable?.ToExpression(ref exprsConverted),
      cb.Body.ToExpression(ref exprsConverted),
      cb.Filter?.ToExpression(ref exprsConverted));

  private static System.Linq.Expressions.CatchBlock[] ToCatchBlocks(
    CatchBlock[] hs, ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    if (hs == null)
      return Tools.Empty<System.Linq.Expressions.CatchBlock>();
    var catchBlocks = new System.Linq.Expressions.CatchBlock[hs.Length];
    for (var i = 0; i < hs.Length; ++i)
      catchBlocks[i] = ToCatchBlock(ref hs[i], ref exprsConverted);
    return catchBlocks;
  }

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    if (Finally == null)
    {
      sb.Append("TryCatch(");
      sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces).Append(',');
      ToCatchBlocksCode(Handlers, sb, lineIdent, stripNamespace, printType, identSpaces);
    }
    else if (Handlers == null)
    {
      sb.Append("TryFinally(");
      sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces).Append(',');
      sb.AppendLineIdent(Finally, lineIdent, stripNamespace, printType, identSpaces);
    }
    else
    {
      sb.Append("TryCatchFinally(");
      sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces).Append(',');
      ToCatchBlocksCode(Handlers, sb, lineIdent, stripNamespace, printType, identSpaces).Append(',');
      sb.AppendLineIdent(Finally, lineIdent, stripNamespace, printType, identSpaces);
    }

    return sb.Append(')');
  }

  private static StringBuilder ToCatchBlocksCode(IReadOnlyList<CatchBlock> hs, StringBuilder sb, int lineIdent,
    bool stripNamespace, Func<Type, string, string> printType, int identSpaces)
  {
    if (hs.Count == 0)
      return sb.Append("new CatchBlock[0]");

    for (var i = 0; i < hs.Count; i++)
    {
      if (i > 0)
        sb.Append(',');
      sb.AppendLineIdent(lineIdent);
      hs[i].ToCodeString(sb, lineIdent + identSpaces, stripNamespace, printType, identSpaces);
    }

    return sb;
  }

  internal TryExpression(Expression body, Expression @finally, CatchBlock[] handlers)
  {
    Body = body;
    _handlers = handlers;
    Finally = @finally;
  }
}

public struct CatchBlock
{
  public readonly ParameterExpression Variable;
  public readonly Expression Body;
  public readonly Expression Filter;
  public readonly Type Test;

  internal CatchBlock(ParameterExpression variable, Expression body, Expression filter, Type test)
  {
    Variable = variable;
    Body = body;
    Filter = filter;
    Test = test;
  }

  internal StringBuilder ToCodeString(StringBuilder sb, int lineIdent,
    bool stripNamespace, Func<Type, string, string> printType, int identSpaces)
  {
    sb.Append("MakeCatchBlock(");
    sb.AppendLineIdent(lineIdent).AppendTypeof(Test, stripNamespace, printType).Append(',');
    sb.AppendLineIdent(Variable, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Filter, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }
}

public sealed class LabelExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Label;
  public override Type Type => Target.Type;

  // todo: Introduce proper LabelTarget instead of system one
  public readonly LabelTarget Target;
  public readonly Expression DefaultValue;

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    DefaultValue == null ? SysExpr.Label(Target) : SysExpr.Label(Target, DefaultValue.ToExpression(ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Label(\"").Append(Target).Append('"');

    if (DefaultValue != null)
    {
      sb.Append(',');
      sb.AppendLineIdent(DefaultValue, lineIdent, stripNamespace, printType, identSpaces);
    }

    return sb.Append(')');
  }

  internal LabelExpression(LabelTarget target, Expression defaultValue)
  {
    Target = target;
    DefaultValue = defaultValue;
  }
}

public sealed class GotoExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Goto;
  public override Type Type { get; }

  public readonly Expression Value;
  public readonly LabelTarget Target;
  public readonly GotoExpressionKind Kind;

  internal GotoExpression(GotoExpressionKind kind, LabelTarget target, Expression value, Type type)
  {
    Type = type;
    Kind = kind;
    Value = value;
    Target = target;
  }

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    Value == null
      ? SysExpr.Goto(Target, Type)
      : SysExpr.Goto(Target, Value.ToExpression(ref exprsConverted), Type);

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("MakeGoto(");
    sb.Append(nameof(GotoExpressionKind)).Append('.').Append(Enum.GetName(typeof(GotoExpressionKind), Kind)).Append(',');
    sb.AppendLineIdent(lineIdent).Append('"').Append(Target).Append("\",");
    sb.AppendLineIdent(Value, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(lineIdent).AppendTypeof(Type, stripNamespace, printType);
    return sb.Append(')');
  }
}

public struct SwitchCase
{
  public readonly IReadOnlyList<Expression> TestValues;
  public readonly Expression Body;

  public SwitchCase(Expression body, IEnumerable<Expression> testValues)
  {
    Body = body;
    TestValues = testValues.AsReadOnlyList();
  }

  internal StringBuilder ToCodeString(StringBuilder sb, int lineIdent,
    bool stripNamespace, Func<Type, string, string> printType, int identSpaces)
  {
    sb.Append("SwitchCase(");
    sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(TestValues, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }
}

public class SwitchExpression : Expression
{
  public override ExpressionType NodeType { get; }
  public override Type Type { get; }

  public readonly Expression SwitchValue;
  public IReadOnlyList<SwitchCase> Cases => _cases;
  private readonly SwitchCase[] _cases;
  public readonly Expression DefaultBody;
  public readonly MethodInfo Comparison;

  public SwitchExpression(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, SwitchCase[] cases)
  {
    NodeType = ExpressionType.Switch;
    Type = type;
    SwitchValue = switchValue;
    DefaultBody = defaultBody;
    Comparison = comparison;
    _cases = cases;
  }

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Switch(SwitchValue.ToExpression(ref exprsConverted),
      DefaultBody.ToExpression(ref exprsConverted), Comparison,
      ToSwitchCaseExpressions(_cases, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Switch(");
    sb.AppendLineIdent(SwitchValue, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(DefaultBody, lineIdent, stripNamespace, printType, identSpaces).Append(',');

    var methodIndex = Comparison.DeclaringType.GetTypeInfo().DeclaredMethods.AsArray().GetFirstIndex(Comparison);
    sb.AppendLineIdent(lineIdent).AppendTypeof(Comparison.DeclaringType, stripNamespace, printType)
      .Append(".GetTypeInfo().GetDeclaredMethods(\"").Append(Comparison.Name).Append("\"[")
      .Append(methodIndex).Append("],");

    ToCodeString(_cases, sb, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }

  internal static StringBuilder ToCodeString(IReadOnlyList<SwitchCase> items, StringBuilder sb,
    int lineIdent, bool stripNamespace, Func<Type, string, string> printType, int identSpaces)
  {
    if (items.Count == 0)
      return sb.Append("new SwitchCase[0]");

    for (var i = 0; i < items.Count; i++)
    {
      if (i > 0)
        sb.Append(',');
      sb.AppendLineIdent(lineIdent);
      items[i].ToCodeString(sb, lineIdent, stripNamespace, printType, identSpaces);
    }

    return sb;
  }

  internal static System.Linq.Expressions.SwitchCase ToSwitchCase(ref SwitchCase sw, ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.SwitchCase(sw.Body.ToExpression(ref exprsConverted), ToExpressions(sw.TestValues, ref exprsConverted));

  internal static System.Linq.Expressions.SwitchCase[] ToSwitchCaseExpressions(
    SwitchCase[] sw, ref LiveCountArray<LightAndSysExpr> exprsConverted)
  {
    if (sw.Length == 0)
      return Tools.Empty<System.Linq.Expressions.SwitchCase>();

    var result = new System.Linq.Expressions.SwitchCase[sw.Length];
    for (var i = 0; i < result.Length; ++i)
      result[i] = ToSwitchCase(ref sw[i], ref exprsConverted);
    return result;
  }
}

public class LambdaExpression : Expression
{
  public override ExpressionType NodeType => ExpressionType.Lambda;
  public override Type Type { get; }

  public readonly Type ReturnType;
  public readonly Expression Body;
  public virtual IReadOnlyList<ParameterExpression> Parameters => Tools.Empty<ParameterExpression>();

  public System.Linq.Expressions.LambdaExpression ToLambdaExpression() =>
    (System.Linq.Expressions.LambdaExpression)ToExpression();

  internal override SysExpr CreateSysExpression(ref LiveCountArray<LightAndSysExpr> exprsConverted) =>
    SysExpr.Lambda(Type, Body.ToExpression(ref exprsConverted), ParameterExpression.ToParameterExpressions(Parameters, ref exprsConverted));

  public override StringBuilder ToCodeString(StringBuilder sb, int lineIdent = 0,
    bool stripNamespace = false, Func<Type, string, string> printType = null, int identSpaces = 2)
  {
    sb.Append("Lambda(/*$*/"); // bookmark the lambdas - $ means it casts something
    sb.AppendLineIdent(lineIdent).AppendTypeof(Type, stripNamespace, printType).Append(',');
    sb.AppendLineIdent(Body, lineIdent, stripNamespace, printType, identSpaces).Append(',');
    sb.AppendLineIdent(Parameters, lineIdent, stripNamespace, printType, identSpaces);
    return sb.Append(')');
  }

  internal LambdaExpression(Type delegateType, Expression body, Type returnType)
  {
    Body = body;
    ReturnType = returnType;

    if (delegateType != null && delegateType != typeof(Delegate))
      Type = delegateType;
    else
      Type = Tools.GetFuncOrActionType(Tools.Empty<Type>(), ReturnType);
  }
}

public sealed class ManyParametersLambdaExpression : LambdaExpression
{
  public override IReadOnlyList<ParameterExpression> Parameters { get; }

  internal ManyParametersLambdaExpression(Type delegateType, Expression body, IReadOnlyList<ParameterExpression> parameters, Type returnType)
    : base(delegateType, body, returnType) => Parameters = parameters;
}

public class Expression<TDelegate> : LambdaExpression
{
  public new System.Linq.Expressions.Expression<TDelegate> ToLambdaExpression()
  {
    var exprsConverted = new LiveCountArray<LightAndSysExpr>(Tools.Empty<LightAndSysExpr>());
    return SysExpr.Lambda<TDelegate>(Body.ToExpression(ref exprsConverted),
      ParameterExpression.ToParameterExpressions(Parameters, ref exprsConverted));
  }

  internal Expression(Expression body, Type returnType)
    : base(typeof(TDelegate), body, returnType) { }
}

public sealed class ManyParametersExpression<TDelegate> : Expression<TDelegate>
{
  public override IReadOnlyList<ParameterExpression> Parameters { get; }

  internal ManyParametersExpression(Expression body, IReadOnlyList<ParameterExpression> parameters, Type returnType)
    : base(body, returnType) => Parameters = parameters;
}