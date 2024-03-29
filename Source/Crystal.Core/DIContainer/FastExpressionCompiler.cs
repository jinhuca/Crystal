﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace FastExpressionCompiler.LightExpression;

/// <summary>
/// Compiles expression to delegate ~20 times faster than Expression.Compile.
/// Partial to extend with your things when used as source file.
/// </summary>
public static class ExpressionCompiler
{
  #region Expression.CompileFast overloads for Delegate, Func, and Action

  /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static TDelegate CompileFast<TDelegate>(this LambdaExpression lambdaExpr,
    bool ifFastFailedReturnNull = false) where TDelegate : class =>
    (TDelegate)(TryCompileBoundToFirstClosureParam(typeof(TDelegate) == typeof(Delegate) ? lambdaExpr.Type : typeof(TDelegate),
                  lambdaExpr.Body, lambdaExpr.Parameters, GetClosureTypeToParamTypes(lambdaExpr.Parameters), lambdaExpr.ReturnType)
                ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys()));

  /// Compiles a static method to the passed IL Generator.
  /// Could be used as alternative for `CompileToMethod` like this <code><![CDATA[funcExpr.CompileFastToIL(methodBuilder.GetILGenerator())]]></code>.
  /// Check `IssueTests.Issue179_Add_something_like_LambdaExpression_CompileToMethod.cs` for example.
  public static bool CompileFastToIL(this LambdaExpression lambdaExpr, ILGenerator il, bool ifFastFailedReturnNull = false)
  {
    var closureInfo = new ClosureInfo(ClosureStatus.ShouldBeStaticMethod);

    var parentFlags = lambdaExpr.ReturnType == typeof(void) ? ParentFlags.IgnoreResult : ParentFlags.Empty;
    if (!EmittingVisitor.TryEmit(lambdaExpr.Body, lambdaExpr.Parameters, il, ref closureInfo, parentFlags))
      return false;

    il.Emit(OpCodes.Ret);
    return true;
  }

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Delegate CompileFast(this LambdaExpression lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Delegate)TryCompileBoundToFirstClosureParam(lambdaExpr.Type, lambdaExpr.Body, lambdaExpr.Parameters,
      GetClosureTypeToParamTypes(lambdaExpr.Parameters), lambdaExpr.ReturnType)
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Unifies Compile for System.Linq.Expressions and FEC.LightExpression</summary>
  public static TDelegate CompileSys<TDelegate>(this Expression<TDelegate> lambdaExpr) where TDelegate : class =>
    lambdaExpr
      .ToLambdaExpression()
      .Compile();

  /// <summary>Unifies Compile for System.Linq.Expressions and FEC.LightExpression</summary>
  public static Delegate CompileSys(this LambdaExpression lambdaExpr) =>
    lambdaExpr
      .ToLambdaExpression()
      .Compile();

  /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static TDelegate CompileFast<TDelegate>(this Expression<TDelegate> lambdaExpr,
    bool ifFastFailedReturnNull = false)
    where TDelegate : class => ((LambdaExpression)lambdaExpr).CompileFast<TDelegate>(ifFastFailedReturnNull);

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<R> CompileFast<R>(this Expression<Func<R>> lambdaExpr,
    bool ifFastFailedReturnNull = false) =>
    (Func<R>)TryCompileBoundToFirstClosureParam(typeof(Func<R>),
      lambdaExpr.Body, lambdaExpr.Parameters, _closureAsASingleParamType, typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<T1, R> CompileFast<T1, R>(this Expression<Func<T1, R>> lambdaExpr,
    bool ifFastFailedReturnNull = false) =>
    (Func<T1, R>)TryCompileBoundToFirstClosureParam(typeof(Func<T1, R>),
      lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(ArrayClosure), typeof(T1) }, typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<T1, T2, R> CompileFast<T1, T2, R>(this Expression<Func<T1, T2, R>> lambdaExpr,
    bool ifFastFailedReturnNull = false) =>
    (Func<T1, T2, R>)TryCompileBoundToFirstClosureParam(typeof(Func<T1, T2, R>),
      lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(ArrayClosure), typeof(T1), typeof(T2) },
      typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<T1, T2, T3, R> CompileFast<T1, T2, T3, R>(
    this Expression<Func<T1, T2, T3, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Func<T1, T2, T3, R>)TryCompileBoundToFirstClosureParam(typeof(Func<T1, T2, T3, R>),
      lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3) }, typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<T1, T2, T3, T4, R> CompileFast<T1, T2, T3, T4, R>(
    this Expression<Func<T1, T2, T3, T4, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Func<T1, T2, T3, T4, R>)TryCompileBoundToFirstClosureParam(typeof(Func<T1, T2, T3, T4, R>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<T1, T2, T3, T4, T5, R> CompileFast<T1, T2, T3, T4, T5, R>(
    this Expression<Func<T1, T2, T3, T4, T5, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Func<T1, T2, T3, T4, T5, R>)TryCompileBoundToFirstClosureParam(typeof(Func<T1, T2, T3, T4, T5, R>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Func<T1, T2, T3, T4, T5, T6, R> CompileFast<T1, T2, T3, T4, T5, T6, R>(
    this Expression<Func<T1, T2, T3, T4, T5, T6, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Func<T1, T2, T3, T4, T5, T6, R>)TryCompileBoundToFirstClosureParam(typeof(Func<T1, T2, T3, T4, T5, T6, R>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(R))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action CompileFast(this Expression<Action> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Action)TryCompileBoundToFirstClosureParam(typeof(Action),
      lambdaExpr.Body, lambdaExpr.Parameters, _closureAsASingleParamType, typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action<T1> CompileFast<T1>(this Expression<Action<T1>> lambdaExpr,
    bool ifFastFailedReturnNull = false) =>
    (Action<T1>)TryCompileBoundToFirstClosureParam(typeof(Action<T1>),
      lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(ArrayClosure), typeof(T1) }, typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action<T1, T2> CompileFast<T1, T2>(this Expression<Action<T1, T2>> lambdaExpr,
    bool ifFastFailedReturnNull = false) =>
    (Action<T1, T2>)TryCompileBoundToFirstClosureParam(typeof(Action<T1, T2>),
      lambdaExpr.Body, lambdaExpr.Parameters, new[] { typeof(ArrayClosure), typeof(T1), typeof(T2) },
      typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action<T1, T2, T3> CompileFast<T1, T2, T3>(this Expression<Action<T1, T2, T3>> lambdaExpr,
    bool ifFastFailedReturnNull = false) =>
    (Action<T1, T2, T3>)TryCompileBoundToFirstClosureParam(typeof(Action<T1, T2, T3>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3) }, typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action<T1, T2, T3, T4> CompileFast<T1, T2, T3, T4>(
    this Expression<Action<T1, T2, T3, T4>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Action<T1, T2, T3, T4>)TryCompileBoundToFirstClosureParam(typeof(Action<T1, T2, T3, T4>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action<T1, T2, T3, T4, T5> CompileFast<T1, T2, T3, T4, T5>(
    this Expression<Action<T1, T2, T3, T4, T5>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Action<T1, T2, T3, T4, T5>)TryCompileBoundToFirstClosureParam(typeof(Action<T1, T2, T3, T4, T5>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
  public static Action<T1, T2, T3, T4, T5, T6> CompileFast<T1, T2, T3, T4, T5, T6>(
    this Expression<Action<T1, T2, T3, T4, T5, T6>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
    (Action<T1, T2, T3, T4, T5, T6>)TryCompileBoundToFirstClosureParam(typeof(Action<T1, T2, T3, T4, T5, T6>),
      lambdaExpr.Body, lambdaExpr.Parameters,
      new[] { typeof(ArrayClosure), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(void))
    ?? (ifFastFailedReturnNull ? null : lambdaExpr.CompileSys());

  #endregion

  /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/></summary>
  public static TDelegate TryCompile<TDelegate>(this LambdaExpression lambdaExpr) where TDelegate : class =>
    (TDelegate)TryCompileBoundToFirstClosureParam(typeof(TDelegate) == typeof(Delegate) ? lambdaExpr.Type : typeof(TDelegate),
      lambdaExpr.Body, lambdaExpr.Parameters, GetClosureTypeToParamTypes(lambdaExpr.Parameters), lambdaExpr.ReturnType);

  /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/> 
  /// with the provided closure object and constant expressions (or lack there of) -
  /// Constant expression should be the in order of Fields in closure object!
  /// Note 1: Use it on your own risk - FEC won't verify the expression is compile-able with passed closure, it is up to you!
  /// Note 2: The expression with NESTED LAMBDA IS NOT SUPPORTED!
  /// Note 3: `Label` and `GoTo` are not supported in this case, because they need first round to collect out-of-order labels</summary>
  public static TDelegate TryCompileWithPreCreatedClosure<TDelegate>(this LambdaExpression lambdaExpr,
    params ConstantExpression[] closureConstantsExprs)
    where TDelegate : class
  {
    var closureConstants = new object[closureConstantsExprs.Length];
    for (var i = 0; i < closureConstants.Length; i++)
      closureConstants[i] = closureConstantsExprs[i].Value;

    var closureInfo = new ClosureInfo(ClosureStatus.UserProvided | ClosureStatus.HasClosure, closureConstants);
    return TryCompileWithPreCreatedClosure<TDelegate>(lambdaExpr, ref closureInfo);
  }

  internal static TDelegate TryCompileWithPreCreatedClosure<TDelegate>(this LambdaExpression lambdaExpr,
    ref ClosureInfo closureInfo)
    where TDelegate : class
  {
    var closurePlusParamTypes = GetClosureTypeToParamTypes(lambdaExpr.Parameters);
    var method = new DynamicMethod(string.Empty, lambdaExpr.ReturnType, closurePlusParamTypes,
      typeof(ExpressionCompiler), skipVisibility: true);

    var il = method.GetILGenerator();

    EmittingVisitor.EmitLoadConstantsAndNestedLambdasIntoVars(il, ref closureInfo);

    var parentFlags = lambdaExpr.ReturnType == typeof(void) ? ParentFlags.IgnoreResult : ParentFlags.Empty;
    if (!EmittingVisitor.TryEmit(lambdaExpr.Body, lambdaExpr.Parameters, il, ref closureInfo, parentFlags))
      return null;
    il.Emit(OpCodes.Ret);

    var delegateType = typeof(TDelegate) != typeof(Delegate) ? typeof(TDelegate) : lambdaExpr.Type;
    var @delegate = (TDelegate)(object)method.CreateDelegate(delegateType, new ArrayClosure(closureInfo.Constants.Items));
    ReturnClosureTypeToParamTypesToPool(closurePlusParamTypes);
    return @delegate;
  }

  /// <summary>Tries to compile expression to "static" delegate, skipping the step of collecting the closure object.</summary>
  public static TDelegate TryCompileWithoutClosure<TDelegate>(this LambdaExpression lambdaExpr)
    where TDelegate : class
  {
    var closureInfo = new ClosureInfo(ClosureStatus.UserProvided);
    var closurePlusParamTypes = GetClosureTypeToParamTypes(lambdaExpr.Parameters);

    var method = new DynamicMethod(string.Empty, lambdaExpr.ReturnType, closurePlusParamTypes,
      typeof(ArrayClosure), skipVisibility: true);

    var il = method.GetILGenerator();
    var parentFlags = lambdaExpr.ReturnType == typeof(void) ? ParentFlags.IgnoreResult : ParentFlags.Empty;
    if (!EmittingVisitor.TryEmit(lambdaExpr.Body, lambdaExpr.Parameters, il, ref closureInfo, parentFlags))
      return null;
    il.Emit(OpCodes.Ret);

    var delegateType = typeof(TDelegate) != typeof(Delegate) ? typeof(TDelegate) : lambdaExpr.Type;
    var @delegate = (TDelegate)(object)method.CreateDelegate(delegateType, EmptyArrayClosure);
    ReturnClosureTypeToParamTypesToPool(closurePlusParamTypes);
    return @delegate;
  }

  #region Obsolete

  /// Obsolete
  [Obsolete("Not used - candidate for removal")]
  public static TDelegate TryCompile<TDelegate>(
    Expression bodyExpr, IReadOnlyList<ParameterExpression> paramExprs, Type[] paramTypes, Type returnType)
    where TDelegate : class =>
    (TDelegate)TryCompile(typeof(TDelegate), bodyExpr, paramExprs, paramTypes, returnType);

  /// Obsolete
  [Obsolete("Not used - candidate for removal")]
  public static object TryCompile(Type delegateType,
    Expression bodyExpr, IReadOnlyList<ParameterExpression> paramExprs, Type[] paramTypes, Type returnType) =>
    TryCompileBoundToFirstClosureParam(
      delegateType != typeof(Delegate) ? delegateType : Tools.GetFuncOrActionType(paramTypes, returnType),
      bodyExpr, paramExprs, GetClosureTypeToParamTypes(paramExprs), returnType);

  #endregion

  internal static object TryCompileBoundToFirstClosureParam(Type delegateType,
    Expression bodyExpr, IReadOnlyList<ParameterExpression> paramExprs, Type[] closurePlusParamTypes, Type returnType)
  {
    var closureInfo = new ClosureInfo(ClosureStatus.ToBeCollected);
    if (!TryCollectBoundConstants(ref closureInfo, bodyExpr, paramExprs, false, ref closureInfo))
      return null;

    var nestedLambdas = closureInfo.NestedLambdas;
    if (nestedLambdas.Length != 0)
      for (var i = 0; i < nestedLambdas.Length; ++i)
        if (!TryCompileNestedLambda(ref closureInfo, i))
          return null;

    var closure = (closureInfo.Status & ClosureStatus.HasClosure) == 0
      ? EmptyArrayClosure
      : new ArrayClosure(closureInfo.GetArrayOfConstantsAndNestedLambdas());

    var method = new DynamicMethod(string.Empty,
      returnType, closurePlusParamTypes, typeof(ArrayClosure), true);

    var il = method.GetILGenerator();

    if (closure.ConstantsAndNestedLambdas != null)
      EmittingVisitor.EmitLoadConstantsAndNestedLambdasIntoVars(il, ref closureInfo);

    var parentFlags = returnType == typeof(void) ? ParentFlags.IgnoreResult : ParentFlags.Empty;
    if (!EmittingVisitor.TryEmit(bodyExpr, paramExprs, il, ref closureInfo, parentFlags))
      return null;

    il.Emit(OpCodes.Ret);

    var @delegate = method.CreateDelegate(delegateType, closure);
    ReturnClosureTypeToParamTypesToPool(closurePlusParamTypes);
    return @delegate;
  }

  private static Type[] PrependClosureTypeToParamTypes(IReadOnlyList<ParameterExpression> paramExprs)
  {
    var count = paramExprs.Count;
    var closureAndParamTypes = new Type[count + 1];
    closureAndParamTypes[0] = typeof(ArrayClosure);
    for (var i = 0; i < count; i++)
    {
      var parameterExpr = paramExprs[i];
      closureAndParamTypes[i + 1] = parameterExpr.IsByRef ? parameterExpr.Type.MakeByRefType() : parameterExpr.Type;
    }
    return closureAndParamTypes;
  }

  private static readonly Type[] _closureAsASingleParamType = { typeof(ArrayClosure) };
  private static readonly Type[][] _closureTypePlusParamTypesPool = new Type[8][];

  private static Type[] GetClosureTypeToParamTypes(IReadOnlyList<ParameterExpression> paramExprs)
  {
    var paramCount = paramExprs.Count;
    if (paramCount == 0)
      return _closureAsASingleParamType;

    if (paramCount < 8)
    {
      var closureAndParamTypes = Interlocked.Exchange(ref _closureTypePlusParamTypesPool[paramCount], null);
      if (closureAndParamTypes != null)
      {
        for (var i = 0; i < paramExprs.Count; i++)
        {
          var parameterExpr = paramExprs[i];
          closureAndParamTypes[i + 1] = parameterExpr.IsByRef ? parameterExpr.Type.MakeByRefType() : parameterExpr.Type;
        }
        return closureAndParamTypes;
      }
    }

    return PrependClosureTypeToParamTypes(paramExprs);
  }

  private static void ReturnClosureTypeToParamTypesToPool(Type[] closurePlusParamTypes)
  {
    var paramCount = closurePlusParamTypes.Length - 1;
    if (paramCount != 0 && paramCount < 8)
      Interlocked.Exchange(ref _closureTypePlusParamTypesPool[paramCount], closurePlusParamTypes);
  }

  private struct BlockInfo
  {
    public object VarExprs;     // ParameterExpression  | IReadOnlyList<ParameterExpression>
    public int[] VarIndexes;
  }

  [Flags]
  internal enum ClosureStatus
  {
    ToBeCollected = 1,
    UserProvided = 1 << 1,
    HasClosure = 1 << 2,
    ShouldBeStaticMethod = 1 << 3
  }

  /// Track the info required to build a closure object + some context information not directly related to closure.
  internal struct ClosureInfo
  {
    public bool LastEmitIsAddress;

    /// Helpers to know if a Return GotoExpression's Label should be emitted.
    /// First set bit is ContainsReturnGoto, the rest is ReturnLabelIndex
    private int[] _tryCatchFinallyInfos;
    public int CurrentTryCatchFinallyIndex;

    /// Tracks the stack of blocks where are we in emit phase
    private LiveCountArray<BlockInfo> _blockStack;

    /// Dictionary for the used Labels in IL
    private KeyValuePair<LabelTarget, Label?>[] _labels;

    public ClosureStatus Status;

    /// Constant expressions to find an index (by reference) of constant expression from compiled expression.
    public LiveCountArray<object> Constants;
    // todo: combine Constants and Usage to save the memory
    /// Constant usage count and variable index
    public LiveCountArray<int> ConstantUsageThenVarIndex;

    /// Parameters not passed through lambda parameter list But used inside lambda body.
    /// The top expression should Not contain not passed parameters. 
    public ParameterExpression[] NonPassedParameters;

    /// All nested lambdas recursively nested in expression
    public NestedLambdaInfo[] NestedLambdas;

    /// <summary>Populates info directly with provided closure object and constants.
    /// If provided, the <paramref name="constUsage"/> should be the size of <paramref name="constValues"/>
    /// </summary>
    public ClosureInfo(ClosureStatus status, object[] constValues = null, int[] constUsage = null)
    {
      Status = status;

      Constants = new LiveCountArray<object>(constValues ?? Tools.Empty<object>());
      ConstantUsageThenVarIndex = new LiveCountArray<int>(
        constValues == null ? Tools.Empty<int>() : constUsage ?? new int[constValues.Length]);

      NonPassedParameters = Tools.Empty<ParameterExpression>();
      NestedLambdas = Tools.Empty<NestedLambdaInfo>();

      LastEmitIsAddress = false;
      CurrentTryCatchFinallyIndex = -1;
      _tryCatchFinallyInfos = null;
      _labels = null;
      _blockStack = new LiveCountArray<BlockInfo>(Tools.Empty<BlockInfo>());
    }

    public void AddConstantOrIncrementUsageCount(object value, Type type)
    {
      Status |= ClosureStatus.HasClosure;

      var constItems = Constants.Items;
      var constIndex = Constants.Count - 1;
      while (constIndex != -1 && !ReferenceEquals(constItems[constIndex], value))
        --constIndex;

      if (constIndex == -1)
      {
        Constants.PushSlot(value);
        ConstantUsageThenVarIndex.PushSlot(1);
      }
      else
      {
        ++ConstantUsageThenVarIndex.Items[constIndex];
      }
    }

    public void AddNonPassedParam(ParameterExpression expr)
    {
      Status |= ClosureStatus.HasClosure;

      if (NonPassedParameters.Length == 0)
      {
        NonPassedParameters = new[] { expr };
        return;
      }

      var count = NonPassedParameters.Length;
      for (var i = 0; i < count; ++i)
        if (ReferenceEquals(NonPassedParameters[i], expr))
          return;

      if (NonPassedParameters.Length == 1)
        NonPassedParameters = new[] { NonPassedParameters[0], expr };
      else if (NonPassedParameters.Length == 2)
        NonPassedParameters = new[] { NonPassedParameters[0], NonPassedParameters[1], expr };
      else
      {
        var newItems = new ParameterExpression[count + 1];
        Array.Copy(NonPassedParameters, 0, newItems, 0, count);
        newItems[count] = expr;
        NonPassedParameters = newItems;
      }
    }

    public void AddNestedLambda(NestedLambdaInfo nestedLambdaInfo)
    {
      Status |= ClosureStatus.HasClosure;

      var nestedLambdas = NestedLambdas;
      var count = nestedLambdas.Length;
      if (count == 0)
        NestedLambdas = new[] { nestedLambdaInfo };
      else if (count == 1)
        NestedLambdas = new[] { nestedLambdas[0], nestedLambdaInfo };
      else if (count == 2)
        NestedLambdas = new[] { nestedLambdas[0], nestedLambdas[1], nestedLambdaInfo };
      else
      {
        var newNestedLambdas = new NestedLambdaInfo[count + 1];
        Array.Copy(nestedLambdas, 0, newNestedLambdas, 0, count);
        newNestedLambdas[count] = nestedLambdaInfo;
        NestedLambdas = newNestedLambdas;
      }
    }

    public void AddLabel(LabelTarget labelTarget)
    {
      if (labelTarget != null &&
          GetLabelIndex(labelTarget) == -1)
        _labels = _labels.WithLast(new KeyValuePair<LabelTarget, Label?>(labelTarget, null));
    }

    public Label GetOrCreateLabel(LabelTarget labelTarget, ILGenerator il) =>
      GetOrCreateLabel(GetLabelIndex(labelTarget), il);

    public Label GetOrCreateLabel(int index, ILGenerator il)
    {
      var labelPair = _labels[index];
      var label = labelPair.Value;
      if (!label.HasValue)
        _labels[index] = new KeyValuePair<LabelTarget, Label?>(labelPair.Key, label = il.DefineLabel());
      return label.Value;
    }

    public int GetLabelIndex(LabelTarget labelTarget)
    {
      if (_labels != null)
        for (var i = 0; i < _labels.Length; ++i)
          if (_labels[i].Key == labelTarget)
            return i;
      return -1;
    }

    public void AddTryCatchFinallyInfo()
    {
      ++CurrentTryCatchFinallyIndex;
      var infos = _tryCatchFinallyInfos;
      if (infos == null)
        _tryCatchFinallyInfos = new int[1];
      else if (infos.Length == 1)
        _tryCatchFinallyInfos = new[] { infos[0], 0 };
      else if (infos.Length == 2)
        _tryCatchFinallyInfos = new[] { infos[0], infos[1], 0 };
      else
      {
        var sourceLength = infos.Length;
        var newInfos = new int[sourceLength + 1];
        Array.Copy(infos, newInfos, sourceLength);
        _tryCatchFinallyInfos = newInfos;
      }
    }

    public void MarkAsContainsReturnGotoExpression()
    {
      if (CurrentTryCatchFinallyIndex != -1)
        _tryCatchFinallyInfos[CurrentTryCatchFinallyIndex] |= 1;
    }

    public void MarkReturnLabelIndex(int index)
    {
      if (CurrentTryCatchFinallyIndex != -1)
        _tryCatchFinallyInfos[CurrentTryCatchFinallyIndex] |= index << 1;
    }

    public bool TryCatchFinallyContainsReturnGotoExpression() =>
      _tryCatchFinallyInfos != null && (_tryCatchFinallyInfos[++CurrentTryCatchFinallyIndex] & 1) != 0;

    public object[] GetArrayOfConstantsAndNestedLambdas()
    {
      var constCount = Constants.Count;
      var nestedLambdas = NestedLambdas;
      if (constCount == 0)
      {
        if (nestedLambdas.Length == 0)
          return null;

        var nestedLambdaItems = new object[nestedLambdas.Length];
        for (var i = 0; i < nestedLambdas.Length; i++)
        {
          var nestedLambda = nestedLambdas[i];
          if (nestedLambda.ClosureInfo.NonPassedParameters.Length == 0)
            nestedLambdaItems[i] = nestedLambda.Lambda;
          else
            nestedLambdaItems[i] = new NestedLambdaWithConstantsAndNestedLambdas(
              nestedLambda.Lambda, nestedLambda.ClosureInfo.GetArrayOfConstantsAndNestedLambdas());
        }

        return nestedLambdaItems;
      }

      var constItems = Constants.Items;
      if (nestedLambdas.Length == 0)
      {
        Array.Resize(ref constItems, constCount);
        return constItems;
      }

      var itemCount = constCount + nestedLambdas.Length;

      var closureItems = constItems;
      if (itemCount > constItems.Length)
      {
        closureItems = new object[itemCount];
        for (var i = 0; i < constCount; ++i)
          closureItems[i] = constItems[i];
      }
      else
      {
        Array.Resize(ref constItems, itemCount);
      }

      for (var i = 0; i < nestedLambdas.Length; i++)
      {
        var nestedLambda = nestedLambdas[i];
        if (nestedLambda.ClosureInfo.NonPassedParameters.Length == 0)
          closureItems[constCount + i] = nestedLambda.Lambda;
        else
          closureItems[constCount + i] = new NestedLambdaWithConstantsAndNestedLambdas(
            nestedLambda.Lambda, nestedLambda.ClosureInfo.GetArrayOfConstantsAndNestedLambdas());
      }

      return closureItems;
    }

    /// LocalVar maybe a `null` in collecting phase when we only need to decide if ParameterExpression is an actual parameter or variable
    public void PushBlockWithVars(ParameterExpression blockVarExpr)
    {
      ref var block = ref _blockStack.PushSlot();
      block.VarExprs = blockVarExpr;
    }

    public void PushBlockWithVars(ParameterExpression blockVarExpr, int varIndex)
    {
      ref var block = ref _blockStack.PushSlot();
      block.VarExprs = blockVarExpr;
      block.VarIndexes = new[] { varIndex };
    }

    /// LocalVars maybe a `null` in collecting phase when we only need to decide if ParameterExpression is an actual parameter or variable
    public void PushBlockWithVars(IReadOnlyList<ParameterExpression> blockVarExprs, int[] localVarIndexes = null)
    {
      ref var block = ref _blockStack.PushSlot();
      block.VarExprs = blockVarExprs;
      block.VarIndexes = localVarIndexes;
    }

    public void PushBlockAndConstructLocalVars(IReadOnlyList<ParameterExpression> blockVarExprs, ILGenerator il)
    {
      var localVars = new int[blockVarExprs.Count];
      for (var i = 0; i < localVars.Length; i++)
        localVars[i] = il.GetNextLocalVarIndex(blockVarExprs[i].Type);

      PushBlockWithVars(blockVarExprs, localVars);
    }

    public void PopBlock() => _blockStack.Pop();

    public bool IsLocalVar(object varParamExpr)
    {
      for (var i = _blockStack.Count - 1; i > -1; --i)
      {
        var varExprObj = _blockStack.Items[i].VarExprs;
        if (ReferenceEquals(varExprObj, varParamExpr))
          return true;

        if (varExprObj is IReadOnlyList<ParameterExpression> varExprs)
          for (var j = 0; j < varExprs.Count; j++)
            if (ReferenceEquals(varExprs[j], varParamExpr))
              return true;
      }

      return false;
    }

    public int GetDefinedLocalVarOrDefault(ParameterExpression varParamExpr)
    {
      for (var i = _blockStack.Count - 1; i > -1; --i)
      {
        ref var block = ref _blockStack.Items[i];
        var varExprObj = block.VarExprs;

        if (ReferenceEquals(varExprObj, varParamExpr))
          return block.VarIndexes[0];

        if (varExprObj is IReadOnlyList<ParameterExpression> varExprs)
          for (var j = 0; j < varExprs.Count; j++)
            if (ReferenceEquals(varExprs[j], varParamExpr))
              return block.VarIndexes[j];
      }
      return -1;
    }

    public bool IsTryReturnLabel(int index)
    {
      var tryCatchFinallyInfos = _tryCatchFinallyInfos;
      if (tryCatchFinallyInfos != null)
        for (var i = 0; i < tryCatchFinallyInfos.Length; ++i)
          if (tryCatchFinallyInfos[i] >> 1 == index)
            return true;
      return false;
    }
  }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

  public static readonly ArrayClosure EmptyArrayClosure = new ArrayClosure(null);

  public static FieldInfo ArrayClosureArrayField =
    typeof(ArrayClosure).GetTypeInfo().GetDeclaredField(nameof(ArrayClosure.ConstantsAndNestedLambdas));

  public static FieldInfo ArrayClosureWithNonPassedParamsField =
    typeof(ArrayClosureWithNonPassedParams).GetTypeInfo().GetDeclaredField(nameof(ArrayClosureWithNonPassedParams.NonPassedParams));

  public static ConstructorInfo ArrayClosureWithNonPassedParamsConstructor =
    typeof(ArrayClosureWithNonPassedParams).GetTypeInfo().DeclaredConstructors.GetFirst();

  public class ArrayClosure
  {
    public readonly object[] ConstantsAndNestedLambdas;
    public ArrayClosure(object[] constantsAndNestedLambdas) => ConstantsAndNestedLambdas = constantsAndNestedLambdas;
  }

  public sealed class ArrayClosureWithNonPassedParams : ArrayClosure
  {
    public readonly object[] NonPassedParams;

    public ArrayClosureWithNonPassedParams(object[] constantsAndNestedLambdas, object[] nonPassedParams) : base(constantsAndNestedLambdas) =>
      NonPassedParams = nonPassedParams;
  }

  public sealed class NestedLambdaWithConstantsAndNestedLambdas
  {
    public static FieldInfo NestedLambdaField =
      typeof(NestedLambdaWithConstantsAndNestedLambdas).GetTypeInfo().GetDeclaredField(nameof(NestedLambda));

    public static FieldInfo ConstantsAndNestedLambdasField =
      typeof(NestedLambdaWithConstantsAndNestedLambdas).GetTypeInfo().GetDeclaredField(nameof(ConstantsAndNestedLambdas));

    public readonly object NestedLambda;
    public readonly object ConstantsAndNestedLambdas;
    public NestedLambdaWithConstantsAndNestedLambdas(object nestedLambda, object constantsAndNestedLambdas)
    {
      NestedLambda = nestedLambda;
      ConstantsAndNestedLambdas = constantsAndNestedLambdas;
    }
  }

  internal sealed class NestedLambdaInfo
  {
    public readonly LambdaExpression LambdaExpression;
    public ClosureInfo ClosureInfo;
    public object Lambda;
    public int UsageCountOrVarIndex;

    public NestedLambdaInfo(LambdaExpression lambdaExpression)
    {
      LambdaExpression = lambdaExpression;
      ClosureInfo = new ClosureInfo(ClosureStatus.ToBeCollected);
      Lambda = null;
    }
  }

  internal static class CurryClosureFuncs
  {
    public static readonly MethodInfo[] Methods =
      typeof(CurryClosureFuncs).GetTypeInfo().DeclaredMethods.AsArray();

    public static Func<R> Curry<C, R>(Func<C, R> f, C c) =>
      () => f(c);

    public static Func<T1, R> Curry<C, T1, R>(Func<C, T1, R> f, C c) =>
      t1 => f(c, t1);

    public static Func<T1, T2, R> Curry<C, T1, T2, R>(Func<C, T1, T2, R> f, C c) =>
      (t1, t2) => f(c, t1, t2);

    public static Func<T1, T2, T3, R> Curry<C, T1, T2, T3, R>(Func<C, T1, T2, T3, R> f, C c) =>
      (t1, t2, t3) => f(c, t1, t2, t3);

    public static Func<T1, T2, T3, T4, R> Curry<C, T1, T2, T3, T4, R>(Func<C, T1, T2, T3, T4, R> f, C c) =>
      (t1, t2, t3, t4) => f(c, t1, t2, t3, t4);

    public static Func<T1, T2, T3, T4, T5, R> Curry<C, T1, T2, T3, T4, T5, R>(Func<C, T1, T2, T3, T4, T5, R> f,
      C c) => (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5);

    public static Func<T1, T2, T3, T4, T5, T6, R>
      Curry<C, T1, T2, T3, T4, T5, T6, R>(Func<C, T1, T2, T3, T4, T5, T6, R> f, C c) =>
      (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6);
  }

  internal static class CurryClosureActions
  {
    public static readonly MethodInfo[] Methods =
      typeof(CurryClosureActions).GetTypeInfo().DeclaredMethods.AsArray();

    public static Action Curry<C>(Action<C> a, C c) =>
      () => a(c);

    public static Action<T1> Curry<C, T1>(Action<C, T1> f, C c) =>
      t1 => f(c, t1);

    public static Action<T1, T2> Curry<C, T1, T2>(Action<C, T1, T2> f, C c) =>
      (t1, t2) => f(c, t1, t2);

    public static Action<T1, T2, T3> Curry<C, T1, T2, T3>(Action<C, T1, T2, T3> f, C c) =>
      (t1, t2, t3) => f(c, t1, t2, t3);

    public static Action<T1, T2, T3, T4> Curry<C, T1, T2, T3, T4>(Action<C, T1, T2, T3, T4> f, C c) =>
      (t1, t2, t3, t4) => f(c, t1, t2, t3, t4);

    public static Action<T1, T2, T3, T4, T5> Curry<C, T1, T2, T3, T4, T5>(Action<C, T1, T2, T3, T4, T5> f,
      C c) => (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5);

    public static Action<T1, T2, T3, T4, T5, T6>
      Curry<C, T1, T2, T3, T4, T5, T6>(Action<C, T1, T2, T3, T4, T5, T6> f, C c) =>
      (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6);
  }

  #region Collect Bound Constants

  /// Helps to identify constants as the one to be put into the Closure
  public static bool IsClosureBoundConstant(object value, TypeInfo type) =>
    value is Delegate ||
    !type.IsPrimitive && !type.IsEnum && value is string == false && value is Type == false && value is decimal == false;

  // @paramExprs is required for nested lambda compilation
  private static bool TryCollectBoundConstants(ref ClosureInfo closure, Expression expr,
    IReadOnlyList<ParameterExpression> paramExprs, bool isNestedLambda, ref ClosureInfo rootClosure)
  {
    while (true)
    {
      if (expr == null)
        return false;

      switch (expr.NodeType)
      {
        case ExpressionType.Constant:
          var constantExpr = (ConstantExpression)expr;
          var value = constantExpr.Value;
          if (value != null)
          {
            // todo: find the way to speed-up this, track the usage in constant itself?

            var valueType = value.GetType();
            if (IsClosureBoundConstant(value, valueType.GetTypeInfo()))
              closure.AddConstantOrIncrementUsageCount(value, valueType);
          }

          return true;

        case ExpressionType.Quote:
          //var operand = ((UnaryExpression)expr).Operand;
          //if (operand != null && IsClosureBoundConstant(operand, expr.Type.GetTypeInfo()))
          //    closure.AddConstant(operand);
          return false;

        case ExpressionType.Parameter:
          // if parameter is used BUT is not in passed parameters and not in local variables,
          // it means parameter is provided by outer lambda and should be put in closure for current lambda
          var p = paramExprs.Count - 1;
          while (p != -1 && !ReferenceEquals(paramExprs[p], expr)) --p;
          if (p == -1 && !closure.IsLocalVar(expr))
          {
            if (!isNestedLambda)
              return false;
            closure.AddNonPassedParam((ParameterExpression)expr);
          }
          return true;

        case ExpressionType.Call:
          var callExpr = (MethodCallExpression)expr;
          var callObjectExpr = callExpr.Object;

          var fewCallArgCount = callExpr.FewArgumentCount;
          if (fewCallArgCount == 0)
          {
            if (callObjectExpr != null)
            {
              expr = callObjectExpr;
              continue;
            }

            return true;
          }

          if (fewCallArgCount > 0)
          {
            if (callObjectExpr != null &&
                !TryCollectBoundConstants(ref closure, callObjectExpr, paramExprs, isNestedLambda, ref rootClosure))
              return false;

            if (fewCallArgCount == 1)
            {
              expr = ((OneArgumentMethodCallExpression)callExpr).Argument;
              continue;
            }

            if (fewCallArgCount == 2)
            {
              var twoArgsExpr = (TwoArgumentsMethodCallExpression)callExpr;
              if (!TryCollectBoundConstants(ref closure, twoArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure))
                return false;
              expr = twoArgsExpr.Argument1;
              continue;
            }

            if (fewCallArgCount == 3)
            {
              var threeArgsExpr = (ThreeArgumentsMethodCallExpression)callExpr;
              if (!TryCollectBoundConstants(ref closure, threeArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure) ||
                  !TryCollectBoundConstants(ref closure, threeArgsExpr.Argument1, paramExprs, isNestedLambda, ref rootClosure))
                return false;
              expr = threeArgsExpr.Argument2;
              continue;
            }

            if (fewCallArgCount == 4)
            {
              var fourArgsExpr = (FourArgumentsMethodCallExpression)callExpr;
              if (!TryCollectBoundConstants(ref closure, fourArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure) ||
                  !TryCollectBoundConstants(ref closure, fourArgsExpr.Argument1, paramExprs, isNestedLambda, ref rootClosure) ||
                  !TryCollectBoundConstants(ref closure, fourArgsExpr.Argument2, paramExprs, isNestedLambda, ref rootClosure))
                return false;
              expr = fourArgsExpr.Argument3;
              continue;
            }

            var fiveArgsExpr = (FiveArgumentsMethodCallExpression)callExpr;
            if (!TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument1, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument2, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument3, paramExprs, isNestedLambda, ref rootClosure))
              return false;
            expr = fiveArgsExpr.Argument4;
            continue;
          }

          var methodArgs = callExpr.Arguments;
          var methodArgCount = methodArgs.Count;
          if (methodArgCount == 0)
          {
            if (callObjectExpr != null)
            {
              expr = callObjectExpr;
              continue;
            }

            return true;
          }

          if (callObjectExpr != null &&
              !TryCollectBoundConstants(ref closure, callExpr.Object, paramExprs, isNestedLambda, ref rootClosure))
            return false;

          for (var i = 0; i < methodArgCount - 1; i++)
            if (!TryCollectBoundConstants(ref closure, methodArgs[i], paramExprs, isNestedLambda, ref rootClosure))
              return false;

          expr = methodArgs[methodArgCount - 1];
          continue;

        case ExpressionType.MemberAccess:
          var memberExpr = ((MemberExpression)expr).Expression;
          if (memberExpr == null)
            return true;
          expr = memberExpr;
          continue;

        case ExpressionType.New:
          var newExpr = (NewExpression)expr;

          var fewArgCount = newExpr.FewArgumentCount;
          if (fewArgCount == 0)
            return true;

          if (fewArgCount > 0)
          {
            if (fewArgCount == 1)
            {
              expr = ((OneArgumentNewExpression)newExpr).Argument;
              continue;
            }

            if (fewArgCount == 2)
            {
              var twoArgsExpr = (TwoArgumentsNewExpression)newExpr;
              if (!TryCollectBoundConstants(ref closure, twoArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure))
                return false;
              expr = twoArgsExpr.Argument1;
              continue;
            }

            if (fewArgCount == 3)
            {
              var threeArgsExpr = (ThreeArgumentsNewExpression)newExpr;
              if (!TryCollectBoundConstants(ref closure, threeArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure) ||
                  !TryCollectBoundConstants(ref closure, threeArgsExpr.Argument1, paramExprs, isNestedLambda, ref rootClosure))
                return false;
              expr = threeArgsExpr.Argument2;
              continue;
            }

            if (fewArgCount == 4)
            {
              var fourArgsExpr = (FourArgumentsNewExpression)newExpr;
              if (!TryCollectBoundConstants(ref closure, fourArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure) ||
                  !TryCollectBoundConstants(ref closure, fourArgsExpr.Argument1, paramExprs, isNestedLambda, ref rootClosure) ||
                  !TryCollectBoundConstants(ref closure, fourArgsExpr.Argument2, paramExprs, isNestedLambda, ref rootClosure))
                return false;
              expr = fourArgsExpr.Argument3;
              continue;
            }

            var fiveArgsExpr = (FiveArgumentsNewExpression)newExpr;
            if (!TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument0, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument1, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument2, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, fiveArgsExpr.Argument3, paramExprs, isNestedLambda, ref rootClosure))
              return false;
            expr = fiveArgsExpr.Argument4;
            continue;
          }

          var ctorArgs = ((NewExpression)expr).Arguments;
          var ctorLastArgIndex = ctorArgs.Count - 1;
          if (ctorLastArgIndex == -1)
            return true;

          for (var i = 0; i < ctorLastArgIndex; i++)
            if (!TryCollectBoundConstants(ref closure, ctorArgs[i], paramExprs, isNestedLambda, ref rootClosure))
              return false;
          expr = ctorArgs[ctorLastArgIndex];
          continue;

        case ExpressionType.NewArrayBounds:
        case ExpressionType.NewArrayInit:
          var elemExprs = ((NewArrayExpression)expr).Expressions;
          var elemExprsCount = elemExprs.Count;
          if (elemExprsCount == 0)
            return true;

          for (var i = 0; i < elemExprsCount - 1; i++)
            if (!TryCollectBoundConstants(ref closure, elemExprs[i], paramExprs, isNestedLambda, ref rootClosure))
              return false;

          expr = elemExprs[elemExprsCount - 1];
          continue;

        case ExpressionType.MemberInit:
          return TryCollectMemberInitExprConstants(
            ref closure, (MemberInitExpression)expr, paramExprs, isNestedLambda, ref rootClosure);

        case ExpressionType.Lambda:
          var nestedLambdaExpr = (LambdaExpression)expr;

          // Look for the already collected lambdas and if we have the same lambda, start from the root
          var nestedLambdas = rootClosure.NestedLambdas;
          if (nestedLambdas.Length != 0)
          {
            var foundLambdaInfo = FindAlreadyCollectedNestedLambdaInfo(nestedLambdas, nestedLambdaExpr, out var foundInLambdas);
            if (foundLambdaInfo != null)
            {
              // if the lambda is not found on the same level, then add it
              if (foundInLambdas == closure.NestedLambdas)
              {
                ++foundLambdaInfo.UsageCountOrVarIndex;
              }
              else
              {
                closure.AddNestedLambda(foundLambdaInfo);
                var foundLambdaNonPassedParams = foundLambdaInfo.ClosureInfo.NonPassedParameters;
                if (foundLambdaNonPassedParams.Length != 0)
                  PropagateNonPassedParamsToOuterLambda(ref closure, paramExprs, nestedLambdaExpr.Parameters, foundLambdaNonPassedParams);
              }

              return true;
            }
          }

          var nestedLambdaInfo = new NestedLambdaInfo(nestedLambdaExpr);
          if (!TryCollectBoundConstants(ref nestedLambdaInfo.ClosureInfo,
                nestedLambdaExpr.Body, nestedLambdaExpr.Parameters, true, ref rootClosure))
            return false;

          closure.AddNestedLambda(nestedLambdaInfo);
          var nestedNonPassedParams = nestedLambdaInfo.ClosureInfo.NonPassedParameters;
          if (nestedNonPassedParams.Length != 0)
            PropagateNonPassedParamsToOuterLambda(ref closure, paramExprs, nestedLambdaExpr.Parameters, nestedNonPassedParams);

          return true;

        case ExpressionType.Invoke:
          var invokeExpr = (InvocationExpression)expr;
          var invokeArgs = invokeExpr.Arguments;
          if (invokeArgs.Count == 0)
          {
            expr = invokeExpr.Expression;
            continue;
          }

          if (!TryCollectBoundConstants(ref closure, invokeExpr.Expression, paramExprs, isNestedLambda, ref rootClosure))
            return false;

          var lastArgIndex = invokeArgs.Count - 1;
          if (lastArgIndex > 0)
            for (var i = 0; i < lastArgIndex; i++)
              if (!TryCollectBoundConstants(ref closure, invokeArgs[i], paramExprs, isNestedLambda, ref rootClosure))
                return false;
          expr = invokeArgs[lastArgIndex];
          continue;

        case ExpressionType.Conditional:
          var condExpr = (ConditionalExpression)expr;
          if (!TryCollectBoundConstants(ref closure, condExpr.Test, paramExprs, isNestedLambda, ref rootClosure) ||
              !TryCollectBoundConstants(ref closure, condExpr.IfFalse, paramExprs, isNestedLambda, ref rootClosure))
            return false;
          expr = condExpr.IfTrue;
          continue;

        case ExpressionType.Block:

          if (expr is OneVariableTwoExpressionBlockExpression simpleBlock)
          {
            closure.PushBlockWithVars(simpleBlock.Variable);
            if (!TryCollectBoundConstants(ref closure, simpleBlock.Expression1, paramExprs, isNestedLambda, ref rootClosure) ||
                !TryCollectBoundConstants(ref closure, simpleBlock.Expression2, paramExprs, isNestedLambda, ref rootClosure))
              return false;
            closure.PopBlock();
            return true;
          }

          var blockExpr = (BlockExpression)expr;
          var blockVarExprs = blockExpr.Variables;
          var blockExprs = blockExpr.Expressions;

          if (blockVarExprs.Count == 0)
          {
            for (var i = 0; i < blockExprs.Count - 1; i++)
              if (!TryCollectBoundConstants(ref closure, blockExprs[i], paramExprs, isNestedLambda, ref rootClosure))
                return false;
            expr = blockExprs[blockExprs.Count - 1];
            continue;
          }
          else
          {
            if (blockVarExprs.Count == 1)
              closure.PushBlockWithVars(blockVarExprs[0]);
            else
              closure.PushBlockWithVars(blockVarExprs);

            for (var i = 0; i < blockExprs.Count; i++)
              if (!TryCollectBoundConstants(ref closure, blockExprs[i], paramExprs, isNestedLambda, ref rootClosure))
                return false;
            closure.PopBlock();
          }

          return true;

        case ExpressionType.Loop:
          var loopExpr = (LoopExpression)expr;
          closure.AddLabel(loopExpr.BreakLabel);
          closure.AddLabel(loopExpr.ContinueLabel);
          expr = loopExpr.Body;
          continue;

        case ExpressionType.Index:
          var indexExpr = (IndexExpression)expr;
          var indexArgs = indexExpr.Arguments;
          for (var i = 0; i < indexArgs.Count; i++)
            if (!TryCollectBoundConstants(ref closure, indexArgs[i], paramExprs, isNestedLambda, ref rootClosure))
              return false;
          if (indexExpr.Object == null)
            return true;
          expr = indexExpr.Object;
          continue;

        case ExpressionType.Try:
          return TryCollectTryExprConstants(ref closure, (TryExpression)expr, paramExprs, isNestedLambda, ref rootClosure);

        case ExpressionType.Label:
          var labelExpr = (LabelExpression)expr;
          var defaultValueExpr = labelExpr.DefaultValue;
          closure.AddLabel(labelExpr.Target);
          if (defaultValueExpr == null)
            return true;
          expr = defaultValueExpr;
          continue;

        case ExpressionType.Goto:
          var gotoExpr = (GotoExpression)expr;
          if (gotoExpr.Kind == GotoExpressionKind.Return)
            closure.MarkAsContainsReturnGotoExpression();

          if (gotoExpr.Value == null)
            return true;

          expr = gotoExpr.Value;
          continue;

        case ExpressionType.Switch:
          var switchExpr = ((SwitchExpression)expr);
          if (!TryCollectBoundConstants(ref closure, switchExpr.SwitchValue, paramExprs, isNestedLambda, ref rootClosure) ||
              switchExpr.DefaultBody != null &&
              !TryCollectBoundConstants(ref closure, switchExpr.DefaultBody, paramExprs, isNestedLambda, ref rootClosure))
            return false;
          var switchCases = switchExpr.Cases;
          for (var i = 0; i < switchCases.Count - 1; i++)
            if (!TryCollectBoundConstants(ref closure, switchCases[i].Body, paramExprs, isNestedLambda, ref rootClosure))
              return false;
          expr = switchCases[switchCases.Count - 1].Body;
          continue;

        case ExpressionType.Extension:
          expr = expr.Reduce();
          continue;

        case ExpressionType.Default:
          return true;

        default:
          if (expr is UnaryExpression unaryExpr)
          {
            expr = unaryExpr.Operand;
            continue;
          }

          if (expr is BinaryExpression binaryExpr)
          {
            if (!TryCollectBoundConstants(ref closure, binaryExpr.Left, paramExprs, isNestedLambda, ref rootClosure))
              return false;
            expr = binaryExpr.Right;
            continue;
          }

          if (expr is TypeBinaryExpression typeBinaryExpr)
          {
            expr = typeBinaryExpr.Expression;
            continue;
          }

          return false;
      }
    }
  }

  private static void PropagateNonPassedParamsToOuterLambda(
    ref ClosureInfo closure, IReadOnlyList<ParameterExpression> paramExprs,
    IReadOnlyList<ParameterExpression> nestedLambdaParamExprs, ParameterExpression[] nestedNonPassedParams)
  {
    // If nested non passed parameter is not matched with any outer passed parameter, 
    // then ensure it goes to outer non passed parameter.
    // But check that having a non-passed parameter in root expression is invalid.
    for (var i = 0; i < nestedNonPassedParams.Length; i++)
    {
      var nestedNonPassedParam = nestedNonPassedParams[i];

      var isInNestedLambda = false;
      if (nestedLambdaParamExprs.Count != 0)
        for (var p = 0; !isInNestedLambda && p < nestedLambdaParamExprs.Count; ++p)
          isInNestedLambda = ReferenceEquals(nestedLambdaParamExprs[p], nestedNonPassedParam);

      var isInOuterLambda = false;
      if (paramExprs.Count != 0)
        for (var p = 0; !isInOuterLambda && p < paramExprs.Count; ++p)
          isInOuterLambda = ReferenceEquals(paramExprs[p], nestedNonPassedParam);

      if (!isInNestedLambda && !isInOuterLambda)
        closure.AddNonPassedParam(nestedNonPassedParam);
    }
  }

  private static NestedLambdaInfo FindAlreadyCollectedNestedLambdaInfo(
    NestedLambdaInfo[] nestedLambdas, LambdaExpression nestedLambdaExpr, out NestedLambdaInfo[] foundInLambdas)
  {
    for (var i = 0; i < nestedLambdas.Length; i++)
    {
      var lambdaInfo = nestedLambdas[i];
      if (ReferenceEquals(lambdaInfo.LambdaExpression, nestedLambdaExpr))
      {
        foundInLambdas = nestedLambdas;
        return lambdaInfo;
      }

      var deeperNestedLambdas = lambdaInfo.ClosureInfo.NestedLambdas;
      if (deeperNestedLambdas.Length != 0)
      {
        var deeperLambdaInfo = FindAlreadyCollectedNestedLambdaInfo(deeperNestedLambdas, nestedLambdaExpr, out foundInLambdas);
        if (deeperLambdaInfo != null)
          return deeperLambdaInfo;
      }
    }

    foundInLambdas = null;
    return null;
  }

  private static bool TryCompileNestedLambda(ref ClosureInfo outerClosureInfo, int nestedLambdaIndex)
  {
    // 1. Try to compile nested lambda in place
    // 2. Check that parameters used in compiled lambda are passed or closed by outer lambda
    // 3. Add the compiled lambda to closure of outer lambda for later invocation
    var nestedLambdaInfo = outerClosureInfo.NestedLambdas[nestedLambdaIndex];
    if (nestedLambdaInfo.Lambda != null)
      return true;

    var nestedLambdaExpr = nestedLambdaInfo.LambdaExpression;
    ref var nestedLambdaClosureInfo = ref nestedLambdaInfo.ClosureInfo;

    var nestedLambdaParamExprs = nestedLambdaExpr.Parameters;
    var nestedLambdaNestedLambdas = nestedLambdaClosureInfo.NestedLambdas;
    if (nestedLambdaNestedLambdas.Length != 0)
      for (var i = 0; i < nestedLambdaNestedLambdas.Length; ++i)
        if (!TryCompileNestedLambda(ref nestedLambdaClosureInfo, i))
          return false;

    ArrayClosure nestedLambdaClosure = null;
    if (nestedLambdaClosureInfo.NonPassedParameters.Length == 0)
    {
      if ((nestedLambdaClosureInfo.Status & ClosureStatus.HasClosure) == 0)
        nestedLambdaClosure = EmptyArrayClosure;
      else
        nestedLambdaClosure = new ArrayClosure(nestedLambdaClosureInfo.GetArrayOfConstantsAndNestedLambdas());
    }

    var nestedReturnType = nestedLambdaExpr.ReturnType;
    var closurePlusParamTypes = GetClosureTypeToParamTypes(nestedLambdaParamExprs);

    var method = new DynamicMethod(string.Empty,
      nestedReturnType, closurePlusParamTypes, typeof(ArrayClosure), true);

    var il = method.GetILGenerator();

    if ((nestedLambdaClosureInfo.Status & ClosureStatus.HasClosure) != 0)
      EmittingVisitor.EmitLoadConstantsAndNestedLambdasIntoVars(il, ref nestedLambdaClosureInfo);

    var parentFlags = nestedReturnType == typeof(void) ? ParentFlags.IgnoreResult : ParentFlags.Empty;
    if (!EmittingVisitor.TryEmit(nestedLambdaExpr.Body, nestedLambdaParamExprs, il, ref nestedLambdaClosureInfo, parentFlags))
      return false;
    il.Emit(OpCodes.Ret);

    if (nestedLambdaClosure != null)
    {
      nestedLambdaInfo.Lambda = method.CreateDelegate(nestedLambdaExpr.Type, nestedLambdaClosure);
    }
    else
    {
      // Otherwise create a static or an open delegate to pass closure later with `TryEmitNestedLambda`,
      // constructing the new closure with non-passed arguments and the rest of items
      nestedLambdaInfo.Lambda = method.CreateDelegate(
        Tools.GetFuncOrActionType(closurePlusParamTypes, nestedReturnType),
        null);
    }

    ReturnClosureTypeToParamTypesToPool(closurePlusParamTypes);
    return true;
  }

  private static bool TryCollectMemberInitExprConstants(ref ClosureInfo closure, MemberInitExpression expr,
    IReadOnlyList<ParameterExpression> paramExprs, bool isNestedLambda, ref ClosureInfo rootClosure)
  {
    var newExpr = expr.NewExpression
                  ?? expr.Expression
      ;
    if (!TryCollectBoundConstants(ref closure, newExpr, paramExprs, isNestedLambda, ref rootClosure))
      return false;

    var memberBindings = expr.Bindings;
    for (var i = 0; i < memberBindings.Count; ++i)
    {
      var memberBinding = memberBindings[i];
      if (memberBinding.BindingType == MemberBindingType.Assignment &&
          !TryCollectBoundConstants(
            ref closure, ((MemberAssignment)memberBinding).Expression, paramExprs, isNestedLambda, ref rootClosure))
        return false;
    }

    return true;
  }

  private static bool TryCollectTryExprConstants(ref ClosureInfo closure, TryExpression tryExpr,
    IReadOnlyList<ParameterExpression> paramExprs, bool isNestedLambda, ref ClosureInfo rootClosure)
  {
    closure.AddTryCatchFinallyInfo();

    if (!TryCollectBoundConstants(ref closure, tryExpr.Body, paramExprs, isNestedLambda, ref rootClosure))
      return false;

    var catchBlocks = tryExpr.Handlers;
    for (var i = 0; i < catchBlocks.Count; i++)
    {
      var catchBlock = catchBlocks[i];
      var catchExVar = catchBlock.Variable;
      if (catchExVar != null)
      {
        closure.PushBlockWithVars(catchExVar);
        if (!TryCollectBoundConstants(ref closure, catchExVar, paramExprs, isNestedLambda, ref rootClosure))
          return false;
      }

      if (catchBlock.Filter != null &&
          !TryCollectBoundConstants(ref closure, catchBlock.Filter, paramExprs, isNestedLambda, ref rootClosure))
        return false;

      if (!TryCollectBoundConstants(ref closure, catchBlock.Body, paramExprs, isNestedLambda, ref rootClosure))
        return false;

      if (catchExVar != null)
        closure.PopBlock();
    }

    if (tryExpr.Finally != null &&
        !TryCollectBoundConstants(ref closure, tryExpr.Finally, paramExprs, isNestedLambda, ref rootClosure))
      return false;

    --closure.CurrentTryCatchFinallyIndex;
    return true;
  }

  #endregion

  // The minimal context-aware flags set by parent
  [Flags]
  internal enum ParentFlags
  {
    Empty = 0,
    IgnoreResult = 1 << 1,
    Call = 1 << 2,
    MemberAccess = 1 << 3, // Any Parent Expression is a MemberExpression
    Arithmetic = 1 << 4,
    Coalesce = 1 << 5,
    InstanceAccess = 1 << 6,
    DupMemberOwner = 1 << 7,
    TryCatch = 1 << 8,
    InstanceCall = Call | InstanceAccess
  }

  internal static bool IgnoresResult(this ParentFlags parent) => (parent & ParentFlags.IgnoreResult) != 0;

  /// <summary>Supports emitting of selected expressions, e.g. lambdaExpr are not supported yet.
  /// When emitter find not supported expression it will return false from <see cref="TryEmit"/>, so I could fallback
  /// to normal and slow Expression.Compile.</summary>
  private static class EmittingVisitor
  {
#if NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
            private static readonly MethodInfo _getTypeFromHandleMethod =
                typeof(Type).GetTypeInfo().GetDeclaredMethod("GetTypeFromHandle");

            private static readonly MethodInfo _objectEqualsMethod = GetObjectEquals();
            private static MethodInfo GetObjectEquals()
            {
                var ms = typeof(object).GetTypeInfo().GetDeclaredMethods("Equals");
                foreach (var m in ms)
                    if (m.GetParameters().Length == 2)
                        return m;
                throw new InvalidOperationException("object.Equals is not found");
            }
#else
    private static readonly MethodInfo _getTypeFromHandleMethod =
      ((Func<RuntimeTypeHandle, Type>)Type.GetTypeFromHandle).Method;

    private static readonly MethodInfo _objectEqualsMethod =
      ((Func<object, object, bool>)object.Equals).Method;
#endif

    public static bool TryEmit(Expression expr, IReadOnlyList<ParameterExpression> paramExprs,
      ILGenerator il, ref ClosureInfo closure, ParentFlags parent, int byRefIndex = -1)
    {
      while (true)
      {
        closure.LastEmitIsAddress = false;

        switch (expr.NodeType)
        {
          case ExpressionType.Parameter:
            return (parent & ParentFlags.IgnoreResult) != 0 ||
                   TryEmitParameter((ParameterExpression)expr, paramExprs, il, ref closure, parent, byRefIndex);

          case ExpressionType.TypeAs:
          case ExpressionType.IsTrue:
          case ExpressionType.IsFalse:
          case ExpressionType.Increment:
          case ExpressionType.Decrement:
          case ExpressionType.Negate:
          case ExpressionType.NegateChecked:
          case ExpressionType.OnesComplement:
          case ExpressionType.UnaryPlus:
          case ExpressionType.Unbox:
            return TryEmitSimpleUnaryExpression((UnaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Quote:
            //return TryEmitNotNullConstant(true, expr.Type, ((UnaryExpression)expr).Operand, il, ref closure);
            return false;

          case ExpressionType.TypeIs:
            return TryEmitTypeIs((TypeBinaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Not:
            return TryEmitNot((UnaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Convert:
          case ExpressionType.ConvertChecked:
            return TryEmitConvert((UnaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.ArrayIndex:
            var arrIndexExpr = (BinaryExpression)expr;
            return TryEmit(arrIndexExpr.Left, paramExprs, il, ref closure, parent) &&
                   TryEmit(arrIndexExpr.Right, paramExprs, il, ref closure, parent) &&
                   TryEmitArrayIndex(expr.Type, il);

          case ExpressionType.ArrayLength:
            var arrLengthExpr = (UnaryExpression)expr;
            return TryEmitArrayLength(arrLengthExpr, paramExprs, il, ref closure, parent);

          case ExpressionType.Constant:
            var constExpr = (ConstantExpression)expr;
            if ((parent & ParentFlags.IgnoreResult) != 0)
              return true;

            if (constExpr.Value == null)
            {
              il.Emit(OpCodes.Ldnull);
              return true;
            }

            return TryEmitNotNullConstant(true, constExpr.Type, constExpr.Value, il, ref closure);

          case ExpressionType.Call:
            return TryEmitMethodCall(expr, paramExprs, il, ref closure, parent);

          case ExpressionType.MemberAccess:
            return TryEmitMemberAccess((MemberExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.New:
            return TryEmitNew(expr, paramExprs, il, ref closure, parent);

          case ExpressionType.NewArrayBounds:
          case ExpressionType.NewArrayInit:
            return EmitNewArray((NewArrayExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.MemberInit:
            return EmitMemberInit((MemberInitExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Lambda:
            return TryEmitNestedLambda((LambdaExpression)expr, paramExprs, il, ref closure);

          case ExpressionType.Invoke:
            return TryEmitInvoke((InvocationExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.GreaterThan:
          case ExpressionType.GreaterThanOrEqual:
          case ExpressionType.LessThan:
          case ExpressionType.LessThanOrEqual:
          case ExpressionType.Equal:
          case ExpressionType.NotEqual:
            var binaryExpr = (BinaryExpression)expr;
            return TryEmitComparison(binaryExpr.Left, binaryExpr.Right, binaryExpr.NodeType,
              paramExprs, il, ref closure, parent);

          case ExpressionType.Add:
          case ExpressionType.AddChecked:
          case ExpressionType.Subtract:
          case ExpressionType.SubtractChecked:
          case ExpressionType.Multiply:
          case ExpressionType.MultiplyChecked:
          case ExpressionType.Divide:
          case ExpressionType.Modulo:
          case ExpressionType.Power:
          case ExpressionType.And:
          case ExpressionType.Or:
          case ExpressionType.ExclusiveOr:
          case ExpressionType.LeftShift:
          case ExpressionType.RightShift:
            var arithmeticExpr = (BinaryExpression)expr;
            return TryEmitArithmetic(arithmeticExpr, expr.NodeType, paramExprs, il, ref closure, parent);

          case ExpressionType.AndAlso:
          case ExpressionType.OrElse:
            return TryEmitLogicalOperator((BinaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Coalesce:
            return TryEmitCoalesceOperator((BinaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Conditional:
            return TryEmitConditional((ConditionalExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.PostIncrementAssign:
          case ExpressionType.PreIncrementAssign:
          case ExpressionType.PostDecrementAssign:
          case ExpressionType.PreDecrementAssign:
            return TryEmitIncDecAssign((UnaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.AddAssign:
          case ExpressionType.AddAssignChecked:
          case ExpressionType.SubtractAssign:
          case ExpressionType.SubtractAssignChecked:
          case ExpressionType.MultiplyAssign:
          case ExpressionType.MultiplyAssignChecked:
          case ExpressionType.DivideAssign:
          case ExpressionType.ModuloAssign:
          case ExpressionType.PowerAssign:
          case ExpressionType.AndAssign:
          case ExpressionType.OrAssign:
          case ExpressionType.ExclusiveOrAssign:
          case ExpressionType.LeftShiftAssign:
          case ExpressionType.RightShiftAssign:
          case ExpressionType.Assign:
            return TryEmitAssign((BinaryExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Block:
            if (expr is OneVariableTwoExpressionBlockExpression simpleBlockExpr)
            {
              closure.PushBlockWithVars(simpleBlockExpr.Variable, il.GetNextLocalVarIndex(simpleBlockExpr.Variable.Type));
              if (!TryEmit(simpleBlockExpr.Expression1, paramExprs, il, ref closure, parent | ParentFlags.IgnoreResult) ||
                  !TryEmit(simpleBlockExpr.Expression2, paramExprs, il, ref closure, parent))
                return false;
              closure.PopBlock();
              return true;
            }

            var blockExpr = (BlockExpression)expr;
            var blockVarExprs = blockExpr.Variables;
            var blockVarCount = blockVarExprs.Count;
            if (blockVarCount == 1)
              closure.PushBlockWithVars(blockVarExprs[0], il.GetNextLocalVarIndex(blockVarExprs[0].Type));
            else if (blockVarCount > 1)
              closure.PushBlockAndConstructLocalVars(blockVarExprs, il);

            var statementExprs = blockExpr.Expressions; // Trim the expressions after the Throw - #196
            var statementCount = statementExprs.Count;
            expr = statementExprs[statementCount - 1]; // The last (result) statement in block will provide the result

            // Try to trim the statements up to the Throw (if any)
            if (statementCount > 1)
            {
              var throwIndex = statementCount - 1;
              while (throwIndex != -1 && statementExprs[throwIndex].NodeType != ExpressionType.Throw)
                --throwIndex;

              // If we have a Throw and it is not the last one
              if (throwIndex != -1 && throwIndex != statementCount - 1)
              {
                // Change the Throw return type to match the one for the Block, and adjust the statement count
                expr = Expression.Throw(((UnaryExpression)statementExprs[throwIndex]).Operand, blockExpr.Type);
                statementCount = throwIndex + 1;
              }
            }

            // handle the all statements in block excluding the last one
            if (statementCount > 1)
              for (var i = 0; i < statementCount - 1; i++)
                if (!TryEmit(statementExprs[i], paramExprs, il, ref closure, parent | ParentFlags.IgnoreResult))
                  return false;

            if (blockVarCount == 0)
              continue; // OMG, no recursion, continue with last expression

            if (!TryEmit(expr, paramExprs, il, ref closure, parent))
              return false;

            closure.PopBlock();
            return true;

          case ExpressionType.Loop:
            return TryEmitLoop((LoopExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Try:
            return TryEmitTryCatchFinallyBlock((TryExpression)expr, paramExprs, il, ref closure,
              parent | ParentFlags.TryCatch);

          case ExpressionType.Throw:
            {
              if (!TryEmit(((UnaryExpression)expr).Operand, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
                return false;
              il.Emit(OpCodes.Throw);
              return true;
            }

          case ExpressionType.Default:
            if (expr.Type != typeof(void) && (parent & ParentFlags.IgnoreResult) == 0)
              EmitDefault(expr.Type, il);
            return true;

          case ExpressionType.Index:
            return TryEmitIndex((IndexExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Goto:
            return TryEmitGoto((GotoExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Label:
            return TryEmitLabel((LabelExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Switch:
            return TryEmitSwitch((SwitchExpression)expr, paramExprs, il, ref closure, parent);

          case ExpressionType.Extension:
            expr = expr.Reduce();
            continue;

          default:
            return false;
        }
      }
    }

    private static bool TryEmitNew(Expression expr, IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent)
    {
      var newExpr = (NewExpression)expr;

      var argCount = newExpr.FewArgumentCount;
      if (argCount > 0)
      {
        var args = newExpr.Constructor.GetParameters();
        if (argCount == 1)
        {
          var argExpr = ((OneArgumentNewExpression)newExpr).Argument;
          if (!TryEmit(argExpr, paramExprs, il, ref closure, parent, args[0].ParameterType.IsByRef ? 0 : -1))
            return false;
        }
        else if (argCount == 2)
        {
          var twoArgsExpr = (TwoArgumentsNewExpression)newExpr;
          if (!TryEmit(twoArgsExpr.Argument0, paramExprs, il, ref closure, parent, args[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(twoArgsExpr.Argument1, paramExprs, il, ref closure, parent, args[1].ParameterType.IsByRef ? 1 : -1))
            return false;
        }
        else if (argCount == 3)
        {
          var threeArgsExpr = (ThreeArgumentsNewExpression)newExpr;
          if (!TryEmit(threeArgsExpr.Argument0, paramExprs, il, ref closure, parent, args[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(threeArgsExpr.Argument1, paramExprs, il, ref closure, parent, args[1].ParameterType.IsByRef ? 1 : -1) ||
              !TryEmit(threeArgsExpr.Argument2, paramExprs, il, ref closure, parent, args[2].ParameterType.IsByRef ? 2 : -1))
            return false;
        }
        else if (argCount == 4)
        {
          var fourArgsExpr = (FourArgumentsNewExpression)newExpr;
          if (!TryEmit(fourArgsExpr.Argument0, paramExprs, il, ref closure, parent, args[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(fourArgsExpr.Argument1, paramExprs, il, ref closure, parent, args[1].ParameterType.IsByRef ? 1 : -1) ||
              !TryEmit(fourArgsExpr.Argument2, paramExprs, il, ref closure, parent, args[2].ParameterType.IsByRef ? 2 : -1) ||
              !TryEmit(fourArgsExpr.Argument3, paramExprs, il, ref closure, parent, args[3].ParameterType.IsByRef ? 3 : -1))
            return false;
        }
        else if (argCount == 5)
        {
          var fourArgsExpr = (FiveArgumentsNewExpression)newExpr;
          if (!TryEmit(fourArgsExpr.Argument0, paramExprs, il, ref closure, parent, args[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(fourArgsExpr.Argument1, paramExprs, il, ref closure, parent, args[1].ParameterType.IsByRef ? 1 : -1) ||
              !TryEmit(fourArgsExpr.Argument2, paramExprs, il, ref closure, parent, args[2].ParameterType.IsByRef ? 2 : -1) ||
              !TryEmit(fourArgsExpr.Argument3, paramExprs, il, ref closure, parent, args[3].ParameterType.IsByRef ? 3 : -1) ||
              !TryEmit(fourArgsExpr.Argument4, paramExprs, il, ref closure, parent, args[4].ParameterType.IsByRef ? 4 : -1))
            return false;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (newExpr.Constructor != null)
          il.Emit(OpCodes.Newobj, newExpr.Constructor);
        else if (newExpr.Type.IsValueType())
          EmitLoadLocalVariable(il, InitValueTypeVariable(il, newExpr.Type));
        else
          return false;
        return true;
      }

      var argExprs = newExpr.Arguments;
      if (argExprs.Count != 0)
      {
        var args = newExpr.Constructor.GetParameters();
        for (var i = 0; i < args.Length; ++i)
          if (!TryEmit(argExprs[i], paramExprs, il, ref closure, parent, args[i].ParameterType.IsByRef ? i : -1))
            return false;
      }

      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      if (newExpr.Constructor != null)
        il.Emit(OpCodes.Newobj, newExpr.Constructor);
      else if (newExpr.Type.IsValueType())
        EmitLoadLocalVariable(il, InitValueTypeVariable(il, newExpr.Type));
      else
        return false;
      return true;
    }

    private static bool TryEmitArrayLength(UnaryExpression arrLengthExpr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent)
    {
      if (!TryEmit(arrLengthExpr.Operand, paramExprs, il, ref closure, parent))
        return false;

      if ((parent & ParentFlags.IgnoreResult) == 0)
      {
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
      }

      return true;
    }

    private static bool TryEmitLoop(LoopExpression loopExpr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      // Mark the start of the loop body:
      var loopBodyLabel = il.DefineLabel();
      il.MarkLabel(loopBodyLabel);

      if (loopExpr.ContinueLabel != null)
        il.MarkLabel(closure.GetOrCreateLabel(loopExpr.ContinueLabel, il));

      if (!TryEmit(loopExpr.Body, paramExprs, il, ref closure, parent))
        return false;

      // If loop hasn't exited, jump back to start of its body:
      il.Emit(OpCodes.Br, loopBodyLabel);

      if (loopExpr.BreakLabel != null)
        il.MarkLabel(closure.GetOrCreateLabel(loopExpr.BreakLabel, il));

      return true;
    }

    private static bool TryEmitIndex(IndexExpression indexExpr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      if (indexExpr.Object != null &&
          !TryEmit(indexExpr.Object, paramExprs, il, ref closure, parent))
        return false;

      var indexArgExprs = indexExpr.Arguments;
      for (var i = 0; i < indexArgExprs.Count; i++)
        if (!TryEmit(indexArgExprs[i], paramExprs, il, ref closure, parent,
              indexArgExprs[i].Type.IsByRef ? i : -1))
          return false;

      var indexerProp = indexExpr.Indexer;
      if (indexerProp != null)
        return EmitMethodCall(il, indexerProp.DeclaringType.FindPropertyGetMethod(indexerProp.Name));

      if (indexExpr.Arguments.Count == 1) // one dimensional array
        return TryEmitArrayIndex(indexExpr.Type, il);

      // multi dimensional array
      return EmitMethodCall(il, indexExpr.Object?.Type.FindMethod("Get"));
    }

    private static bool TryEmitLabel(LabelExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var index = closure.GetLabelIndex(expr.Target);
      if (index == -1)
        return false; // should be found in first collecting constants round

      if (closure.IsTryReturnLabel(index))
        return true; // label will be emitted by TryEmitTryCatchFinallyBlock

      // define a new label or use the label provided by the preceding GoTo expression
      var label = closure.GetOrCreateLabel(index, il);

      il.MarkLabel(label);

      return expr.DefaultValue == null || TryEmit(expr.DefaultValue, paramExprs, il, ref closure, parent);
    }

    private static bool TryEmitGoto(GotoExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var index = closure.GetLabelIndex(expr.Target);
      if (index == -1)
      {
        if ((closure.Status & ClosureStatus.ToBeCollected) == 0)
          return false; // if no collection cycle then the labels may be not collected
        throw new InvalidOperationException("Cannot jump, no labels found");
      }

      if (expr.Value != null &&
          !TryEmit(expr.Value, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
        return false;

      switch (expr.Kind)
      {
        case GotoExpressionKind.Break:
        case GotoExpressionKind.Continue:
          // use label defined by Label expression or define its own to use by subsequent Label
          il.Emit(OpCodes.Br, closure.GetOrCreateLabel(index, il));
          return true;

        case GotoExpressionKind.Goto:
          if (expr.Value != null)
            goto case GotoExpressionKind.Return;

          // use label defined by Label expression or define its own to use by subsequent Label
          il.Emit(OpCodes.Br, closure.GetOrCreateLabel(index, il));
          return true;

        case GotoExpressionKind.Return:

          // check that we are inside the Try-Catch-Finally block
          if ((parent & ParentFlags.TryCatch) != 0)
          {
            // Can't emit a Return inside a Try/Catch, so leave it to TryEmitTryCatchFinallyBlock
            // to emit the Leave instruction, return label and return result
            closure.MarkReturnLabelIndex(index);
          }
          else
          {
            // use label defined by Label expression or define its own to use by subsequent Label
            il.Emit(OpCodes.Ret, closure.GetOrCreateLabel(index, il));
          }

          return true;

        default:
          return false;
      }
    }

    private static bool TryEmitCoalesceOperator(BinaryExpression exprObj,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var labelFalse = il.DefineLabel();
      var labelDone = il.DefineLabel();

      var left = exprObj.Left;
      var right = exprObj.Right;

      if (!TryEmit(left, paramExprs, il, ref closure, parent | ParentFlags.Coalesce))
        return false;

      var leftType = left.Type;
      if (leftType.IsValueType()) // Nullable -> It's the only ValueType comparable to null
      {
        var varIndex = EmitStoreLocalVariableAndLoadItsAddress(il, leftType);
        il.Emit(OpCodes.Call, leftType.FindNullableHasValueGetterMethod());

        il.Emit(OpCodes.Brfalse, labelFalse);
        EmitLoadLocalVariableAddress(il, varIndex);
        il.Emit(OpCodes.Call, leftType.FindNullableGetValueOrDefaultMethod());

        il.Emit(OpCodes.Br, labelDone);
        il.MarkLabel(labelFalse);
        if (!TryEmit(right, paramExprs, il, ref closure, parent | ParentFlags.Coalesce))
          return false;

        il.MarkLabel(labelDone);
        return true;
      }

      il.Emit(OpCodes.Dup); // duplicate left, if it's not null, after the branch this value will be on the top of the stack
      il.Emit(OpCodes.Ldnull);
      il.Emit(OpCodes.Ceq);
      il.Emit(OpCodes.Brfalse, labelFalse);

      il.Emit(OpCodes.Pop); // left is null, pop its value from the stack

      if (!TryEmit(right, paramExprs, il, ref closure, parent | ParentFlags.Coalesce))
        return false;

      if (right.Type != exprObj.Type)
      {
        if (right.Type.IsValueType())
          il.Emit(OpCodes.Box, right.Type);
      }

      if (left.Type == exprObj.Type)
        il.MarkLabel(labelFalse);
      else
      {
        il.Emit(OpCodes.Br, labelDone);
        il.MarkLabel(labelFalse);
        il.Emit(OpCodes.Castclass, exprObj.Type);
        il.MarkLabel(labelDone);
      }

      return true;
    }

    private static void EmitDefault(Type type, ILGenerator il)
    {
      if (!type.GetTypeInfo().IsValueType)
      {
        il.Emit(OpCodes.Ldnull);
      }
      else if (
        type == typeof(bool) ||
        type == typeof(byte) ||
        type == typeof(char) ||
        type == typeof(sbyte) ||
        type == typeof(int) ||
        type == typeof(uint) ||
        type == typeof(short) ||
        type == typeof(ushort))
      {
        il.Emit(OpCodes.Ldc_I4_0);
      }
      else if (
        type == typeof(long) ||
        type == typeof(ulong))
      {
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Conv_I8);
      }
      else if (type == typeof(float))
        il.Emit(OpCodes.Ldc_R4, default(float));
      else if (type == typeof(double))
        il.Emit(OpCodes.Ldc_R8, default(double));
      else
        EmitLoadLocalVariable(il, InitValueTypeVariable(il, type));
    }

    private static bool TryEmitTryCatchFinallyBlock(TryExpression tryExpr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var containsReturnGotoExpression = closure.TryCatchFinallyContainsReturnGotoExpression();
      il.BeginExceptionBlock();

      if (!TryEmit(tryExpr.Body, paramExprs, il, ref closure, parent))
        return false;

      var exprType = tryExpr.Type;
      var returnsResult = exprType != typeof(void) && (containsReturnGotoExpression || !parent.IgnoresResult());
      var resultVarIndex = -1;

      if (returnsResult)
        EmitStoreLocalVariable(il, resultVarIndex = il.GetNextLocalVarIndex(exprType));

      var catchBlocks = tryExpr.Handlers;
      for (var i = 0; i < catchBlocks.Count; i++)
      {
        var catchBlock = catchBlocks[i];
        if (catchBlock.Filter != null)
          return false; // todo: Add support for filters in catch expression

        il.BeginCatchBlock(catchBlock.Test);

        // at the beginning of catch the Exception value is on the stack,
        // we will store into local variable.
        var exVarExpr = catchBlock.Variable;
        if (exVarExpr != null)
        {
          var exVarIndex = il.GetNextLocalVarIndex(exVarExpr.Type);
          closure.PushBlockWithVars(exVarExpr, exVarIndex);
          EmitStoreLocalVariable(il, exVarIndex);
        }

        if (!TryEmit(catchBlock.Body, paramExprs, il, ref closure, parent))
          return false;

        if (exVarExpr != null)
          closure.PopBlock();

        if (returnsResult)
          EmitStoreLocalVariable(il, resultVarIndex);
      }

      var finallyExpr = tryExpr.Finally;
      if (finallyExpr != null)
      {
        il.BeginFinallyBlock();
        if (!TryEmit(finallyExpr, paramExprs, il, ref closure, parent))
          return false;
      }

      il.EndExceptionBlock();

      if (returnsResult)
        EmitLoadLocalVariable(il, resultVarIndex);

      --closure.CurrentTryCatchFinallyIndex;
      return true;
    }

    private static bool TryEmitParameter(ParameterExpression paramExpr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent, int byRefIndex = -1)
    {
      // if parameter is passed through, then just load it on stack
      var paramType = paramExpr.Type;

      var paramIndex = paramExprs.Count - 1;
      while (paramIndex != -1 && !ReferenceEquals(paramExprs[paramIndex], paramExpr))
        --paramIndex;
      if (paramIndex != -1)
      {
        if ((closure.Status & ClosureStatus.ShouldBeStaticMethod) == 0)
          ++paramIndex; // shift parameter index by one, because the first one will be closure

        closure.LastEmitIsAddress = !paramExpr.IsByRef && paramType.IsValueType() &&
                                    ((parent & ParentFlags.InstanceCall) == ParentFlags.InstanceCall ||
                                     (parent & ParentFlags.MemberAccess) != 0);

        if (closure.LastEmitIsAddress)
          il.Emit(OpCodes.Ldarga_S, (byte)paramIndex);
        else
        {
          if (paramIndex == 0)
            il.Emit(OpCodes.Ldarg_0);
          else if (paramIndex == 1)
            il.Emit(OpCodes.Ldarg_1);
          else if (paramIndex == 2)
            il.Emit(OpCodes.Ldarg_2);
          else if (paramIndex == 3)
            il.Emit(OpCodes.Ldarg_3);
          else
            il.Emit(OpCodes.Ldarg_S, (byte)paramIndex);
        }

        if (paramExpr.IsByRef)
        {
          if ((parent & ParentFlags.MemberAccess) != 0 && paramType.IsClass() ||
              (parent & ParentFlags.Coalesce) != 0)
            il.Emit(OpCodes.Ldind_Ref);
          else if ((parent & ParentFlags.Arithmetic) != 0)
            EmitDereference(il, paramType);
        }

        return true;
      }

      // If parameter isn't passed, then it is passed into some outer lambda or it is a local variable,
      // so it should be loaded from closure or from the locals. Then the closure is null will be an invalid state.
      // Parameter may represent a variable, so first look if this is the case
      var varIndex = closure.GetDefinedLocalVarOrDefault(paramExpr);
      if (varIndex != -1)
      {
        if (byRefIndex != -1 ||
            paramType.IsValueType() && (parent & (ParentFlags.MemberAccess | ParentFlags.InstanceAccess)) != 0)
          EmitLoadLocalVariableAddress(il, varIndex);
        else
          EmitLoadLocalVariable(il, varIndex);
        return true;
      }

      if (paramExpr.IsByRef)
      {
        EmitLoadLocalVariableAddress(il, byRefIndex);
        return true;
      }

      // the only possibility that we are here is because we are in the nested lambda,
      // and it uses the parameter or variable from the outer lambda
      var nonPassedParams = closure.NonPassedParameters;
      var nonPassedParamIndex = nonPassedParams.Length - 1;
      while (nonPassedParamIndex != -1 && !ReferenceEquals(nonPassedParams[nonPassedParamIndex], paramExpr))
        --nonPassedParamIndex;
      if (nonPassedParamIndex == -1)
        return false; // what??? no chance

      // Load non-passed argument from Closure - closure object is always a first argument
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldfld, ArrayClosureWithNonPassedParamsField);
      EmitLoadConstantInt(il, nonPassedParamIndex);
      il.Emit(OpCodes.Ldelem_Ref);

      // source type is object, NonPassedParams is object array
      if (paramType.IsValueType())
        il.Emit(OpCodes.Unbox_Any, paramType);

      return true;
    }

    private static void EmitDereference(ILGenerator il, Type type)
    {
      if (type == typeof(Int32))
        il.Emit(OpCodes.Ldind_I4);
      else if (type == typeof(Int64))
        il.Emit(OpCodes.Ldind_I8);
      else if (type == typeof(Int16))
        il.Emit(OpCodes.Ldind_I2);
      else if (type == typeof(SByte))
        il.Emit(OpCodes.Ldind_I1);
      else if (type == typeof(Single))
        il.Emit(OpCodes.Ldind_R4);
      else if (type == typeof(Double))
        il.Emit(OpCodes.Ldind_R8);
      else if (type == typeof(IntPtr))
        il.Emit(OpCodes.Ldind_I);
      else if (type == typeof(UIntPtr))
        il.Emit(OpCodes.Ldind_I);
      else if (type == typeof(Byte))
        il.Emit(OpCodes.Ldind_U1);
      else if (type == typeof(UInt16))
        il.Emit(OpCodes.Ldind_U2);
      else if (type == typeof(UInt32))
        il.Emit(OpCodes.Ldind_U4);
      else
        il.Emit(OpCodes.Ldobj, type);
      //todo: UInt64 as there is no OpCodes? Ldind_Ref?
    }

    private static bool TryEmitSimpleUnaryExpression(UnaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent)
    {
      var exprType = expr.Type;

      // todo: support decimal here
      if (exprType == typeof(decimal))
        return false;

      if (!TryEmit(expr.Operand, paramExprs, il, ref closure, parent))
        return false;

      if ((parent & ParentFlags.IgnoreResult) != 0)
        il.Emit(OpCodes.Pop);
      else
      {
        if (expr.NodeType == ExpressionType.TypeAs)
        {
          il.Emit(OpCodes.Isinst, exprType);
        }
        else if (expr.NodeType == ExpressionType.IsFalse)
        {
          var falseLabel = il.DefineLabel();
          var continueLabel = il.DefineLabel();
          il.Emit(OpCodes.Brfalse, falseLabel);
          il.Emit(OpCodes.Ldc_I4_0);
          il.Emit(OpCodes.Br, continueLabel);
          il.MarkLabel(falseLabel);
          il.Emit(OpCodes.Ldc_I4_1);
          il.MarkLabel(continueLabel);
        }
        else if (expr.NodeType == ExpressionType.Increment)
        {
          if (!TryEmitNumberOne(il, exprType))
            return false;
          il.Emit(OpCodes.Add);
        }
        else if (expr.NodeType == ExpressionType.Decrement)
        {
          if (!TryEmitNumberOne(il, exprType))
            return false;
          il.Emit(OpCodes.Sub);
        }
        else if (expr.NodeType == ExpressionType.Negate || expr.NodeType == ExpressionType.NegateChecked)
        {
          il.Emit(OpCodes.Neg);
        }
        else if (expr.NodeType == ExpressionType.OnesComplement)
        {
          il.Emit(OpCodes.Not);
        }
        else if (expr.NodeType == ExpressionType.Unbox)
        {
          il.Emit(OpCodes.Unbox_Any, exprType);
        }
        else if (expr.NodeType == ExpressionType.IsTrue)
        { }
        else if (expr.NodeType == ExpressionType.UnaryPlus)
        { }
      }
      return true;
    }

    private static bool TryEmitTypeIs(TypeBinaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent)
    {
      if (!TryEmit(expr.Expression, paramExprs, il, ref closure, parent))
        return false;

      if ((parent & ParentFlags.IgnoreResult) != 0)
        il.Emit(OpCodes.Pop);
      else
      {
        il.Emit(OpCodes.Isinst, expr.TypeOperand);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Cgt_Un);
      }

      return true;
    }

    private static bool TryEmitNot(UnaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent)
    {
      if (!TryEmit(expr.Operand, paramExprs, il, ref closure, parent))
        return false;
      if ((parent & ParentFlags.IgnoreResult) > 0)
        il.Emit(OpCodes.Pop);
      else
      {
        if (expr.Type == typeof(bool))
        {
          il.Emit(OpCodes.Ldc_I4_0);
          il.Emit(OpCodes.Ceq);
        }
        else
        {
          il.Emit(OpCodes.Not);
        }
      }
      return true;
    }

    private static bool TryEmitConvert(UnaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var opExpr = expr.Operand;
      var method = expr.Method;
      if (method != null && method.Name != "op_Implicit" && method.Name != "op_Explicit")
      {
        if (!TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult | ParentFlags.InstanceCall, 0))
          return false;

        il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        if ((parent & ParentFlags.IgnoreResult) != 0 && method.ReturnType != typeof(void))
          il.Emit(OpCodes.Pop);
        return true;
      }

      var sourceType = opExpr.Type;
      var sourceTypeIsNullable = sourceType.IsNullable();
      var underlyingNullableSourceType = Nullable.GetUnderlyingType(sourceType);
      var targetType = expr.Type;

      if (sourceTypeIsNullable && targetType == underlyingNullableSourceType)
      {
        if (!TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult | ParentFlags.InstanceAccess))
          return false;

        if (!closure.LastEmitIsAddress)
          EmitStoreLocalVariableAndLoadItsAddress(il, sourceType);

        il.Emit(OpCodes.Call, sourceType.FindValueGetterMethod());

        if ((parent & ParentFlags.IgnoreResult) != 0)
          il.Emit(OpCodes.Pop);
        return true;
      }

      if (!TryEmit(opExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess))
        return false;

      var targetTypeIsNullable = targetType.IsNullable();
      var underlyingNullableTargetType = Nullable.GetUnderlyingType(targetType);
      if (targetTypeIsNullable && sourceType == underlyingNullableTargetType)
      {
        il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
        return true;
      }

      if (sourceType == targetType || targetType == typeof(object))
      {
        if (targetType == typeof(object) && sourceType.IsValueType())
          il.Emit(OpCodes.Box, sourceType);
        if (IgnoresResult(parent))
          il.Emit(OpCodes.Pop);
        return true;
      }

      // check implicit / explicit conversion operators on source and target types
      // for non-primitives and for non-primitive nullable - #73
      if (!sourceTypeIsNullable && !sourceType.IsPrimitive())
      {
        var actualTargetType = targetTypeIsNullable ? underlyingNullableTargetType : targetType;
        var convertOpMethod = method ?? sourceType.FindConvertOperator(sourceType, actualTargetType);
        if (convertOpMethod != null)
        {
          il.Emit(OpCodes.Call, convertOpMethod);

          if (targetTypeIsNullable)
            il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());

          if ((parent & ParentFlags.IgnoreResult) != 0)
            il.Emit(OpCodes.Pop);

          return true;
        }
      }
      else if (!targetTypeIsNullable)
      {
        var actualSourceType = sourceTypeIsNullable ? underlyingNullableSourceType : sourceType;

        var convertOpMethod = method ?? actualSourceType.FindConvertOperator(actualSourceType, targetType);
        if (convertOpMethod != null)
        {
          if (sourceTypeIsNullable)
          {
            EmitStoreLocalVariableAndLoadItsAddress(il, sourceType);
            il.Emit(OpCodes.Call, sourceType.FindValueGetterMethod());
          }

          il.Emit(OpCodes.Call, convertOpMethod);
          if ((parent & ParentFlags.IgnoreResult) != 0)
            il.Emit(OpCodes.Pop);

          return true;
        }
      }

      if (!targetTypeIsNullable && !targetType.IsPrimitive())
      {
        var actualSourceType = sourceTypeIsNullable ? underlyingNullableSourceType : sourceType;

        // ReSharper disable once ConstantNullCoalescingCondition
        var convertOpMethod = method ?? targetType.FindConvertOperator(actualSourceType, targetType);
        if (convertOpMethod != null)
        {
          if (sourceTypeIsNullable)
          {
            EmitStoreLocalVariableAndLoadItsAddress(il, sourceType);
            il.Emit(OpCodes.Call, sourceType.FindValueGetterMethod());
          }

          il.Emit(OpCodes.Call, convertOpMethod);
          if ((parent & ParentFlags.IgnoreResult) != 0)
            il.Emit(OpCodes.Pop);

          return true;
        }
      }
      else if (!sourceTypeIsNullable)
      {
        var actualTargetType = targetTypeIsNullable ? underlyingNullableTargetType : targetType;
        var convertOpMethod = method ?? actualTargetType.FindConvertOperator(sourceType, actualTargetType);
        if (convertOpMethod != null)
        {
          il.Emit(OpCodes.Call, convertOpMethod);

          if (targetTypeIsNullable)
            il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());

          if ((parent & ParentFlags.IgnoreResult) != 0)
            il.Emit(OpCodes.Pop);

          return true;
        }
      }

      if (sourceType == typeof(object) && targetType.IsValueType())
      {
        il.Emit(OpCodes.Unbox_Any, targetType);
      }
      else if (targetTypeIsNullable)
      {
        // Conversion to Nullable: `new Nullable<T>(T val);`
        if (!sourceTypeIsNullable)
        {
          if (!TryEmitValueConvert(underlyingNullableTargetType, il, isChecked: false))
            return false;

          il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
        }
        else
        {
          var sourceVarIndex = EmitStoreLocalVariableAndLoadItsAddress(il, sourceType);
          il.Emit(OpCodes.Call, sourceType.FindNullableHasValueGetterMethod());

          var labelSourceHasValue = il.DefineLabel();
          il.Emit(OpCodes.Brtrue_S, labelSourceHasValue); // jump where source has a value

          // otherwise, emit and load a `new Nullable<TTarget>()` struct (that's why a Init instead of New)
          EmitLoadLocalVariable(il, InitValueTypeVariable(il, targetType));

          // jump to completion
          var labelDone = il.DefineLabel();
          il.Emit(OpCodes.Br_S, labelDone);

          // if source nullable has a value:
          il.MarkLabel(labelSourceHasValue);
          EmitLoadLocalVariableAddress(il, sourceVarIndex);
          il.Emit(OpCodes.Call, sourceType.FindNullableGetValueOrDefaultMethod());

          if (!TryEmitValueConvert(underlyingNullableTargetType, il,
                expr.NodeType == ExpressionType.ConvertChecked))
          {
            var convertOpMethod = method ?? underlyingNullableTargetType.FindConvertOperator(underlyingNullableSourceType, underlyingNullableTargetType);
            if (convertOpMethod == null)
              return false; // nor conversion nor conversion operator is found
            il.Emit(OpCodes.Call, convertOpMethod);
          }

          il.Emit(OpCodes.Newobj, targetType.GetTypeInfo().DeclaredConstructors.GetFirst());
          il.MarkLabel(labelDone);
        }
      }
      else
      {
        if (targetType.GetTypeInfo().IsEnum)
          targetType = Enum.GetUnderlyingType(targetType);

        // fixes #159
        if (sourceTypeIsNullable)
        {
          EmitStoreLocalVariableAndLoadItsAddress(il, sourceType);
          il.Emit(OpCodes.Call, sourceType.FindValueGetterMethod());
        }

        // cast as the last resort and let's it fail if unlucky
        if (!TryEmitValueConvert(targetType, il, expr.NodeType == ExpressionType.ConvertChecked))
          il.Emit(OpCodes.Castclass, targetType);
      }

      if ((parent & ParentFlags.IgnoreResult) != 0)
        il.Emit(OpCodes.Pop);

      return true;
    }

    private static bool TryEmitValueConvert(Type targetType, ILGenerator il, bool isChecked)
    {
      if (targetType == typeof(int))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_I4 : OpCodes.Conv_I4);
      else if (targetType == typeof(float))
        il.Emit(OpCodes.Conv_R4);
      else if (targetType == typeof(uint))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_U4);
      else if (targetType == typeof(sbyte))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_I1 : OpCodes.Conv_I1);
      else if (targetType == typeof(byte))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_U1 : OpCodes.Conv_U1);
      else if (targetType == typeof(short))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_I2 : OpCodes.Conv_I2);
      else if (targetType == typeof(ushort) || targetType == typeof(char))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_U2 : OpCodes.Conv_U2);
      else if (targetType == typeof(long))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_I8 : OpCodes.Conv_I8);
      else if (targetType == typeof(ulong))
        il.Emit(isChecked ? OpCodes.Conv_Ovf_U8 : OpCodes.Conv_U8);
      else if (targetType == typeof(double))
        il.Emit(OpCodes.Conv_R8);
      else
        return false;
      return true;
    }

    private static bool TryEmitNotNullConstant(
      bool considerClosure, Type exprType, object constantValue, ILGenerator il, ref ClosureInfo closure)
    {
      var constValueType = constantValue.GetType();
      if (considerClosure && IsClosureBoundConstant(constantValue, constValueType.GetTypeInfo()))
      {
        var constItems = closure.Constants.Items;
        var constIndex = closure.Constants.Count - 1;
        while (constIndex != -1 && !ReferenceEquals(constItems[constIndex], constantValue))
          --constIndex;
        if (constIndex == -1)
          return false;

        var varIndex = closure.ConstantUsageThenVarIndex.Items[constIndex] - 1;
        if (varIndex > 0)
          EmitLoadLocalVariable(il, varIndex);
        else
        {
          il.Emit(OpCodes.Ldloc_0); // load constants array from variable
          EmitLoadConstantInt(il, constIndex);
          il.Emit(OpCodes.Ldelem_Ref);
          if (exprType.IsValueType())
            il.Emit(OpCodes.Unbox_Any, exprType);
        }
      }
      else
      {
        if (constantValue is string s)
        {
          il.Emit(OpCodes.Ldstr, s);
          return true;
        }

        if (constantValue is Type t)
        {
          il.Emit(OpCodes.Ldtoken, t);
          il.Emit(OpCodes.Call, _getTypeFromHandleMethod);
          return true;
        }

        // get raw enum type to light
        if (constValueType.GetTypeInfo().IsEnum)
          constValueType = Enum.GetUnderlyingType(constValueType);

        if (!TryEmitNumberConstant(il, constantValue, constValueType))
          return false;
      }

      var underlyingNullableType = Nullable.GetUnderlyingType(exprType);
      if (underlyingNullableType != null)
        il.Emit(OpCodes.Newobj, exprType.GetTypeInfo().DeclaredConstructors.GetFirst());

      // boxing the value type, otherwise we can get a strange result when 0 is treated as Null.
      else if (exprType == typeof(object) && constValueType.IsValueType())
        il.Emit(OpCodes.Box, constantValue.GetType()); // using normal type for Enum instead of underlying type

      return true;
    }

    // todo: can we do something about boxing?
    private static bool TryEmitNumberConstant(ILGenerator il, object constantValue, Type constValueType)
    {
      if (constValueType == typeof(int))
      {
        EmitLoadConstantInt(il, (int)constantValue);
      }
      else if (constValueType == typeof(char))
      {
        EmitLoadConstantInt(il, (char)constantValue);
      }
      else if (constValueType == typeof(short))
      {
        EmitLoadConstantInt(il, (short)constantValue);
      }
      else if (constValueType == typeof(byte))
      {
        EmitLoadConstantInt(il, (byte)constantValue);
      }
      else if (constValueType == typeof(ushort))
      {
        EmitLoadConstantInt(il, (ushort)constantValue);
      }
      else if (constValueType == typeof(sbyte))
      {
        EmitLoadConstantInt(il, (sbyte)constantValue);
      }
      else if (constValueType == typeof(uint))
      {
        unchecked
        {
          EmitLoadConstantInt(il, (int)(uint)constantValue);
        }
      }
      else if (constValueType == typeof(long))
      {
        il.Emit(OpCodes.Ldc_I8, (long)constantValue);
      }
      else if (constValueType == typeof(ulong))
      {
        unchecked
        {
          il.Emit(OpCodes.Ldc_I8, (long)(ulong)constantValue);
        }
      }
      else if (constValueType == typeof(float))
      {
        il.Emit(OpCodes.Ldc_R4, (float)constantValue);
      }
      else if (constValueType == typeof(double))
      {
        il.Emit(OpCodes.Ldc_R8, (double)constantValue);
      }
      else if (constValueType == typeof(bool))
      {
        il.Emit((bool)constantValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
      }
      else if (constValueType == typeof(IntPtr))
      {
        il.Emit(OpCodes.Ldc_I8, ((IntPtr)constantValue).ToInt64());
      }
      else if (constValueType == typeof(UIntPtr))
      {
        unchecked
        {
          il.Emit(OpCodes.Ldc_I8, (long)((UIntPtr)constantValue).ToUInt64());
        }
      }
      else if (constValueType == typeof(decimal))
      {
        EmitDecimalConstant((decimal)constantValue, il);
      }
      else
      {
        return false;
      }

      return true;
    }

    internal static bool TryEmitNumberOne(ILGenerator il, Type type)
    {
      if (type == typeof(int) || type == typeof(char) || type == typeof(short) ||
          type == typeof(byte) || type == typeof(ushort) || type == typeof(sbyte) ||
          type == typeof(uint))
      {
        il.Emit(OpCodes.Ldc_I4_1);
      }
      else if (type == typeof(long) || type == typeof(ulong) ||
               type == typeof(IntPtr) || type == typeof(UIntPtr))
      {
        il.Emit(OpCodes.Ldc_I8, (long)1);
      }
      else if (type == typeof(float))
      {
        il.Emit(OpCodes.Ldc_R4, 1f);
      }
      else if (type == typeof(double))
      {
        il.Emit(OpCodes.Ldc_R8, 1d);
      }
      else
      {
        return false;
      }

      return true;
    }

    internal static void EmitLoadConstantsAndNestedLambdasIntoVars(ILGenerator il, ref ClosureInfo closure)
    {
      // Load constants array field from Closure and store it into the variable
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldfld, ArrayClosureArrayField);
      EmitStoreLocalVariable(il, il.GetNextLocalVarIndex(typeof(object[])));

      var constItems = closure.Constants.Items;
      var constCount = closure.Constants.Count;
      var constUsage = closure.ConstantUsageThenVarIndex.Items;

      int varIndex;
      for (var i = 0; i < constCount; i++)
      {
        if (constUsage[i] > 1)
        {
          il.Emit(OpCodes.Ldloc_0);// load array field variable on a stack
          EmitLoadConstantInt(il, i);
          il.Emit(OpCodes.Ldelem_Ref);

          var varType = constItems[i].GetType();
          if (varType.IsValueType())
            il.Emit(OpCodes.Unbox_Any, varType);

          varIndex = il.GetNextLocalVarIndex(varType);
          constUsage[i] = varIndex + 1; // to distinguish from the default 1
          EmitStoreLocalVariable(il, varIndex);
        }
      }

      var nestedLambdas = closure.NestedLambdas;
      for (var i = 0; i < nestedLambdas.Length; i++)
      {
        var nestedLambda = nestedLambdas[i];
        if (nestedLambda.UsageCountOrVarIndex > 1)
        {
          il.Emit(OpCodes.Ldloc_0);// load array field variable on a stack
          EmitLoadConstantInt(il, constCount + i);
          il.Emit(OpCodes.Ldelem_Ref);
          varIndex = il.GetNextLocalVarIndex(nestedLambda.Lambda.GetType());
          nestedLambda.UsageCountOrVarIndex = varIndex + 1;
          EmitStoreLocalVariable(il, varIndex);
        }
      }
    }

    private static void EmitDecimalConstant(decimal value, ILGenerator il)
    {
      //check if decimal has decimal places, if not use shorter IL code (constructor from int or long)
      if (value % 1 == 0)
      {
        if (value >= int.MinValue && value <= int.MaxValue)
        {
          EmitLoadConstantInt(il, decimal.ToInt32(value));
          il.Emit(OpCodes.Newobj, typeof(decimal).FindSingleParamConstructor(typeof(int)));
          return;
        }

        if (value >= long.MinValue && value <= long.MaxValue)
        {
          il.Emit(OpCodes.Ldc_I8, decimal.ToInt64(value));
          il.Emit(OpCodes.Newobj, typeof(decimal).FindSingleParamConstructor(typeof(long)));
          return;
        }
      }

      if (value == decimal.MinValue)
      {
        il.Emit(OpCodes.Ldsfld, typeof(decimal).GetTypeInfo().GetDeclaredField(nameof(decimal.MinValue)));
        return;
      }

      if (value == decimal.MaxValue)
      {
        il.Emit(OpCodes.Ldsfld, typeof(decimal).GetTypeInfo().GetDeclaredField(nameof(decimal.MaxValue)));
        return;
      }

      var parts = decimal.GetBits(value);
      var sign = (parts[3] & 0x80000000) != 0;
      var scale = (byte)((parts[3] >> 16) & 0x7F);

      EmitLoadConstantInt(il, parts[0]);
      EmitLoadConstantInt(il, parts[1]);
      EmitLoadConstantInt(il, parts[2]);

      il.Emit(sign ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
      EmitLoadConstantInt(il, scale);

      il.Emit(OpCodes.Conv_U1);

      il.Emit(OpCodes.Newobj, _decimalCtor.Value);
    }

    private static readonly Lazy<ConstructorInfo> _decimalCtor = new Lazy<ConstructorInfo>(() =>
    {
      foreach (var ctor in typeof(decimal).GetTypeInfo().DeclaredConstructors)
        if (ctor.GetParameters().Length == 5)
          return ctor;
      return null;
    });

    private static int InitValueTypeVariable(ILGenerator il, Type exprType)
    {
      var locVarIndex = il.GetNextLocalVarIndex(exprType);
      EmitLoadLocalVariableAddress(il, locVarIndex);
      il.Emit(OpCodes.Initobj, exprType);
      return locVarIndex;
    }

    private static bool EmitNewArray(NewArrayExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var arrayType = expr.Type;
      var elems = expr.Expressions;
      var elemType = arrayType.GetElementType();
      if (elemType == null)
        return false;

      var rank = arrayType.GetArrayRank();
      if (rank == 1) // one dimensional
      {
        EmitLoadConstantInt(il, elems.Count);
      }
      else // multi dimensional
      {
        for (var i = 0; i < elems.Count; i++)
          if (!TryEmit(elems[i], paramExprs, il, ref closure, parent, i))
            return false;

        il.Emit(OpCodes.Newobj, arrayType.GetTypeInfo().DeclaredConstructors.GetFirst());
        return true;
      }

      il.Emit(OpCodes.Newarr, elemType);

      var isElemOfValueType = elemType.IsValueType();

      for (int i = 0, n = elems.Count; i < n; i++)
      {
        il.Emit(OpCodes.Dup);
        EmitLoadConstantInt(il, i);

        // loading element address for later copying of value into it.
        if (isElemOfValueType)
          il.Emit(OpCodes.Ldelema, elemType);

        if (!TryEmit(elems[i], paramExprs, il, ref closure, parent))
          return false;

        if (isElemOfValueType)
          il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
        else
          il.Emit(OpCodes.Stelem_Ref);
      }

      return true;
    }

    private static bool TryEmitArrayIndex(Type exprType, ILGenerator il)
    {
      if (exprType.IsValueType())
        il.Emit(OpCodes.Ldelem, exprType);
      else
        il.Emit(OpCodes.Ldelem_Ref);
      return true;
    }

    private static bool EmitMemberInit(MemberInitExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var valueVarIndex = -1;
      if (expr.Type.IsValueType())
        valueVarIndex = il.GetNextLocalVarIndex(expr.Type);

      var newExpr = expr.NewExpression;
      if (newExpr == null)
      {
        if (!TryEmit(expr.Expression, paramExprs, il, ref closure, parent))
          return false;
      }
      else
      {
        var argExprs = newExpr.Arguments;
        for (var i = 0; i < argExprs.Count; i++)
          if (!TryEmit(argExprs[i], paramExprs, il, ref closure, parent, i))
            return false;

        var ctor = newExpr.Constructor;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (ctor != null)
          il.Emit(OpCodes.Newobj, ctor);
        else if (newExpr.Type.IsValueType())
        {
          if (valueVarIndex == -1)
            valueVarIndex = il.GetNextLocalVarIndex(expr.Type);
          EmitLoadLocalVariableAddress(il, valueVarIndex);
          il.Emit(OpCodes.Initobj, newExpr.Type);
        }
        else
          return false; // null constructor and not a value type, better to fallback
      }

      var bindings = expr.Bindings;
      for (var i = 0; i < bindings.Count; i++)
      {
        var binding = bindings[i];
        if (binding.BindingType != MemberBindingType.Assignment)
          return false;

        if (valueVarIndex != -1) // load local value address, to set its members
          EmitLoadLocalVariableAddress(il, valueVarIndex);
        else
          il.Emit(OpCodes.Dup); // duplicate member owner on stack

        if (!TryEmit(((MemberAssignment)binding).Expression, paramExprs, il, ref closure, parent) ||
            !EmitMemberAssign(il, binding.Member))
          return false;
      }

      if (valueVarIndex != -1)
        EmitLoadLocalVariable(il, valueVarIndex);
      return true;
    }

    private static bool EmitMemberAssign(ILGenerator il, MemberInfo member)
    {
      if (member is PropertyInfo prop)
      {
        var method = prop.DeclaringType.FindPropertySetMethod(prop.Name);
        if (method == null)
          return false;

        il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        return true;
      }

      if (member is FieldInfo field)
      {
        il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
        return true;
      }

      return false;
    }

    private static bool TryEmitIncDecAssign(UnaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var operandExpr = expr.Operand;

      MemberExpression memberAccess;
      var useLocalVar = false;
      int localVarIndex, paramIndex = -1;

      var isParameterOrVariable = operandExpr.NodeType == ExpressionType.Parameter;
      var usesResult = (parent & ParentFlags.IgnoreResult) == 0;
      if (isParameterOrVariable)
      {
        localVarIndex = closure.GetDefinedLocalVarOrDefault((ParameterExpression)operandExpr);
        if (localVarIndex != -1)
        {
          EmitLoadLocalVariable(il, localVarIndex);
          useLocalVar = true;
        }
        else
        {
          paramIndex = paramExprs.Count - 1;
          while (paramIndex != -1 && !ReferenceEquals(paramExprs[paramIndex], operandExpr))
            --paramIndex;
          if (paramIndex == -1)
            return false;
          il.Emit(OpCodes.Ldarg, paramIndex + 1);
        }

        memberAccess = null;

      }
      else if (operandExpr.NodeType == ExpressionType.MemberAccess)
      {
        memberAccess = (MemberExpression)operandExpr;

        if (!TryEmitMemberAccess(memberAccess, paramExprs, il, ref closure, parent | ParentFlags.DupMemberOwner))
          return false;

        useLocalVar = memberAccess.Expression != null && (usesResult || memberAccess.Member is PropertyInfo);
        localVarIndex = useLocalVar ? il.GetNextLocalVarIndex(operandExpr.Type) : -1;
      }
      else
        return false;

      switch (expr.NodeType)
      {
        case ExpressionType.PreIncrementAssign:
          il.Emit(OpCodes.Ldc_I4_1);
          il.Emit(OpCodes.Add);
          StoreIncDecValue(il, usesResult, isParameterOrVariable, localVarIndex);
          break;

        case ExpressionType.PostIncrementAssign:
          StoreIncDecValue(il, usesResult, isParameterOrVariable, localVarIndex);
          il.Emit(OpCodes.Ldc_I4_1);
          il.Emit(OpCodes.Add);
          break;

        case ExpressionType.PreDecrementAssign:
          il.Emit(OpCodes.Ldc_I4_1);
          il.Emit(OpCodes.Sub);
          StoreIncDecValue(il, usesResult, isParameterOrVariable, localVarIndex);
          break;

        case ExpressionType.PostDecrementAssign:
          StoreIncDecValue(il, usesResult, isParameterOrVariable, localVarIndex);
          il.Emit(OpCodes.Ldc_I4_1);
          il.Emit(OpCodes.Sub);
          break;
      }

      if (isParameterOrVariable && paramIndex != -1)
        il.Emit(OpCodes.Starg_S, paramIndex + 1);
      else if (isParameterOrVariable || useLocalVar && !usesResult)
        EmitStoreLocalVariable(il, localVarIndex);

      if (isParameterOrVariable)
        return true;

      if (useLocalVar && !usesResult)
        EmitLoadLocalVariable(il, localVarIndex);

      if (!EmitMemberAssign(il, memberAccess.Member))
        return false;

      if (useLocalVar && usesResult)
        EmitLoadLocalVariable(il, localVarIndex);

      return true;
    }

    private static void StoreIncDecValue(ILGenerator il, bool usesResult, bool isVar, int localVarIndex)
    {
      if (!usesResult)
        return;

      if (isVar || localVarIndex == -1)
        il.Emit(OpCodes.Dup);
      else
      {
        EmitStoreLocalVariable(il, localVarIndex);
        EmitLoadLocalVariable(il, localVarIndex);
      }
    }

    private static bool TryEmitAssign(BinaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var left = expr.Left;
      var right = expr.Right;
      var leftNodeType = expr.Left.NodeType;
      var nodeType = expr.NodeType;

      // if this assignment is part of a single body-less expression or the result of a block
      // we should put its result to the evaluation stack before the return, otherwise we are
      // somewhere inside the block, so we shouldn't return with the result
      var flags = parent & ~ParentFlags.IgnoreResult;
      switch (leftNodeType)
      {
        case ExpressionType.Parameter:
          var leftParamExpr = (ParameterExpression)left;

          var paramIndex = paramExprs.Count - 1;
          while (paramIndex != -1 && !ReferenceEquals(paramExprs[paramIndex], leftParamExpr))
            --paramIndex;

          var arithmeticNodeType = nodeType;
          switch (nodeType)
          {
            case ExpressionType.AddAssign:
              arithmeticNodeType = ExpressionType.Add;
              break;
            case ExpressionType.AddAssignChecked:
              arithmeticNodeType = ExpressionType.AddChecked;
              break;
            case ExpressionType.SubtractAssign:
              arithmeticNodeType = ExpressionType.Subtract;
              break;
            case ExpressionType.SubtractAssignChecked:
              arithmeticNodeType = ExpressionType.SubtractChecked;
              break;
            case ExpressionType.MultiplyAssign:
              arithmeticNodeType = ExpressionType.Multiply;
              break;
            case ExpressionType.MultiplyAssignChecked:
              arithmeticNodeType = ExpressionType.MultiplyChecked;
              break;
            case ExpressionType.DivideAssign:
              arithmeticNodeType = ExpressionType.Divide;
              break;
            case ExpressionType.ModuloAssign:
              arithmeticNodeType = ExpressionType.Modulo;
              break;
            case ExpressionType.PowerAssign:
              arithmeticNodeType = ExpressionType.Power;
              break;
            case ExpressionType.AndAssign:
              arithmeticNodeType = ExpressionType.And;
              break;
            case ExpressionType.OrAssign:
              arithmeticNodeType = ExpressionType.Or;
              break;
            case ExpressionType.ExclusiveOrAssign:
              arithmeticNodeType = ExpressionType.ExclusiveOr;
              break;
            case ExpressionType.LeftShiftAssign:
              arithmeticNodeType = ExpressionType.LeftShift;
              break;
            case ExpressionType.RightShiftAssign:
              arithmeticNodeType = ExpressionType.RightShift;
              break;
          }

          if (paramIndex != -1)
          {
            // shift parameter index by one, because the first one will be closure
            if ((closure.Status & ClosureStatus.ShouldBeStaticMethod) == 0)
              ++paramIndex;

            if (leftParamExpr.IsByRef)
            {
              if (paramIndex == 0)
                il.Emit(OpCodes.Ldarg_0);
              else if (paramIndex == 1)
                il.Emit(OpCodes.Ldarg_1);
              else if (paramIndex == 2)
                il.Emit(OpCodes.Ldarg_2);
              else if (paramIndex == 3)
                il.Emit(OpCodes.Ldarg_3);
              else
                il.Emit(OpCodes.Ldarg_S, (byte)paramIndex);
            }

            if (arithmeticNodeType == nodeType)
            {
              if (!TryEmit(right, paramExprs, il, ref closure, flags))
                return false;
            }
            else if (!TryEmitArithmetic(expr, arithmeticNodeType, paramExprs, il, ref closure, parent))
              return false;

            if ((parent & ParentFlags.IgnoreResult) == 0)
              il.Emit(OpCodes.Dup); // duplicate value to assign and return

            if (leftParamExpr.IsByRef)
              EmitByRefStore(il, leftParamExpr.Type);
            else
              il.Emit(OpCodes.Starg_S, paramIndex);

            return true;
          }
          else if (arithmeticNodeType != nodeType)
          {
            var localVarIdx = closure.GetDefinedLocalVarOrDefault(leftParamExpr);
            if (localVarIdx != -1)
            {
              if (!TryEmitArithmetic(expr, arithmeticNodeType, paramExprs, il, ref closure, parent))
                return false;

              EmitStoreLocalVariable(il, localVarIdx);
              return true;
            }
          }

          // if parameter isn't passed, then it is passed into some outer lambda or it is a local variable,
          // so it should be loaded from closure or from the locals. Then the closure is null will be an invalid state.
          // if it's a local variable, then store the right value in it
          var localVariableIdx = closure.GetDefinedLocalVarOrDefault(leftParamExpr);
          if (localVariableIdx != -1)
          {
            if (!TryEmit(right, paramExprs, il, ref closure, flags))
              return false;

            if ((right as ParameterExpression)?.IsByRef == true)
              il.Emit(OpCodes.Ldind_I4);

            if ((parent & ParentFlags.IgnoreResult) == 0) // if we have to push the result back, duplicate the right value
              il.Emit(OpCodes.Dup);

            EmitStoreLocalVariable(il, localVariableIdx);
            return true;
          }

          // check that it's a captured parameter by closure
          var nonPassedParams = closure.NonPassedParameters;
          var nonPassedParamIndex = nonPassedParams.Length - 1;
          while (nonPassedParamIndex != -1 &&
                 !ReferenceEquals(nonPassedParams[nonPassedParamIndex], leftParamExpr))
            --nonPassedParamIndex;
          if (nonPassedParamIndex == -1)
            return false; // what??? no chance

          il.Emit(OpCodes.Ldarg_0); // closure is always a first argument

          if ((parent & ParentFlags.IgnoreResult) == 0)
          {
            if (!TryEmit(right, paramExprs, il, ref closure, flags))
              return false;

            var valueVarIndex = il.GetNextLocalVarIndex(expr.Type); // store left value in variable
            EmitStoreLocalVariable(il, valueVarIndex);

            // load array field and param item index
            il.Emit(OpCodes.Ldfld, ArrayClosureWithNonPassedParamsField);
            EmitLoadConstantInt(il, nonPassedParamIndex);
            EmitLoadLocalVariable(il, valueVarIndex);
            if (expr.Type.IsValueType())
              il.Emit(OpCodes.Box, expr.Type);
            il.Emit(OpCodes.Stelem_Ref); // put the variable into array
            EmitLoadLocalVariable(il, valueVarIndex);
          }
          else
          {
            // load array field and param item index
            il.Emit(OpCodes.Ldfld, ArrayClosureWithNonPassedParamsField);
            EmitLoadConstantInt(il, nonPassedParamIndex);

            if (!TryEmit(right, paramExprs, il, ref closure, flags))
              return false;

            if (expr.Type.IsValueType())
              il.Emit(OpCodes.Box, expr.Type);
            il.Emit(OpCodes.Stelem_Ref); // put the variable into array
          }

          return true;

        case ExpressionType.MemberAccess:
          var assignFromLocalVar = right.NodeType == ExpressionType.Try;

          var resultLocalVarIndex = -1;
          if (assignFromLocalVar)
          {
            resultLocalVarIndex = il.GetNextLocalVarIndex(right.Type);

            if (!TryEmit(right, paramExprs, il, ref closure, ParentFlags.Empty))
              return false;

            EmitStoreLocalVariable(il, resultLocalVarIndex);
          }

          var memberExpr = (MemberExpression)left;
          var objExpr = memberExpr.Expression;
          if (objExpr != null &&
              !TryEmit(objExpr, paramExprs, il, ref closure, flags | ParentFlags.MemberAccess | ParentFlags.InstanceAccess))
            return false;

          if (assignFromLocalVar)
            EmitLoadLocalVariable(il, resultLocalVarIndex);
          else if (!TryEmit(right, paramExprs, il, ref closure, ParentFlags.Empty))
            return false;

          var member = memberExpr.Member;
          if ((parent & ParentFlags.IgnoreResult) != 0)
            return EmitMemberAssign(il, member);

          il.Emit(OpCodes.Dup);

          var rightVarIndex = il.GetNextLocalVarIndex(expr.Type); // store right value in variable
          EmitStoreLocalVariable(il, rightVarIndex);

          if (!EmitMemberAssign(il, member))
            return false;

          EmitLoadLocalVariable(il, rightVarIndex);
          return true;

        case ExpressionType.Index:
          var indexExpr = (IndexExpression)left;

          var obj = indexExpr.Object;
          if (obj != null && !TryEmit(obj, paramExprs, il, ref closure, flags))
            return false;

          var indexArgExprs = indexExpr.Arguments;
          for (var i = 0; i < indexArgExprs.Count; i++)
            if (!TryEmit(indexArgExprs[i], paramExprs, il, ref closure, flags, i))
              return false;

          if (!TryEmit(right, paramExprs, il, ref closure, flags))
            return false;

          if ((parent & ParentFlags.IgnoreResult) != 0)
            return TryEmitIndexAssign(indexExpr, obj?.Type, expr.Type, il);

          var varIndex = il.GetNextLocalVarIndex(expr.Type); // store value in variable to return
          il.Emit(OpCodes.Dup);
          EmitStoreLocalVariable(il, varIndex);

          if (!TryEmitIndexAssign(indexExpr, obj?.Type, expr.Type, il))
            return false;

          EmitLoadLocalVariable(il, varIndex);
          return true;

        default: // todo: not yet support assignment targets
          return false;
      }
    }

    private static void EmitByRefStore(ILGenerator il, Type type)
    {
      if (type == typeof(int) || type == typeof(uint))
        il.Emit(OpCodes.Stind_I4);
      else if (type == typeof(byte))
        il.Emit(OpCodes.Stind_I1);
      else if (type == typeof(short) || type == typeof(ushort))
        il.Emit(OpCodes.Stind_I2);
      else if (type == typeof(long) || type == typeof(ulong))
        il.Emit(OpCodes.Stind_I8);
      else if (type == typeof(float))
        il.Emit(OpCodes.Stind_R4);
      else if (type == typeof(double))
        il.Emit(OpCodes.Stind_R8);
      else if (type == typeof(object))
        il.Emit(OpCodes.Stind_Ref);
      else if (type == typeof(IntPtr) || type == typeof(UIntPtr))
        il.Emit(OpCodes.Stind_I);
      else
        il.Emit(OpCodes.Stobj, type);
    }

    private static bool TryEmitIndexAssign(IndexExpression indexExpr, Type instType, Type elementType, ILGenerator il)
    {
      if (indexExpr.Indexer != null)
        return EmitMemberAssign(il, indexExpr.Indexer);

      if (indexExpr.Arguments.Count == 1) // one dimensional array
      {
        if (elementType.IsValueType())
          il.Emit(OpCodes.Stelem, elementType);
        else
          il.Emit(OpCodes.Stelem_Ref);
        return true;
      }

      // multi dimensional array
      return EmitMethodCall(il, instType?.FindMethod("Set"));
    }

    private static bool TryEmitMethodCall(Expression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var flags = parent & ~ParentFlags.IgnoreResult | ParentFlags.Call;
      var callExpr = (MethodCallExpression)expr;
      var objExpr = callExpr.Object;
      var method = callExpr.Method;
      var methodParams = method.GetParameters();
      var objIsValueType = false;
      if (objExpr != null)
      {
        if (!TryEmit(objExpr, paramExprs, il, ref closure, flags | ParentFlags.InstanceAccess))
          return false;

        objIsValueType = objExpr.Type.IsValueType();
        if (objIsValueType && objExpr.NodeType != ExpressionType.Parameter && !closure.LastEmitIsAddress)
          EmitStoreLocalVariableAndLoadItsAddress(il, objExpr.Type);
      }

      var fewArgCount = callExpr.FewArgumentCount;
      if (fewArgCount >= 0)
      {
        if (fewArgCount == 1)
        {
          if (!TryEmit(((OneArgumentMethodCallExpression)callExpr).Argument, paramExprs, il, ref closure, flags, methodParams[0].ParameterType.IsByRef ? 0 : -1))
            return false;
        }
        else if (fewArgCount == 2)
        {
          var twoArgsExpr = (TwoArgumentsMethodCallExpression)callExpr;
          if (!TryEmit(twoArgsExpr.Argument0, paramExprs, il, ref closure, flags, methodParams[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(twoArgsExpr.Argument1, paramExprs, il, ref closure, flags, methodParams[1].ParameterType.IsByRef ? 1 : -1))
            return false;
        }
        else if (fewArgCount == 3)
        {
          var threeArgsExpr = (ThreeArgumentsMethodCallExpression)callExpr;
          if (!TryEmit(threeArgsExpr.Argument0, paramExprs, il, ref closure, flags, methodParams[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(threeArgsExpr.Argument1, paramExprs, il, ref closure, flags, methodParams[1].ParameterType.IsByRef ? 1 : -1) ||
              !TryEmit(threeArgsExpr.Argument2, paramExprs, il, ref closure, flags, methodParams[2].ParameterType.IsByRef ? 2 : -1))
            return false;
        }
        else if (fewArgCount == 4)
        {
          var fourArgsExpr = (FourArgumentsMethodCallExpression)callExpr;
          if (!TryEmit(fourArgsExpr.Argument0, paramExprs, il, ref closure, flags, methodParams[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(fourArgsExpr.Argument1, paramExprs, il, ref closure, flags, methodParams[1].ParameterType.IsByRef ? 1 : -1) ||
              !TryEmit(fourArgsExpr.Argument2, paramExprs, il, ref closure, flags, methodParams[2].ParameterType.IsByRef ? 2 : -1) ||
              !TryEmit(fourArgsExpr.Argument3, paramExprs, il, ref closure, flags, methodParams[3].ParameterType.IsByRef ? 3 : -1))
            return false;
        }
        else if (fewArgCount == 5)
        {
          var fiveArgsExpr = (FiveArgumentsMethodCallExpression)callExpr;
          if (!TryEmit(fiveArgsExpr.Argument0, paramExprs, il, ref closure, flags, methodParams[0].ParameterType.IsByRef ? 0 : -1) ||
              !TryEmit(fiveArgsExpr.Argument1, paramExprs, il, ref closure, flags, methodParams[1].ParameterType.IsByRef ? 1 : -1) ||
              !TryEmit(fiveArgsExpr.Argument2, paramExprs, il, ref closure, flags, methodParams[2].ParameterType.IsByRef ? 2 : -1) ||
              !TryEmit(fiveArgsExpr.Argument3, paramExprs, il, ref closure, flags, methodParams[3].ParameterType.IsByRef ? 3 : -1) ||
              !TryEmit(fiveArgsExpr.Argument4, paramExprs, il, ref closure, flags, methodParams[4].ParameterType.IsByRef ? 4 : -1))
            return false;
        }

        if (objIsValueType && method.IsVirtual)
          il.Emit(OpCodes.Constrained, objExpr.Type);
        il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        if (parent.IgnoresResult() && method.ReturnType != typeof(void))
          il.Emit(OpCodes.Pop);
        closure.LastEmitIsAddress = false;
        return true;
      }

      var args = callExpr.Arguments;
      for (var i = 0; i < methodParams.Length; i++)
        if (!TryEmit(args[i], paramExprs, il, ref closure, flags, methodParams[i].ParameterType.IsByRef ? i : -1))
          return false;

      if (objIsValueType && method.IsVirtual)
        il.Emit(OpCodes.Constrained, objExpr.Type);
      il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
      if (parent.IgnoresResult() && method.ReturnType != typeof(void))
        il.Emit(OpCodes.Pop);

      closure.LastEmitIsAddress = false;
      return true;
    }

    private static bool TryEmitMemberAccess(MemberExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      if (expr.Member is PropertyInfo prop)
      {
        var instanceExpr = expr.Expression;
        if (instanceExpr != null)
        {
          if (!TryEmit(instanceExpr, paramExprs, il, ref closure,
                ~ParentFlags.IgnoreResult & ~ParentFlags.DupMemberOwner &
                (parent | ParentFlags.Call | ParentFlags.MemberAccess | ParentFlags.InstanceAccess)))
            return false;

          if ((parent & ParentFlags.DupMemberOwner) != 0)
            il.Emit(OpCodes.Dup);

          // Value type special treatment to load address of value instance in order to access a field or call a method.
          // Parameter should be excluded because it already loads an address via `LDARGA`, and you don't need to.
          // And for field access no need to load address, cause the field stored on stack nearby
          if (!closure.LastEmitIsAddress &&
              instanceExpr.NodeType != ExpressionType.Parameter && instanceExpr.Type.IsValueType())
            EmitStoreLocalVariableAndLoadItsAddress(il, instanceExpr.Type);
        }

        closure.LastEmitIsAddress = false;
        var propGetter = prop.DeclaringType.FindPropertyGetMethod(prop.Name);
        if (propGetter == null)
          return false;

        il.Emit(propGetter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, propGetter);
        return true;
      }

      if (expr.Member is FieldInfo field)
      {
        var instanceExpr = expr.Expression;
        if (instanceExpr != null)
        {
          if (!TryEmit(instanceExpr, paramExprs, il, ref closure,
                ~ParentFlags.IgnoreResult & ~ParentFlags.DupMemberOwner &
                (parent | ParentFlags.MemberAccess | ParentFlags.InstanceAccess)))
            return false;

          if ((parent & ParentFlags.DupMemberOwner) != 0)
            il.Emit(OpCodes.Dup);

          closure.LastEmitIsAddress = field.FieldType.IsValueType() && (parent & ParentFlags.InstanceAccess) != 0;
          il.Emit(closure.LastEmitIsAddress ? OpCodes.Ldflda : OpCodes.Ldfld, field);
        }
        else if (field.IsLiteral)
        {
          var fieldValue = field.GetValue(null);
          if (fieldValue != null)
            return TryEmitNotNullConstant(false, field.FieldType, fieldValue, il, ref closure);

          il.Emit(OpCodes.Ldnull);
        }
        else
        {
          il.Emit(OpCodes.Ldsfld, field);
        }

        return true;
      }

      return false;
    }

    // ReSharper disable once FunctionComplexityOverflow
    private static bool TryEmitNestedLambda(LambdaExpression lambdaExpr,
      IReadOnlyList<ParameterExpression> outerParamExprs, ILGenerator il, ref ClosureInfo closure)
    {
      // First, find in closed compiled lambdas the one corresponding to the current lambda expression.
      // Situation with not found lambda is not possible/exceptional,
      // it means that we somehow skipped the lambda expression while collecting closure info.
      var outerNestedLambdas = closure.NestedLambdas;
      var outerNestedLambdaIndex = outerNestedLambdas.Length - 1;
      while (outerNestedLambdaIndex != -1 &&
             !ReferenceEquals(outerNestedLambdas[outerNestedLambdaIndex].LambdaExpression, lambdaExpr))
        --outerNestedLambdaIndex;
      if (outerNestedLambdaIndex == -1)
        return false;

      var nestedLambdaInfo = closure.NestedLambdas[outerNestedLambdaIndex];
      var nestedLambda = nestedLambdaInfo.Lambda;
      var nestedLambdaInClosureIndex = outerNestedLambdaIndex + closure.Constants.Count;

      var varIndex = nestedLambdaInfo.UsageCountOrVarIndex - 1;
      if (varIndex > 0)
        EmitLoadLocalVariable(il, varIndex);
      else
      {
        il.Emit(OpCodes.Ldloc_0);
        EmitLoadConstantInt(il, nestedLambdaInClosureIndex);
        il.Emit(OpCodes.Ldelem_Ref); // load the array item object
      }

      // If lambda does not use any outer parameters to be set in closure, then we're done
      ref var nestedClosureInfo = ref nestedLambdaInfo.ClosureInfo;
      var nestedNonPassedParams = nestedClosureInfo.NonPassedParameters;
      if (nestedNonPassedParams.Length == 0)
        return true;

      //-------------------------------------------------------------------
      // For the lambda with non-passed parameters (or variables) in closure
      // we have loaded `NestedLambdaWithConstantsAndNestedLambdas` pair.
      if (varIndex > 0)
      {
        // we are already have variable loaded
        il.Emit(OpCodes.Ldfld, NestedLambdaWithConstantsAndNestedLambdas.NestedLambdaField);
        EmitLoadLocalVariable(il, varIndex); // load the variable for the second time
        il.Emit(OpCodes.Ldfld, NestedLambdaWithConstantsAndNestedLambdas.ConstantsAndNestedLambdasField);
      }
      else
      {
        var nestedLambdaAndClosureItemsVarIndex = il.GetNextLocalVarIndex(typeof(NestedLambdaWithConstantsAndNestedLambdas));
        EmitStoreLocalVariable(il, nestedLambdaAndClosureItemsVarIndex);

        // - load the `NestedLambda` field
        EmitLoadLocalVariable(il, nestedLambdaAndClosureItemsVarIndex);
        il.Emit(OpCodes.Ldfld, NestedLambdaWithConstantsAndNestedLambdas.NestedLambdaField);

        // - load the `ConstantsAndNestedLambdas` field
        EmitLoadLocalVariable(il, nestedLambdaAndClosureItemsVarIndex);
        il.Emit(OpCodes.Ldfld, NestedLambdaWithConstantsAndNestedLambdas.ConstantsAndNestedLambdasField);
      }

      // - create `NonPassedParameters` array
      EmitLoadConstantInt(il, nestedNonPassedParams.Length); // size of array
      il.Emit(OpCodes.Newarr, typeof(object));

      // - populate the `NonPassedParameters` array
      var outerNonPassedParams = closure.NonPassedParameters;
      for (var nestedParamIndex = 0; nestedParamIndex < nestedNonPassedParams.Length; ++nestedParamIndex)
      {
        var nestedParam = nestedNonPassedParams[nestedParamIndex];

        // Duplicate nested array on stack to store the item, and load index to where to store
        il.Emit(OpCodes.Dup);
        EmitLoadConstantInt(il, nestedParamIndex);

        var outerParamIndex = outerParamExprs.Count - 1;
        while (outerParamIndex != -1 && !ReferenceEquals(outerParamExprs[outerParamIndex], nestedParam))
          --outerParamIndex;
        if (outerParamIndex != -1) // load parameter from input outer params
        {
          // Add `+1` to index because the `0` index is for the closure argument
          if (outerParamIndex == 0)
            il.Emit(OpCodes.Ldarg_1);
          else if (outerParamIndex == 1)
            il.Emit(OpCodes.Ldarg_2);
          else if (outerParamIndex == 2)
            il.Emit(OpCodes.Ldarg_3);
          else
            il.Emit(OpCodes.Ldarg_S, (byte)(1 + outerParamIndex));

          if (nestedParam.Type.IsValueType())
            il.Emit(OpCodes.Box, nestedParam.Type);
        }
        else // load parameter from outer closure or from the local variables
        {
          if (outerNonPassedParams.Length == 0)
            return false; // impossible, better to throw?

          var variableIdx = closure.GetDefinedLocalVarOrDefault(nestedParam);
          if (variableIdx != -1) // it's a local variable
          {
            EmitLoadLocalVariable(il, variableIdx);
          }
          else // it's a parameter from the outer closure
          {
            var outerNonPassedParamIndex = outerNonPassedParams.Length - 1;
            while (outerNonPassedParamIndex != -1 && !ReferenceEquals(outerNonPassedParams[outerNonPassedParamIndex], nestedParam))
              --outerNonPassedParamIndex;
            if (outerNonPassedParamIndex == -1)
              return false; // impossible

            // Load the parameter from outer closure `Items` array
            il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
            il.Emit(OpCodes.Ldfld, ArrayClosureWithNonPassedParamsField);
            EmitLoadConstantInt(il, outerNonPassedParamIndex);
            il.Emit(OpCodes.Ldelem_Ref);
          }
        }

        // Store the item into nested lambda array
        il.Emit(OpCodes.Stelem_Ref);
      }

      // - create `ArrayClosureWithNonPassedParams` out of the both above
      il.Emit(OpCodes.Newobj, ArrayClosureWithNonPassedParamsConstructor);

      // - call `Curry` method with nested lambda and array closure to produce a closed lambda with the expected signature
      var lambdaTypeArgs = nestedLambda.GetType().GetTypeInfo().GenericTypeArguments;

      var closureMethod = nestedLambdaInfo.LambdaExpression.ReturnType == typeof(void)
        ? CurryClosureActions.Methods[lambdaTypeArgs.Length - 1].MakeGenericMethod(lambdaTypeArgs)
        : CurryClosureFuncs.Methods[lambdaTypeArgs.Length - 2].MakeGenericMethod(lambdaTypeArgs);

      il.Emit(OpCodes.Call, closureMethod);
      return true;
    }

    private static bool TryEmitInvoke(InvocationExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var lambda = expr.Expression;
      if (!TryEmit(lambda, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
        return false;

      var argExprs = expr.Arguments;
      if (argExprs.Count > 0)
        for (var i = 0; i < argExprs.Count; i++)
          if (!TryEmit(argExprs[i], paramExprs, il, ref closure,
                parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess,
                argExprs[i].Type.IsByRef ? i : -1))
            return false;

      var delegateInvokeMethod = lambda.Type.FindDelegateInvokeMethod();
      il.Emit(OpCodes.Call, delegateInvokeMethod);

      if ((parent & ParentFlags.IgnoreResult) != 0 && delegateInvokeMethod.ReturnType != typeof(void))
        il.Emit(OpCodes.Pop);

      return true;
    }

    private static bool TryEmitSwitch(SwitchExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      // todo:
      //- use switch statement for int comparison (if int difference is less or equal 3 -> use IL switch)
      //- TryEmitComparison should not emit "CEQ" so we could use Beq_S instead of Brtrue_S (not always possible (nullable))
      //- if switch SwitchValue is a nullable parameter, we should call getValue only once and store the result.
      //- use comparison methods (when defined)

      var endLabel = il.DefineLabel();
      var labels = new Label[expr.Cases.Count];
      for (var index = 0; index < expr.Cases.Count; index++)
      {
        var switchCase = expr.Cases[index];
        labels[index] = il.DefineLabel();

        foreach (var switchCaseTestValue in switchCase.TestValues)
        {
          if (!TryEmitComparison(expr.SwitchValue, switchCaseTestValue, ExpressionType.Equal, paramExprs, il,
                ref closure, parent))
            return false;
          il.Emit(OpCodes.Brtrue, labels[index]);
        }
      }

      if (expr.DefaultBody != null)
      {
        if (!TryEmit(expr.DefaultBody, paramExprs, il, ref closure, parent))
          return false;
        il.Emit(OpCodes.Br, endLabel);
      }

      for (var index = 0; index < expr.Cases.Count; index++)
      {
        var switchCase = expr.Cases[index];
        il.MarkLabel(labels[index]);
        if (!TryEmit(switchCase.Body, paramExprs, il, ref closure, parent))
          return false;

        if (index != expr.Cases.Count - 1)
          il.Emit(OpCodes.Br, endLabel);
      }

      il.MarkLabel(endLabel);

      return true;
    }

    private static bool TryEmitComparison(Expression exprLeft, Expression exprRight, ExpressionType expressionType,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var leftOpType = exprLeft.Type;
      var leftIsNullable = leftOpType.IsNullable();
      var rightOpType = exprRight.Type;
      if (exprRight is ConstantExpression c && c.Value == null && exprRight.Type == typeof(object))
        rightOpType = leftOpType;

      int lVarIndex = -1, rVarIndex = -1;
      if (!TryEmit(exprLeft, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess))
        return false;

      if (leftIsNullable)
      {
        lVarIndex = EmitStoreLocalVariableAndLoadItsAddress(il, leftOpType);
        il.Emit(OpCodes.Call, leftOpType.FindNullableGetValueOrDefaultMethod());
        leftOpType = Nullable.GetUnderlyingType(leftOpType);
      }

      if (!TryEmit(exprRight, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceAccess))
        return false;

      if (leftOpType != rightOpType)
      {
        if (leftOpType.IsClass() && rightOpType.IsClass() &&
            (leftOpType == typeof(object) || rightOpType == typeof(object)))
        {
          if (expressionType == ExpressionType.Equal)
          {
            il.Emit(OpCodes.Ceq);
            if ((parent & ParentFlags.IgnoreResult) != 0)
              il.Emit(OpCodes.Pop);
          }
          else if (expressionType == ExpressionType.NotEqual)
          {
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
          }
          else
            return false;

          if ((parent & ParentFlags.IgnoreResult) != 0)
            il.Emit(OpCodes.Pop);

          return true;
        }
      }

      if (rightOpType.IsNullable())
      {
        rVarIndex = EmitStoreLocalVariableAndLoadItsAddress(il, rightOpType);
        il.Emit(OpCodes.Call, rightOpType.FindNullableGetValueOrDefaultMethod());
        // ReSharper disable once AssignNullToNotNullAttribute
        rightOpType = Nullable.GetUnderlyingType(rightOpType);
      }

      var leftOpTypeInfo = leftOpType.GetTypeInfo();
      if (!leftOpTypeInfo.IsPrimitive && !leftOpTypeInfo.IsEnum)
      {
        var methodName
          = expressionType == ExpressionType.Equal ? "op_Equality"
          : expressionType == ExpressionType.NotEqual ? "op_Inequality"
          : expressionType == ExpressionType.GreaterThan ? "op_GreaterThan"
          : expressionType == ExpressionType.GreaterThanOrEqual ? "op_GreaterThanOrEqual"
          : expressionType == ExpressionType.LessThan ? "op_LessThan"
          : expressionType == ExpressionType.LessThanOrEqual ? "op_LessThanOrEqual"
          : null;

        if (methodName == null)
          return false;

        // todo: for now handling only parameters of the same type
        var methods = leftOpTypeInfo.DeclaredMethods.AsArray();
        for (var i = 0; i < methods.Length; i++)
        {
          var m = methods[i];
          if (m.IsSpecialName && m.IsStatic && m.Name == methodName &&
              IsComparisonOperatorSignature(leftOpType, m.GetParameters()))
          {
            il.Emit(OpCodes.Call, m);
            return true;
          }
        }

        if (expressionType != ExpressionType.Equal && expressionType != ExpressionType.NotEqual)
          return false;

        il.Emit(OpCodes.Call, _objectEqualsMethod);

        if (expressionType == ExpressionType.NotEqual) // invert result for not equal
        {
          il.Emit(OpCodes.Ldc_I4_0);
          il.Emit(OpCodes.Ceq);
        }

        if (leftIsNullable)
          goto nullCheck;

        if ((parent & ParentFlags.IgnoreResult) > 0)
          il.Emit(OpCodes.Pop);

        return true;
      }

      // handle primitives comparison
      switch (expressionType)
      {
        case ExpressionType.Equal:
          il.Emit(OpCodes.Ceq);
          break;

        case ExpressionType.NotEqual:
          il.Emit(OpCodes.Ceq);
          il.Emit(OpCodes.Ldc_I4_0);
          il.Emit(OpCodes.Ceq);
          break;

        case ExpressionType.LessThan:
          il.Emit(OpCodes.Clt);
          break;

        case ExpressionType.GreaterThan:
          il.Emit(OpCodes.Cgt);
          break;

        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.LessThanOrEqual:
          var ifTrueLabel = il.DefineLabel();
          if (rightOpType == typeof(uint) || rightOpType == typeof(ulong) ||
              rightOpType == typeof(ushort) || rightOpType == typeof(byte))
            il.Emit(expressionType == ExpressionType.GreaterThanOrEqual ? OpCodes.Bge_Un_S : OpCodes.Ble_Un_S, ifTrueLabel);
          else
            il.Emit(expressionType == ExpressionType.GreaterThanOrEqual ? OpCodes.Bge_S : OpCodes.Ble_S, ifTrueLabel);

          il.Emit(OpCodes.Ldc_I4_0);
          var doneLabel = il.DefineLabel();
          il.Emit(OpCodes.Br_S, doneLabel);

          il.MarkLabel(ifTrueLabel);
          il.Emit(OpCodes.Ldc_I4_1);

          il.MarkLabel(doneLabel);
          break;

        default:
          return false;
      }

    nullCheck:
      if (leftIsNullable)
      {
        var leftNullableHasValueGetterMethod = exprLeft.Type.FindNullableHasValueGetterMethod();

        EmitLoadLocalVariableAddress(il, lVarIndex);
        il.Emit(OpCodes.Call, leftNullableHasValueGetterMethod);

        // ReSharper disable once AssignNullToNotNullAttribute
        EmitLoadLocalVariableAddress(il, rVarIndex);
        il.Emit(OpCodes.Call, leftNullableHasValueGetterMethod);

        switch (expressionType)
        {
          case ExpressionType.Equal:
            il.Emit(OpCodes.Ceq); // compare both HasValue calls
            il.Emit(OpCodes.And); // both results need to be true
            break;

          case ExpressionType.NotEqual:
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Or);
            break;

          case ExpressionType.LessThan:
          case ExpressionType.GreaterThan:
          case ExpressionType.LessThanOrEqual:
          case ExpressionType.GreaterThanOrEqual:
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.And);
            break;

          default:
            return false;
        }
      }

      if ((parent & ParentFlags.IgnoreResult) > 0)
        il.Emit(OpCodes.Pop);

      return true;
    }

    private static bool IsComparisonOperatorSignature(Type t, ParameterInfo[] pars) =>
      pars.Length == 2 && pars[0].ParameterType == t && pars[1].ParameterType == t;

    private static bool TryEmitArithmetic(BinaryExpression expr, ExpressionType exprNodeType,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure,
      ParentFlags parent)
    {
      var flags = parent & ~ParentFlags.IgnoreResult & ~ParentFlags.InstanceCall | ParentFlags.Arithmetic;

      var leftNoValueLabel = default(Label);
      var leftExpr = expr.Left;
      var lefType = leftExpr.Type;
      var leftIsNullable = lefType.IsNullable();
      if (leftIsNullable)
      {
        leftNoValueLabel = il.DefineLabel();
        if (!TryEmit(leftExpr, paramExprs, il, ref closure, flags | ParentFlags.InstanceCall))
          return false;

        if (!closure.LastEmitIsAddress)
          EmitStoreLocalVariableAndLoadItsAddress(il, lefType);

        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Call, lefType.FindNullableHasValueGetterMethod());

        il.Emit(OpCodes.Brfalse, leftNoValueLabel);
        il.Emit(OpCodes.Call, lefType.FindNullableGetValueOrDefaultMethod());
      }
      else if (!TryEmit(leftExpr, paramExprs, il, ref closure, flags))
        return false;

      var rightNoValueLabel = default(Label);
      var rightExpr = expr.Right;
      var rightType = rightExpr.Type;
      var rightIsNullable = rightType.IsNullable();
      if (rightIsNullable)
      {
        rightNoValueLabel = il.DefineLabel();
        if (!TryEmit(rightExpr, paramExprs, il, ref closure, flags | ParentFlags.InstanceCall))
          return false;

        if (!closure.LastEmitIsAddress)
          EmitStoreLocalVariableAndLoadItsAddress(il, rightType);

        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Call, rightType.FindNullableHasValueGetterMethod());
        il.Emit(OpCodes.Brfalse, rightNoValueLabel);
        il.Emit(OpCodes.Call, rightType.FindNullableGetValueOrDefaultMethod());
      }
      else if (!TryEmit(rightExpr, paramExprs, il, ref closure, flags))
        return false;

      var exprType = expr.Type;
      if (!TryEmitArithmeticOperation(expr, exprNodeType, exprType, il))
        return false;

      if (leftIsNullable || rightIsNullable)
      {
        var valueLabel = il.DefineLabel();
        il.Emit(OpCodes.Br, valueLabel);

        if (rightIsNullable)
          il.MarkLabel(rightNoValueLabel);
        il.Emit(OpCodes.Pop);

        if (leftIsNullable)
          il.MarkLabel(leftNoValueLabel);
        il.Emit(OpCodes.Pop);

        if (exprType.IsNullable())
        {
          var endL = il.DefineLabel();
          var locIndex = InitValueTypeVariable(il, exprType);
          EmitLoadLocalVariable(il, locIndex);
          il.Emit(OpCodes.Br_S, endL);
          il.MarkLabel(valueLabel);
          il.Emit(OpCodes.Newobj, exprType.GetTypeInfo().DeclaredConstructors.GetFirst());
          il.MarkLabel(endL);
        }
        else
        {
          il.Emit(OpCodes.Ldc_I4_0);
          il.MarkLabel(valueLabel);
        }
      }

      return true;
    }

    private static bool TryEmitArithmeticOperation(BinaryExpression expr,
      ExpressionType exprNodeType, Type exprType, ILGenerator il)
    {
      if (!exprType.IsPrimitive())
      {
        if (exprType.IsNullable())
          exprType = Nullable.GetUnderlyingType(exprType);

        if (!exprType.IsPrimitive())
        {
          MethodInfo method = null;
          if (exprType == typeof(string))
          {
            var paraType = typeof(string);
            if (expr.Left.Type != expr.Right.Type || expr.Left.Type != typeof(string))
              paraType = typeof(object);

            var methods = typeof(string).GetTypeInfo().DeclaredMethods.AsArray();
            for (var i = 0; i < methods.Length; i++)
            {
              var m = methods[i];
              if (m.IsStatic && m.Name == "Concat" &&
                  m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == paraType)
              {
                method = m;
                break;
              }
            }
          }
          else
          {
            var methodName
              = exprNodeType == ExpressionType.Add ? "op_Addition"
              : exprNodeType == ExpressionType.AddChecked ? "op_Addition"
              : exprNodeType == ExpressionType.Subtract ? "op_Subtraction"
              : exprNodeType == ExpressionType.SubtractChecked ? "op_Subtraction"
              : exprNodeType == ExpressionType.Multiply ? "op_Multiply"
              : exprNodeType == ExpressionType.MultiplyChecked ? "op_Multiply"
              : exprNodeType == ExpressionType.Divide ? "op_Division"
              : exprNodeType == ExpressionType.Modulo ? "op_Modulus"
              : null;

            if (methodName != null)
            {
              var methods = exprType.GetTypeInfo().DeclaredMethods.AsArray();
              for (var i = 0; method == null && i < methods.Length; i++)
              {
                var m = methods[i];
                if (m.IsSpecialName && m.IsStatic && m.Name == methodName)
                  method = m;
              }
            }
          }

          if (method == null)
            return false;

          il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
          return true;
        }
      }

      switch (exprNodeType)
      {
        case ExpressionType.Add:
        case ExpressionType.AddAssign:
          il.Emit(OpCodes.Add);
          return true;

        case ExpressionType.AddChecked:
        case ExpressionType.AddAssignChecked:
          il.Emit(exprType.IsUnsigned() ? OpCodes.Add_Ovf_Un : OpCodes.Add_Ovf);
          return true;

        case ExpressionType.Subtract:
        case ExpressionType.SubtractAssign:
          il.Emit(OpCodes.Sub);
          return true;

        case ExpressionType.SubtractChecked:
        case ExpressionType.SubtractAssignChecked:
          il.Emit(exprType.IsUnsigned() ? OpCodes.Sub_Ovf_Un : OpCodes.Sub_Ovf);
          return true;

        case ExpressionType.Multiply:
        case ExpressionType.MultiplyAssign:
          il.Emit(OpCodes.Mul);
          return true;

        case ExpressionType.MultiplyChecked:
        case ExpressionType.MultiplyAssignChecked:
          il.Emit(exprType.IsUnsigned() ? OpCodes.Mul_Ovf_Un : OpCodes.Mul_Ovf);
          return true;

        case ExpressionType.Divide:
        case ExpressionType.DivideAssign:
          il.Emit(OpCodes.Div);
          return true;

        case ExpressionType.Modulo:
        case ExpressionType.ModuloAssign:
          il.Emit(OpCodes.Rem);
          return true;

        case ExpressionType.And:
        case ExpressionType.AndAssign:
          il.Emit(OpCodes.And);
          return true;

        case ExpressionType.Or:
        case ExpressionType.OrAssign:
          il.Emit(OpCodes.Or);
          return true;

        case ExpressionType.ExclusiveOr:
        case ExpressionType.ExclusiveOrAssign:
          il.Emit(OpCodes.Xor);
          return true;

        case ExpressionType.LeftShift:
        case ExpressionType.LeftShiftAssign:
          il.Emit(OpCodes.Shl);
          return true;

        case ExpressionType.RightShift:
        case ExpressionType.RightShiftAssign:
          il.Emit(OpCodes.Shr);
          return true;

        case ExpressionType.Power:
          il.Emit(OpCodes.Call, typeof(Math).FindMethod("Pow"));
          return true;
      }

      return false;
    }

    private static bool TryEmitLogicalOperator(BinaryExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      if (!TryEmit(expr.Left, paramExprs, il, ref closure, parent))
        return false;

      var labelSkipRight = il.DefineLabel();
      il.Emit(expr.NodeType == ExpressionType.AndAlso ? OpCodes.Brfalse : OpCodes.Brtrue, labelSkipRight);

      if (!TryEmit(expr.Right, paramExprs, il, ref closure, parent))
        return false;

      var labelDone = il.DefineLabel();
      il.Emit(OpCodes.Br, labelDone);

      il.MarkLabel(labelSkipRight); // label the second branch
      il.Emit(expr.NodeType == ExpressionType.AndAlso ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
      il.MarkLabel(labelDone);

      return true;
    }

    private static bool TryEmitConditional(ConditionalExpression expr,
      IReadOnlyList<ParameterExpression> paramExprs, ILGenerator il, ref ClosureInfo closure, ParentFlags parent)
    {
      var testExpr = TryReduceCondition(expr.Test);

      // detect a special simplistic case of comparison with `null`
      var comparedWithNull = false;
      if (testExpr is BinaryExpression b)
      {
        if (b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual ||
            !b.Left.Type.IsNullable() && !b.Right.Type.IsNullable())
        {
          if (b.Right is ConstantExpression r && r.Value == null)
          {
            // the null comparison for nullable is actually a `nullable.HasValue` check,
            // which implies member access on nullable struct - therefore loading it by address
            if (b.Left.Type.IsNullable())
              parent |= ParentFlags.MemberAccess;
            comparedWithNull = TryEmit(b.Left, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult);
          }
          else if (b.Left is ConstantExpression l && l.Value == null)
          {
            // the null comparison for nullable is actually a `nullable.HasValue` check,
            // which implies member access on nullable struct - therefore loading it by address
            if (b.Right.Type.IsNullable())
              parent |= ParentFlags.MemberAccess;
            comparedWithNull = TryEmit(b.Right, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult);
          }
        }
      }

      if (!comparedWithNull)
      {
        if (!TryEmit(testExpr, paramExprs, il, ref closure, parent & ~ParentFlags.IgnoreResult))
          return false;
      }

      var labelIfFalse = il.DefineLabel();
      il.Emit(comparedWithNull && testExpr.NodeType == ExpressionType.Equal ? OpCodes.Brtrue : OpCodes.Brfalse, labelIfFalse);

      var ifTrueExpr = expr.IfTrue;
      if (!TryEmit(ifTrueExpr, paramExprs, il, ref closure, parent & ParentFlags.IgnoreResult))
        return false;

      var ifFalseExpr = expr.IfFalse;
      if (ifFalseExpr.NodeType == ExpressionType.Default && ifFalseExpr.Type == typeof(void))
      {
        il.MarkLabel(labelIfFalse);
        return true;
      }

      var labelDone = il.DefineLabel();
      il.Emit(OpCodes.Br, labelDone);

      il.MarkLabel(labelIfFalse);
      if (!TryEmit(ifFalseExpr, paramExprs, il, ref closure, parent & ParentFlags.IgnoreResult))
        return false;

      il.MarkLabel(labelDone);
      return true;
    }

    private static Expression TryReduceCondition(Expression testExpr)
    {
      if (testExpr is BinaryExpression b)
      {
        if (b.NodeType == ExpressionType.OrElse || b.NodeType == ExpressionType.Or)
        {
          if (b.Left is ConstantExpression l && l.Value is bool lb)
            return lb ? b.Left : TryReduceCondition(b.Right);

          if (b.Right is ConstantExpression r && r.Value is bool rb && rb == false)
            return TryReduceCondition(b.Left);
        }
        else if (b.NodeType == ExpressionType.AndAlso || b.NodeType == ExpressionType.And)
        {
          if (b.Left is ConstantExpression l && l.Value is bool lb)
            return !lb ? b.Left : TryReduceCondition(b.Right);

          if (b.Right is ConstantExpression r && r.Value is bool rb && rb)
            return TryReduceCondition(b.Left);
        }
      }

      return testExpr;
    }

    private static bool EmitMethodCall(ILGenerator il, MethodInfo method, ParentFlags parent = ParentFlags.Empty)
    {
      if (method == null)
        return false;

      il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);

      if ((parent & ParentFlags.IgnoreResult) != 0 && method.ReturnType != typeof(void))
        il.Emit(OpCodes.Pop);
      return true;
    }

    private static void EmitLoadConstantInt(ILGenerator il, int i)
    {
      switch (i)
      {
        case -1:
          il.Emit(OpCodes.Ldc_I4_M1);
          break;
        case 0:
          il.Emit(OpCodes.Ldc_I4_0);
          break;
        case 1:
          il.Emit(OpCodes.Ldc_I4_1);
          break;
        case 2:
          il.Emit(OpCodes.Ldc_I4_2);
          break;
        case 3:
          il.Emit(OpCodes.Ldc_I4_3);
          break;
        case 4:
          il.Emit(OpCodes.Ldc_I4_4);
          break;
        case 5:
          il.Emit(OpCodes.Ldc_I4_5);
          break;
        case 6:
          il.Emit(OpCodes.Ldc_I4_6);
          break;
        case 7:
          il.Emit(OpCodes.Ldc_I4_7);
          break;
        case 8:
          il.Emit(OpCodes.Ldc_I4_8);
          break;
        default:
          if (i > -129 && i < 128)
            il.Emit(OpCodes.Ldc_I4_S, (sbyte)i);
          else
            il.Emit(OpCodes.Ldc_I4, i);
          break;
      }
    }

    private static void EmitLoadLocalVariableAddress(ILGenerator il, int location)
    {
      if (location < 256)
        il.Emit(OpCodes.Ldloca_S, (byte)location);
      else
        il.Emit(OpCodes.Ldloca, location);
    }

    private static void EmitLoadLocalVariable(ILGenerator il, int location)
    {
      if (location == 0)
        il.Emit(OpCodes.Ldloc_0);
      else if (location == 1)
        il.Emit(OpCodes.Ldloc_1);
      else if (location == 2)
        il.Emit(OpCodes.Ldloc_2);
      else if (location == 3)
        il.Emit(OpCodes.Ldloc_3);
      else if (location < 256)
        il.Emit(OpCodes.Ldloc_S, (byte)location);
      else
        il.Emit(OpCodes.Ldloc, location);
    }

    private static void EmitStoreLocalVariable(ILGenerator il, int location)
    {
      if (location == 0)
        il.Emit(OpCodes.Stloc_0);
      else if (location == 1)
        il.Emit(OpCodes.Stloc_1);
      else if (location == 2)
        il.Emit(OpCodes.Stloc_2);
      else if (location == 3)
        il.Emit(OpCodes.Stloc_3);
      else if (location < 256)
        il.Emit(OpCodes.Stloc_S, (byte)location);
      else
        il.Emit(OpCodes.Stloc, location);
    }

    private static int EmitStoreLocalVariableAndLoadItsAddress(ILGenerator il, Type type)
    {
      var varIndex = il.GetNextLocalVarIndex(type);
      if (varIndex == 0)
      {
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Ldloca_S, (byte)0);
      }
      else if (varIndex == 1)
      {
        il.Emit(OpCodes.Stloc_1);
        il.Emit(OpCodes.Ldloca_S, (byte)1);
      }
      else if (varIndex == 2)
      {
        il.Emit(OpCodes.Stloc_2);
        il.Emit(OpCodes.Ldloca_S, (byte)2);
      }
      else if (varIndex == 3)
      {
        il.Emit(OpCodes.Stloc_3);
        il.Emit(OpCodes.Ldloca_S, (byte)3);
      }
      else if (varIndex < 256)
      {
        il.Emit(OpCodes.Stloc_S, (byte)varIndex);
        il.Emit(OpCodes.Ldloca_S, (byte)varIndex);
      }
      else
      {
        il.Emit(OpCodes.Stloc, varIndex);
        il.Emit(OpCodes.Ldloca, varIndex);
      }

      return varIndex;
    }
  }
}

// Helpers targeting the performance. Extensions method names may be a bit funny (non standard), 
// in order to prevent conflicts with YOUR helpers with standard names
internal static class Tools
{
  internal static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;
  internal static bool IsPrimitive(this Type type) => type.GetTypeInfo().IsPrimitive;
  internal static bool IsClass(this Type type) => type.GetTypeInfo().IsClass;

  internal static bool IsUnsigned(this Type type) =>
    type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

  internal static bool IsNullable(this Type type) =>
    type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>);

  internal static MethodInfo FindMethod(this Type type, string methodName)
  {
    var methods = type.GetTypeInfo().DeclaredMethods.AsArray();
    for (var i = 0; i < methods.Length; i++)
      if (methods[i].Name == methodName)
        return methods[i];

    return type.GetTypeInfo().BaseType?.FindMethod(methodName);
  }

  internal static MethodInfo DelegateTargetGetterMethod = typeof(Delegate).FindPropertyGetMethod("Target");

  internal static MethodInfo FindDelegateInvokeMethod(this Type type) => type.FindMethod("Invoke");

  internal static MethodInfo FindNullableGetValueOrDefaultMethod(this Type type)
  {
    var methods = type.GetTypeInfo().DeclaredMethods.AsArray();
    for (var i = 0; i < methods.Length; i++)
    {
      var m = methods[i];
      if (m.GetParameters().Length == 0 && m.Name == "GetValueOrDefault")
        return m;
    }

    return null;
  }

  internal static MethodInfo FindValueGetterMethod(this Type type) =>
    type.FindPropertyGetMethod("Value");

  internal static MethodInfo FindNullableHasValueGetterMethod(this Type type) =>
    type.FindPropertyGetMethod("HasValue");

  internal static MethodInfo FindPropertyGetMethod(this Type propHolderType, string propName)
  {
    var methods = propHolderType.GetTypeInfo().DeclaredMethods.AsArray();
    for (var i = 0; i < methods.Length; i++)
    {
      var method = methods[i];
      if (method.IsSpecialName)
      {
        var methodName = method.Name;
        if (methodName.Length == propName.Length + 4 && methodName[0] == 'g' && methodName[3] == '_')
        {
          var j = propName.Length - 1;
          while (j != -1 && propName[j] == methodName[j + 4]) --j;
          if (j == -1)
            return method;
        }
      }
    }

    return propHolderType.GetTypeInfo().BaseType?.FindPropertyGetMethod(propName);
  }

  internal static MethodInfo FindPropertySetMethod(this Type propHolderType, string propName)
  {
    var methods = propHolderType.GetTypeInfo().DeclaredMethods.AsArray();
    for (var i = 0; i < methods.Length; i++)
    {
      var method = methods[i];
      if (method.IsSpecialName)
      {
        var methodName = method.Name;
        if (methodName.Length == propName.Length + 4 && methodName[0] == 's' && methodName[3] == '_')
        {
          var j = propName.Length - 1;
          while (j != -1 && propName[j] == methodName[j + 4]) --j;
          if (j == -1)
            return method;
        }
      }
    }

    return propHolderType.GetTypeInfo().BaseType?.FindPropertySetMethod(propName);
  }

  internal static MethodInfo FindConvertOperator(this Type type, Type sourceType, Type targetType)
  {
    var methods = type.GetTypeInfo().DeclaredMethods.AsArray();
    for (var i = 0; i < methods.Length; i++)
    {
      var m = methods[i];
      if (m.IsStatic && m.IsSpecialName && m.ReturnType == targetType)
      {
        var n = m.Name;
        // n == "op_Implicit" || n == "op_Explicit"
        if (n.Length == 11 &&
            n[2] == '_' && n[5] == 'p' && n[6] == 'l' && n[7] == 'i' && n[8] == 'c' && n[9] == 'i' && n[10] == 't' &&
            m.GetParameters()[0].ParameterType == sourceType)
          return m;
      }
    }

    return null;
  }

  internal static ConstructorInfo FindSingleParamConstructor(this Type type, Type paramType)
  {
    var ctors = type.GetTypeInfo().DeclaredConstructors.AsArray();
    for (var i = 0; i < ctors.Length; i++)
    {
      var ctor = ctors[i];
      var parameters = ctor.GetParameters();
      if (parameters.Length == 1 && parameters[0].ParameterType == paramType)
        return ctor;
    }

    return null;
  }

  public static T[] AsArray<T>(this IEnumerable<T> xs)
  {
    if (xs is T[] array)
      return array;
    return xs == null ? null : xs.ToArray();
  }

  private static class EmptyArray<T>
  {
    public static readonly T[] Value = new T[0];
  }

  public static T[] Empty<T>() => EmptyArray<T>.Value;

  public static T[] WithLast<T>(this T[] source, T value)
  {
    if (source == null || source.Length == 0)
      return new[] { value };
    if (source.Length == 1)
      return new[] { source[0], value };
    if (source.Length == 2)
      return new[] { source[0], source[1], value };
    var sourceLength = source.Length;
    var result = new T[sourceLength + 1];
    Array.Copy(source, 0, result, 0, sourceLength);
    result[sourceLength] = value;
    return result;
  }

  public static Type[] GetParamTypes(IReadOnlyList<ParameterExpression> paramExprs)
  {
    if (paramExprs == null || paramExprs.Count == 0)
      return Empty<Type>();

    if (paramExprs.Count == 1)
      return new[] { paramExprs[0].IsByRef ? paramExprs[0].Type.MakeByRefType() : paramExprs[0].Type };

    var paramTypes = new Type[paramExprs.Count];
    for (var i = 0; i < paramTypes.Length; i++)
    {
      var parameterExpr = paramExprs[i];
      paramTypes[i] = parameterExpr.IsByRef ? parameterExpr.Type.MakeByRefType() : parameterExpr.Type;
    }

    return paramTypes;
  }

  public static Type GetFuncOrActionType(Type[] paramTypes, Type returnType)
  {
    if (returnType == typeof(void))
    {
      switch (paramTypes.Length)
      {
        case 0: return typeof(Action);
        case 1: return typeof(Action<>).MakeGenericType(paramTypes);
        case 2: return typeof(Action<,>).MakeGenericType(paramTypes);
        case 3: return typeof(Action<,,>).MakeGenericType(paramTypes);
        case 4: return typeof(Action<,,,>).MakeGenericType(paramTypes);
        case 5: return typeof(Action<,,,,>).MakeGenericType(paramTypes);
        case 6: return typeof(Action<,,,,,>).MakeGenericType(paramTypes);
        case 7: return typeof(Action<,,,,,,>).MakeGenericType(paramTypes);
        default:
          throw new NotSupportedException(
            $"Action with so many ({paramTypes.Length}) parameters is not supported!");
      }
    }

    switch (paramTypes.Length)
    {
      case 0: return typeof(Func<>).MakeGenericType(returnType);
      case 1: return typeof(Func<,>).MakeGenericType(paramTypes[0], returnType);
      case 2: return typeof(Func<,,>).MakeGenericType(paramTypes[0], paramTypes[1], returnType);
      case 3: return typeof(Func<,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], returnType);
      case 4: return typeof(Func<,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], returnType);
      case 5: return typeof(Func<,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], paramTypes[4], returnType);
      case 6: return typeof(Func<,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], paramTypes[4], paramTypes[5], returnType);
      case 7: return typeof(Func<,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], paramTypes[4], paramTypes[5], paramTypes[6], returnType);
      default:
        throw new NotSupportedException(
          $"Func with so many ({paramTypes.Length}) parameters is not supported!");
    }
  }

  public static T GetFirst<T>(this IEnumerable<T> source)
  {
    // This is pretty much Linq.FirstOrDefault except it does not need to check
    // if source is IPartition<T> (but should it?)

    if (source is IList<T> list)
      return list.Count == 0 ? default : list[0];
    using (var items = source.GetEnumerator())
      return items.MoveNext() ? items.Current : default;
  }

  public static T GetFirst<T>(this IList<T> source)
  {
    return source.Count == 0 ? default : source[0];
  }

  public static T GetFirst<T>(this T[] source)
  {
    return source.Length == 0 ? default : source[0];
  }
}

/// <summary>Reflecting the internal methods to access the more performant for defining the local variable</summary>
public static class ILGeneratorHacks
{
  // The original ILGenerator methods we are trying to hack without allocating the `LocalBuilder`
  /*
  public virtual LocalBuilder DeclareLocal(Type localType)
  {
      return this.DeclareLocal(localType, false);
  }

  public virtual LocalBuilder DeclareLocal(Type localType, bool pinned)
  {
      MethodBuilder methodBuilder = this.m_methodBuilder as MethodBuilder;
      if ((MethodInfo)methodBuilder == (MethodInfo)null)
          throw new NotSupportedException();
      if (methodBuilder.IsTypeCreated())
          throw new InvalidOperationException(SR.InvalidOperation_TypeHasBeenCreated);
      if (localType == (Type)null)
          throw new ArgumentNullException(nameof(localType));
      if (methodBuilder.m_bIsBaked)
          throw new InvalidOperationException(SR.InvalidOperation_MethodBaked);
      this.m_localSignature.AddArgument(localType, pinned);
      LocalBuilder localBuilder = new LocalBuilder(this.m_localCount, localType, (MethodInfo)methodBuilder, pinned);
      ++this.m_localCount;
      return localBuilder;
  }
  */

  private static readonly Func<ILGenerator, Type, int> _getNextLocalVarIndex;

  internal static int PostInc(ref int i) => i++;

  static ILGeneratorHacks()
  {
    // the default allocatee method
    _getNextLocalVarIndex = (i, t) => i.DeclareLocal(t).LocalIndex;

    // now let's try to acquire the more efficient less allocating method
    var ilGenTypeInfo = typeof(ILGenerator).GetTypeInfo();
    var localSignatureField = ilGenTypeInfo.GetDeclaredField("m_localSignature");
    if (localSignatureField == null)
      return;

    var localCountField = ilGenTypeInfo.GetDeclaredField("m_localCount");
    if (localCountField == null)
      return;

    // looking for the `SignatureHelper.AddArgument(Type argument, bool pinned)`
    MethodInfo addArgumentMethod = null;
    foreach (var m in typeof(SignatureHelper).GetTypeInfo().GetDeclaredMethods("AddArgument"))
    {
      var ps = m.GetParameters();
      if (ps.Length == 2 && ps[0].ParameterType == typeof(Type) && ps[1].ParameterType == typeof(bool))
      {
        addArgumentMethod = m;
        break;
      }
    }

    if (addArgumentMethod == null)
      return;

    // our own helper - always available
    var postIncMethod = typeof(ILGeneratorHacks).GetTypeInfo().GetDeclaredMethod(nameof(PostInc));

    // now let's compile the following method without allocating the LocalBuilder class:
    /*
         il.m_localSignature.AddArgument(type);
         return PostInc(ref il.LocalCount);
    */
    var efficientMethod = new DynamicMethod(string.Empty,
      typeof(int), new[] { typeof(ExpressionCompiler.ArrayClosure), typeof(ILGenerator), typeof(Type) },
      typeof(ExpressionCompiler.ArrayClosure), skipVisibility: true);
    var il = efficientMethod.GetILGenerator();

    // emitting `il.m_localSignature.AddArgument(type);`
    il.Emit(OpCodes.Ldarg_1);  // load `il` argument (arg_0 is the empty closure object)
    il.Emit(OpCodes.Ldfld, localSignatureField);
    il.Emit(OpCodes.Ldarg_2);  // load `type` argument
    il.Emit(OpCodes.Ldc_I4_0); // load `pinned: false` argument
    il.Emit(OpCodes.Call, addArgumentMethod);

    // emitting `return PostInc(ref il.LocalCount);`
    il.Emit(OpCodes.Ldarg_1); // load `il` argument
    il.Emit(OpCodes.Ldflda, localCountField);
    il.Emit(OpCodes.Call, postIncMethod);

    il.Emit(OpCodes.Ret);

    _getNextLocalVarIndex = (Func<ILGenerator, Type, int>)efficientMethod.CreateDelegate(
      typeof(Func<ILGenerator, Type, int>), ExpressionCompiler.EmptyArrayClosure);
  }

  /// <summary>Efficiently returns the next variable index, hopefully without unnecessary allocations.</summary>
  public static int GetNextLocalVarIndex(this ILGenerator il, Type t) => _getNextLocalVarIndex(il, t);
}

internal struct LiveCountArray<T>
{
  public int Count;
  public T[] Items;

  public LiveCountArray(T[] items)
  {
    Items = items;
    Count = items.Length;
  }

  public ref T PushSlot()
  {
    if (++Count > Items.Length)
      Items = Expand(Items);
    return ref Items[Count - 1];
  }

  public void PushSlot(T item)
  {
    if (++Count > Items.Length)
      Items = Expand(Items);
    Items[Count - 1] = item;
  }

  public void Pop() => --Count;

  public static T[] Expand(T[] items)
  {
    if (items.Length == 0)
      return new T[4];

    var count = items.Length;
    var newItems = new T[count << 1]; // count x 2
    Array.Copy(items, 0, newItems, 0, count);
    return newItems;
  }
}