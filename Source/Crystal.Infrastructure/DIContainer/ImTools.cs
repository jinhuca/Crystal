﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Crystal.Utilities
{
  /// <summary>Helpers for functional composition</summary>
  public static class Fun
  {
    /// <summary>
    /// Always a true condition.
    /// </summary> 
    public static bool Always<T>(T _) => true;

    /// <summary>
    /// Identity function returning passed argument as result.
    /// </summary> 
    public static T Id<T>(T x) => x;

    /// <summary>
    /// Forward pipe operator (`|>` in F#)
    /// </summary> 
    public static R To<T, R>(this T x, Func<T, R> map) => map(x);

    /// <summary>
    /// Forward pipe operator (`|>` in F#) with the additional state A for two arguments function
    /// </summary> 
    public static R To<T, S, R>(this T x, S state, Func<T, S, R> map) => map(x, state);

    /// <summary>
    /// Cast to the R type with the forward pipe operator (`|>` in F#)
    /// </summary> 
    public static R To<R>(this object x) => (R)x;

    /// <summary>
    /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value
    /// </summary> 
    public static T Do<T>(this T x, Action<T> effect)
    {
      effect(x);
      return x;
    }

    /// <summary>
    /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value and the state object
    /// </summary> 
    public static T Do<T, S>(this T x, S state, Action<T, S> effect)
    {
      effect(x, state);
      return x;
    }

    /// <summary>
    /// Lifts argument to Func without allocations ignoring the first argument.
    /// For example if you have `Func{T, R} = _ => instance`,
    /// you may rewrite it without allocations as `instance.ToFunc{A, R}`
    /// </summary> 
    public static R ToFunc<T, R>(this R result, T ignoredArg) => result;
  }

  /// <summary>Helpers for lazy instantiations</summary>
  public static class Lazy
  {
    /// <summary>Provides result type inference for creation of lazy.</summary>
    public static Lazy<T> Of<T>(Func<T> valueFactory) => new Lazy<T>(valueFactory);
  }

  /// Replacement for `Void` type which can be used as a type argument and value.
  /// In traditional functional languages this type is a singleton empty record type,
  /// e.g. `()` in Haskell https://en.wikipedia.org/wiki/Unit_type
  public struct Unit : IEquatable<Unit>
  {
    /// Singleton unit value - making it a lower-case so you could import `using static ImTools.Unit;` and write `return unit;`
    public static readonly Unit unit = new Unit();

    /// <inheritdoc />
    public override string ToString() => "(unit)";

    /// Equals to any other Unit
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is Unit;

    /// Using type hash code for the value
    public override int GetHashCode() => typeof(Unit).GetHashCode();
  }

  /// Simple value provider interface - useful for the type pattern matching via `case I{T} x: ...`
  public interface I<out T>
  {
    /// The value in this case ;)
    T Value { get; }
  }

  /// Helpers for `Is` and `Union`
  public static class UnionTools
  {
    /// Pretty prints the Union using the type information
    internal static string ToString<TName, T>(T value, string prefix = "case(", string suffix = ")")
    {
      if (typeof(TName) == typeof(Unit))
        return prefix + value + suffix;

      var typeName = typeof(TName).Name;
      var i = typeName.IndexOf('`');
      var name = i == -1 ? typeName : typeName.Substring(0, i);
      return name + prefix + value + suffix;
    }
  }

  /// Wraps the `T` in a typed `TData` struct value in a one-line declaration,
  /// so the <c><![CDATA[class Name : Case<Name, string>]]></c>
  /// is different from the <c><![CDATA[class Address : Case<Address, string>]]></c> 
  public abstract class Item<TItem, T> where TItem : Item<TItem, T>
  {
    /// Creation method for the consistency with other types
    public static item Of(T x) => new item(x);

    /// Nested structure that hosts a value.
    /// All nested types by convention here are lowercase
    public readonly struct item : IEquatable<item>, I<T>
    {
      /// <inheritdoc />
      public T Value { [MethodImpl((MethodImplOptions)256)] get => Item; }

      /// The value
      public readonly T Item;

      /// Constructor
      public item(T x) => Item = x;

      /// <inheritdoc />
      public bool Equals(item other) => EqualityComparer<T>.Default.Equals(Value, other.Value);

      /// <inheritdoc />
      public override bool Equals(object obj) => obj is item c && Equals(c);

      /// <inheritdoc />
      public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);

      /// <inheritdoc />
      public override string ToString() => UnionTools.ToString<TItem, T>(Value);
    }
  }

  /// Item without the data payload
  public abstract class Item<TItem> where TItem : Item<TItem>
  {
    /// Single item value
    public static readonly item Single = new item();

    /// Nested structure that hosts a value.
    /// All nested types by convention here are lowercase
    public readonly struct item : IEquatable<item>
    {
      /// <inheritdoc />
      public bool Equals(item other) => true;

      /// <inheritdoc />
      public override bool Equals(object obj) => obj is item;

      /// <inheritdoc />
      public override int GetHashCode() => typeof(TItem).GetHashCode();

      /// <inheritdoc />
      public override string ToString() => "(" + typeof(TItem).Name + ")";
    }
  }

  /// Wraps the `T` in a named `TBox` class in a one-line declaration,
  /// so the <c><![CDATA[class Name : Data<Name, string>]]></c>
  /// is different from the <c><![CDATA[class Address : Data<Address, string>]]></c> 
  public abstract class Box<TBox, T> : I<T>, IEquatable<Box<TBox, T>>
      where TBox : Box<TBox, T>, new()
  {
    /// Wraps the value
    public static TBox Of(T x) => new TBox { Value = x };

    /// <inheritdoc />
    public T Value { get; private set; }

    /// <inheritdoc />
    public bool Equals(Box<TBox, T> other) =>
        other != null && EqualityComparer<T>.Default.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is Box<TBox, T> c && Equals(c);

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    /// <inheritdoc />
    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);

    /// <inheritdoc />
    public override string ToString() => UnionTools.ToString<TBox, T>(Value, "data(");
  }

  /// Unnamed discriminated union (with Empty name), shorter name for simplified inline usage
  public class U<T1, T2> : Union<Unit, T1, T2> { }

  /// Discriminated union
  public abstract class Union<TUnion, T1, T2>
  {
    /// To tag the cases with enum value for efficient pattern matching of required -
    /// otherwise we need to use `is CaseN` pattern or similar which is less efficient
    public enum Tag : byte
    {
      /// Tags Case1
      Case1,
      /// Tags Case2
      Case2
    }

    /// The base interface for the cases to operate.
    /// The naming is selected to start from the lower letter, cause we need to use the nested type.
    /// It is an unusual case, that's why using the __union__ will be fine to highlight this.
    // ReSharper disable once InconsistentNaming
    public interface union
    {
      /// The tag
      Tag Tag { get; }

      /// Matches the union cases to the R value
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2);
    }

    /// Creates the respective case
    public static union Of(T1 x) => new case1(x);

    /// Creates the respective case
    public static union Of(T2 x) => new case2(x);

    /// Wraps the respective case
    public readonly struct case1 : union, IEquatable<case1>, I<T1>
    {
      /// Implicit conversion
      public static implicit operator case1(T1 x) => new case1(x);

      /// <inheritdoc />
      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      /// <inheritdoc />
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2) => map1(Case);

      /// <inheritdoc />
      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      /// The case value
      public readonly T1 Case;

      /// Wraps the value
      public case1(T1 x) => Case = x;

      /// <inheritdoc />
      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Value, other.Value);

      /// <inheritdoc />
      public override bool Equals(object obj) => obj is case1 x && Equals(x);

      /// <inheritdoc />
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Value);

      /// <inheritdoc />
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Value);
    }

    /// Wraps the respective case
    public readonly struct case2 : union, IEquatable<case2>, I<T2>
    {
      /// Conversion
      public static implicit operator case2(T2 x) => new case2(x);

      /// <inheritdoc />
      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }

      /// <inheritdoc />
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2) => map2(Value);

      /// <inheritdoc />
      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      /// The case value
      public readonly T2 Case;

      /// Wraps the value
      public case2(T2 x) => Case = x;

      /// <inheritdoc />
      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Value, other.Value);

      /// <inheritdoc />
      public override bool Equals(object obj) => obj is case2 x && Equals(x);

      /// <inheritdoc />
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Value);

      /// <inheritdoc />
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Value);
    }
  }

#pragma warning disable 1591
  public class U<T1, T2, T3> : Union<Unit, T1, T2, T3> { }

  public abstract class Union<TUnion, T1, T2, T3>
  {
    public enum Tag : byte { Case1, Case2, Case3 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }
  }

  public class U<T1, T2, T3, T4> : Union<Unit, T1, T2, T3, T4> { }
  public abstract class Union<TUnion, T1, T2, T3, T4>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }
  }

  public class U<T1, T2, T3, T4, T5> : Union<Unit, T1, T2, T3, T4, T5> { }
  public abstract class Union<TUnion, T1, T2, T3, T4, T5>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4, Case5 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);
    public static union Of(T5 x) => new case5(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }

    public struct case5 : union, IEquatable<case5>, I<T5>
    {
      public static implicit operator case5(T5 x) => new case5(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case5; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5) => map5(Case);

      public T5 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T5 Case;
      public case5(T5 x) => Case = x;

      public bool Equals(case5 other) => EqualityComparer<T5>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case5 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T5>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T5>(Case);
    }
  }

  public class U<T1, T2, T3, T4, T5, T6> : Union<Unit, T1, T2, T3, T4, T5, T6> { }
  public abstract class Union<TUnion, T1, T2, T3, T4, T5, T6>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4, Case5, Case6 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);
    public static union Of(T5 x) => new case5(x);
    public static union Of(T6 x) => new case6(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }

    public struct case5 : union, IEquatable<case5>, I<T5>
    {
      public static implicit operator case5(T5 x) => new case5(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case5; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6) => map5(Case);

      public T5 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T5 Case;
      public case5(T5 x) => Case = x;

      public bool Equals(case5 other) => EqualityComparer<T5>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case5 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T5>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T5>(Case);
    }

    public struct case6 : union, IEquatable<case6>, I<T6>
    {
      public static implicit operator case6(T6 x) => new case6(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case6; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6) => map6(Case);

      public T6 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T6 Case;
      public case6(T6 x) => Case = x;

      public bool Equals(case6 other) => EqualityComparer<T6>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case6 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T6>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T6>(Case);
    }
  }

  public class U<T1, T2, T3, T4, T5, T6, T7> : Union<Unit, T1, T2, T3, T4, T5, T6, T7> { }
  public abstract class Union<TUnion, T1, T2, T3, T4, T5, T6, T7>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4, Case5, Case6, Case7 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);
    public static union Of(T5 x) => new case5(x);
    public static union Of(T6 x) => new case6(x);
    public static union Of(T7 x) => new case7(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }

    public struct case5 : union, IEquatable<case5>, I<T5>
    {
      public static implicit operator case5(T5 x) => new case5(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case5; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map5(Case);

      public T5 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T5 Case;
      public case5(T5 x) => Case = x;

      public bool Equals(case5 other) => EqualityComparer<T5>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case5 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T5>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T5>(Case);
    }

    public struct case6 : union, IEquatable<case6>, I<T6>
    {
      public static implicit operator case6(T6 x) => new case6(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case6; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map6(Case);

      public T6 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T6 Case;
      public case6(T6 x) => Case = x;

      public bool Equals(case6 other) => EqualityComparer<T6>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case6 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T6>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T6>(Case);
    }

    public struct case7 : union, IEquatable<case7>, I<T7>
    {
      public static implicit operator case7(T7 x) => new case7(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case7; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7) => map7(Case);

      public T7 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T7 Case;
      public case7(T7 x) => Case = x;

      public bool Equals(case7 other) => EqualityComparer<T7>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case7 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T7>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T7>(Case);
    }
  }

  public abstract class Union<TUnion, T1, T2, T3, T4, T5, T6, T7, T8>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4, Case5, Case6, Case7, Case8 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);
    public static union Of(T5 x) => new case5(x);
    public static union Of(T6 x) => new case6(x);
    public static union Of(T7 x) => new case7(x);
    public static union Of(T8 x) => new case8(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }

    public struct case5 : union, IEquatable<case5>, I<T5>
    {
      public static implicit operator case5(T5 x) => new case5(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case5; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map5(Case);

      public T5 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T5 Case;
      public case5(T5 x) => Case = x;

      public bool Equals(case5 other) => EqualityComparer<T5>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case5 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T5>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T5>(Case);
    }

    public struct case6 : union, IEquatable<case6>, I<T6>
    {
      public static implicit operator case6(T6 x) => new case6(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case6; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map6(Case);

      public T6 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T6 Case;
      public case6(T6 x) => Case = x;

      public bool Equals(case6 other) => EqualityComparer<T6>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case6 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T6>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T6>(Case);
    }

    public struct case7 : union, IEquatable<case7>, I<T7>
    {
      public static implicit operator case7(T7 x) => new case7(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case7; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map7(Case);

      public T7 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T7 Case;
      public case7(T7 x) => Case = x;

      public bool Equals(case7 other) => EqualityComparer<T7>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case7 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T7>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T7>(Case);
    }

    public struct case8 : union, IEquatable<case8>, I<T8>
    {
      public static implicit operator case8(T8 x) => new case8(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case8; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5, Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8) => map8(Case);

      public T8 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T8 Case;
      public case8(T8 x) => Case = x;

      public bool Equals(case8 other) => EqualityComparer<T8>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case8 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T8>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T8>(Case);
    }
  }

  public abstract class Union<TUnion, T1, T2, T3, T4, T5, T6, T7, T8, T9>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4, Case5, Case6, Case7, Case8, Case9 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);
    public static union Of(T5 x) => new case5(x);
    public static union Of(T6 x) => new case6(x);
    public static union Of(T7 x) => new case7(x);
    public static union Of(T8 x) => new case8(x);
    public static union Of(T9 x) => new case9(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }

    public struct case5 : union, IEquatable<case5>, I<T5>
    {
      public static implicit operator case5(T5 x) => new case5(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case5; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map5(Case);

      public T5 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T5 Case;
      public case5(T5 x) => Case = x;

      public bool Equals(case5 other) => EqualityComparer<T5>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case5 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T5>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T5>(Case);
    }

    public struct case6 : union, IEquatable<case6>, I<T6>
    {
      public static implicit operator case6(T6 x) => new case6(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case6; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map6(Case);

      public T6 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T6 Case;
      public case6(T6 x) => Case = x;

      public bool Equals(case6 other) => EqualityComparer<T6>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case6 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T6>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T6>(Case);
    }

    public struct case7 : union, IEquatable<case7>, I<T7>
    {
      public static implicit operator case7(T7 x) => new case7(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case7; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map7(Case);

      public T7 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T7 Case;
      public case7(T7 x) => Case = x;

      public bool Equals(case7 other) => EqualityComparer<T7>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case7 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T7>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T7>(Case);
    }

    public struct case8 : union, IEquatable<case8>, I<T8>
    {
      public static implicit operator case8(T8 x) => new case8(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case8; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map8(Case);

      public T8 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T8 Case;
      public case8(T8 x) => Case = x;

      public bool Equals(case8 other) => EqualityComparer<T8>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case8 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T8>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T8>(Case);
    }

    public struct case9 : union, IEquatable<case9>, I<T9>
    {
      public static implicit operator case9(T9 x) => new case9(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case9; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9) => map9(Case);

      public T9 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T9 Case;
      public case9(T9 x) => Case = x;

      public bool Equals(case9 other) => EqualityComparer<T9>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case9 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T9>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T9>(Case);
    }
  }

  public abstract class Union<TUnion, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
  {
    public enum Tag : byte { Case1, Case2, Case3, Case4, Case5, Case6, Case7, Case8, Case9, Case10 }

    public interface union
    {
      Tag Tag { get; }
      R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10);
    }

    public static union Of(T1 x) => new case1(x);
    public static union Of(T2 x) => new case2(x);
    public static union Of(T3 x) => new case3(x);
    public static union Of(T4 x) => new case4(x);
    public static union Of(T5 x) => new case5(x);
    public static union Of(T6 x) => new case6(x);
    public static union Of(T7 x) => new case7(x);
    public static union Of(T8 x) => new case8(x);
    public static union Of(T9 x) => new case9(x);
    public static union Of(T10 x) => new case10(x);

    public struct case1 : union, IEquatable<case1>, I<T1>
    {
      public static implicit operator case1(T1 x) => new case1(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case1; }

      [MethodImpl((MethodImplOptions)256)]
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map1(Case);

      public T1 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T1 Case;
      public case1(T1 x) => Case = x;

      public bool Equals(case1 other) => EqualityComparer<T1>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case1 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T1>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T1>(Case);
    }

    public struct case2 : union, IEquatable<case2>, I<T2>
    {
      public static implicit operator case2(T2 x) => new case2(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case2; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map2(Case);

      public T2 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T2 Case;
      public case2(T2 x) => Case = x;

      public bool Equals(case2 other) => EqualityComparer<T2>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case2 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T2>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T2>(Case);
    }

    public struct case3 : union, IEquatable<case3>, I<T3>
    {
      public static implicit operator case3(T3 x) => new case3(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case3; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map3(Case);

      public T3 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T3 Case;
      public case3(T3 x) => Case = x;

      public bool Equals(case3 other) => EqualityComparer<T3>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case3 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T3>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T3>(Case);
    }

    public struct case4 : union, IEquatable<case4>, I<T4>
    {
      public static implicit operator case4(T4 x) => new case4(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case4; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map4(Case);

      public T4 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T4 Case;
      public case4(T4 x) => Case = x;

      public bool Equals(case4 other) => EqualityComparer<T4>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case4 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T4>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T4>(Case);
    }

    public struct case5 : union, IEquatable<case5>, I<T5>
    {
      public static implicit operator case5(T5 x) => new case5(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case5; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map5(Case);

      public T5 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T5 Case;
      public case5(T5 x) => Case = x;

      public bool Equals(case5 other) => EqualityComparer<T5>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case5 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T5>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T5>(Case);
    }

    public struct case6 : union, IEquatable<case6>, I<T6>
    {
      public static implicit operator case6(T6 x) => new case6(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case6; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map6(Case);

      public T6 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T6 Case;
      public case6(T6 x) => Case = x;

      public bool Equals(case6 other) => EqualityComparer<T6>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case6 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T6>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T6>(Case);
    }

    public struct case7 : union, IEquatable<case7>, I<T7>
    {
      public static implicit operator case7(T7 x) => new case7(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case7; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map7(Case);

      public T7 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T7 Case;
      public case7(T7 x) => Case = x;

      public bool Equals(case7 other) => EqualityComparer<T7>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case7 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T7>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T7>(Case);
    }

    public struct case8 : union, IEquatable<case8>, I<T8>
    {
      public static implicit operator case8(T8 x) => new case8(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case8; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map8(Case);

      public T8 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T8 Case;
      public case8(T8 x) => Case = x;

      public bool Equals(case8 other) => EqualityComparer<T8>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case8 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T8>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T8>(Case);
    }

    public struct case9 : union, IEquatable<case9>, I<T9>
    {
      public static implicit operator case9(T9 x) => new case9(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case9; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map9(Case);

      public T9 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T9 Case;
      public case9(T9 x) => Case = x;

      public bool Equals(case9 other) => EqualityComparer<T9>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case9 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T9>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T9>(Case);
    }

    public struct case10 : union, IEquatable<case10>, I<T10>
    {
      public static implicit operator case10(T10 x) => new case10(x);

      public Tag Tag { [MethodImpl((MethodImplOptions)256)] get => Tag.Case10; }
      public R Match<R>(Func<T1, R> map1, Func<T2, R> map2, Func<T3, R> map3, Func<T4, R> map4, Func<T5, R> map5,
          Func<T6, R> map6, Func<T7, R> map7, Func<T8, R> map8, Func<T9, R> map9, Func<T10, R> map10) => map10(Case);

      public T10 Value { [MethodImpl((MethodImplOptions)256)] get => Case; }

      public readonly T10 Case;
      public case10(T10 x) => Case = x;

      public bool Equals(case10 other) => EqualityComparer<T10>.Default.Equals(Case, other.Case);
      public override bool Equals(object obj) => obj is case10 x && Equals(x);
      public override int GetHashCode() => EqualityComparer<T10>.Default.GetHashCode(Case);
      public override string ToString() => UnionTools.ToString<TUnion, T10>(Case);
    }
  }

#pragma warning restore 1591

  /// <summary>Methods to work with immutable arrays and some sugar.</summary>
  public static class ArrayTools
  {
    private static class EmptyArray<T>
    {
      public static readonly T[] Value = new T[0];
    }

    /// <summary>Returns singleton empty array of provided type.</summary> 
    /// <typeparam name="T">Array item type.</typeparam> <returns>Empty array.</returns>
    public static T[] Empty<T>() => EmptyArray<T>.Value;

    /// <summary>Wraps item in array.</summary>
    public static T[] One<T>(this T one) => new[] { one };

    /// <summary>Returns true if array is null or have no items.</summary> <typeparam name="T">Type of array item.</typeparam>
    /// <param name="source">Source array to check.</param> <returns>True if null or has no items, false otherwise.</returns>
    public static bool IsNullOrEmpty<T>(this T[] source) => source == null || source.Length == 0;

    /// <summary>Returns empty array instead of null, or source array otherwise.</summary> <typeparam name="T">Type of array item.</typeparam>
    public static T[] EmptyIfNull<T>(this T[] source) => source ?? Empty<T>();

    /// Returns source enumerable if it is array, otherwise converts source to array or an empty array if null.
    public static T[] ToArrayOrSelf<T>(this IEnumerable<T> source) =>
        source == null ? Empty<T>() : (source as T[] ?? source.ToArray());

    /// Returns source enumerable if it is list, otherwise converts source to IList or an empty array if null.
    public static IList<T> ToListOrSelf<T>(this IEnumerable<T> source) =>
        source == null ? Empty<T>() : source as IList<T> ?? source.ToList();

    /// <summary>Array copy</summary>
    public static T[] Copy<T>(this T[] items)
    {
      if (items == null)
        return null;
      var copy = new T[items.Length];
      Array.Copy(items, 0, copy, 0, copy.Length);
      return copy;
    }

    /// <summary>Returns new array consisting from all items from source array then all items from added array.
    /// If source is null or empty, then added array will be returned.
    /// If added is null or empty, then source will be returned.</summary>
    /// <typeparam name="T">Array item type.</typeparam>
    /// <param name="source">Array with leading items.</param>
    /// <param name="added">Array with following items.</param>
    /// <returns>New array with items of source and added arrays.</returns>
    public static T[] Append<T>(this T[] source, params T[] added)
    {
      if (added == null || added.Length == 0)
        return source;
      if (source == null || source.Length == 0)
        return added;

      var result = new T[source.Length + added.Length];
      Array.Copy(source, 0, result, 0, source.Length);
      if (added.Length == 1)
        result[source.Length] = added[0];
      else
        Array.Copy(added, 0, result, source.Length, added.Length);
      return result;
    }

    /// <summary>Append a single item value at the end of source array and returns its copy</summary>
    public static T[] Append<T>(this T[] source, T value)
    {
      if (source == null || source.Length == 0)
        return new[] { value };
      var count = source.Length;
      var result = new T[count + 1];
      Array.Copy(source, 0, result, 0, count);
      result[count] = value;
      return result;
    }

    /// <summary>Performant concat of enumerables in case of arrays.
    /// But performance will degrade if you use Concat().Where().</summary>
    /// <typeparam name="T">Type of item.</typeparam>
    /// <param name="source">goes first.</param>
    /// <param name="other">appended to source.</param>
    /// <returns>empty array or concat of source and other.</returns>
    public static T[] Append<T>(this IEnumerable<T> source, IEnumerable<T> other) =>
        source.ToArrayOrSelf().Append(other.ToArrayOrSelf());

    /// <summary>Returns new array with <paramref name="value"/> appended, 
    /// or <paramref name="value"/> at <paramref name="index"/>, if specified.
    /// If source array could be null or empty, then single value item array will be created despite any index.</summary>
    /// <typeparam name="T">Array item type.</typeparam>
    /// <param name="source">Array to append value to.</param>
    /// <param name="value">Value to append.</param>
    /// <param name="index">(optional) Index of value to update.</param>
    /// <returns>New array with appended or updated value.</returns>
    public static T[] AppendOrUpdate<T>(this T[] source, T value, int index = -1)
    {
      if (source == null || source.Length == 0)
        return new[] { value };
      var sourceLength = source.Length;
      index = index < 0 ? sourceLength : index;
      var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
      Array.Copy(source, result, sourceLength);
      result[index] = value;
      return result;
    }

    /// <summary>Calls predicate on each item in <paramref name="source"/> array until predicate returns true,
    /// then method will return this item index, or if predicate returns false for each item, method will return -1.</summary>
    /// <typeparam name="T">Type of array items.</typeparam>
    /// <param name="source">Source array: if null or empty, then method will return -1.</param>
    /// <param name="predicate">Delegate to evaluate on each array item until delegate returns true.</param>
    /// <returns>Index of item for which predicate returns true, or -1 otherwise.</returns>
    public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
    {
      if (source != null && source.Length != 0)
        for (var i = 0; i < source.Length; ++i)
          if (predicate(source[i]))
            return i;
      return -1;
    }

    /// Minimizes the allocations for closure in predicate lambda with the provided <paramref name="state"/>
    public static int IndexOf<T, S>(this T[] source, S state, Func<S, T, bool> predicate)
    {
      if (source != null && source.Length != 0)
        for (var i = 0; i < source.Length; ++i)
          if (predicate(state, source[i]))
            return i;
      return -1;
    }

    /// <summary>Looks up for item in source array equal to provided value, and returns its index, or -1 if not found.</summary>
    /// <typeparam name="T">Type of array items.</typeparam>
    /// <param name="source">Source array: if null or empty, then method will return -1.</param>
    /// <param name="value">Value to look up.</param>
    /// <returns>Index of item equal to value, or -1 item is not found.</returns>
    public static int IndexOf<T>(this T[] source, T value)
    {
      if (source != null && source.Length != 0)
        for (var i = 0; i < source.Length; ++i)
          if (Equals(source[i], value))
            return i;

      return -1;
    }

    /// <summary>The same as `IndexOf` but searching the item by reference</summary>
    public static int IndexOfReference<T>(this T[] source, T reference) where T : class
    {
      if (source != null && source.Length != 0)
        for (var i = 0; i < source.Length; ++i)
          if (ReferenceEquals(source[i], reference))
            return i;

      return -1;
    }

    /// <summary>Produces new array without item at specified <paramref name="index"/>. 
    /// Will return <paramref name="source"/> array if index is out of bounds, or source is null/empty.</summary>
    /// <typeparam name="T">Type of array item.</typeparam>
    /// <param name="source">Input array.</param> <param name="index">Index if item to remove.</param>
    /// <returns>New array with removed item at index, or input source array if index is not in array.</returns>
    public static T[] RemoveAt<T>(this T[] source, int index)
    {
      if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
        return source;
      if (index == 0 && source.Length == 1)
        return new T[0];
      var result = new T[source.Length - 1];
      if (index != 0)
        Array.Copy(source, 0, result, 0, index);
      if (index != result.Length)
        Array.Copy(source, index + 1, result, index, result.Length - index);
      return result;
    }

    /// <summary>Looks for item in array using equality comparison, and returns new array with found item remove, or original array if not item found.</summary>
    /// <typeparam name="T">Type of array item.</typeparam>
    /// <param name="source">Input array.</param> <param name="value">Value to find and remove.</param>
    /// <returns>New array with value removed or original array if value is not found.</returns>
    public static T[] Remove<T>(this T[] source, T value) =>
        source.RemoveAt(source.IndexOf(value));

    /// <summary>Returns first item matching the <paramref name="predicate"/>, or default item value.</summary>
    /// <typeparam name="T">item type</typeparam>
    /// <param name="source">items collection to search</param>
    /// <param name="predicate">condition to evaluate for each item.</param>
    /// <returns>First item matching condition or default value.</returns>
    public static T FindFirst<T>(this T[] source, Func<T, bool> predicate)
    {
      if (source != null && source.Length != 0)
        for (var i = 0; i < source.Length; ++i)
        {
          var item = source[i];
          if (predicate(item))
            return item;
        }

      return default(T);
    }

    /// Version of FindFirst with the fixed state used by predicate to prevent allocations by predicate lambda closure
    public static T FindFirst<T, S>(this T[] source, S state, Func<S, T, bool> predicate)
    {
      if (source != null && source.Length != 0)
        for (var i = 0; i < source.Length; ++i)
        {
          var item = source[i];
          if (predicate(state, item))
            return item;
        }

      return default(T);
    }

    /// <summary>Returns first item matching the <paramref name="predicate"/>, or default item value.</summary>
    /// <typeparam name="T">item type</typeparam>
    /// <param name="source">items collection to search</param>
    /// <param name="predicate">condition to evaluate for each item.</param>
    /// <returns>First item matching condition or default value.</returns>
    public static T FindFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source is T[] sourceArr ? sourceArr.FindFirst(predicate) : source.FirstOrDefault(predicate);

    /// <summary>Returns element if collection consist on single element, otherwise returns default value.
    /// It does not throw for collection with many elements</summary>
    public static T SingleOrDefaultIfMany<T>(this IEnumerable<T> source)
    {
      if (source is IList<T> list)
        return list.Count == 1 ? list[0] : default(T);

      if (source == null)
        return default(T);

      using (var e = source.GetEnumerator())
      {
        if (!e.MoveNext())
          return default(T);
        var it = e.Current;
        return !e.MoveNext() ? it : default(T);
      }
    }

    /// <summary>Does <paramref name="action"/> for each item</summary>
    public static void ForEach<T>(this T[] source, Action<T> action)
    {
      if (!source.IsNullOrEmpty())
        for (var i = 0; i < source.Length; i++)
          action(source[i]);
    }

    /// Appends source to results
    public static T[] AppendTo<T>(T[] source, int sourcePos, int count, T[] results = null)
    {
      if (results == null)
      {
        var newResults = new T[count];
        if (count == 1)
          newResults[0] = source[sourcePos];
        else
          for (int i = 0, j = sourcePos; i < count; ++i, ++j)
            newResults[i] = source[j];
        return newResults;
      }

      var matchCount = results.Length;
      var appendedResults = new T[matchCount + count];
      if (matchCount == 1)
        appendedResults[0] = results[0];
      else
        Array.Copy(results, 0, appendedResults, 0, matchCount);

      if (count == 1)
        appendedResults[matchCount] = source[sourcePos];
      else
        Array.Copy(source, sourcePos, appendedResults, matchCount, count);

      return appendedResults;
    }

    private static R[] AppendTo<T, R>(T[] source, int sourcePos, int count, Func<T, R> map, R[] results = null)
    {
      if (results == null || results.Length == 0)
      {
        var newResults = new R[count];
        if (count == 1)
          newResults[0] = map(source[sourcePos]);
        else
          for (int i = 0, j = sourcePos; i < count; ++i, ++j)
            newResults[i] = map(source[j]);
        return newResults;
      }

      var oldResultsCount = results.Length;
      var appendedResults = new R[oldResultsCount + count];
      if (oldResultsCount == 1)
        appendedResults[0] = results[0];
      else
        Array.Copy(results, 0, appendedResults, 0, oldResultsCount);

      if (count == 1)
        appendedResults[oldResultsCount] = map(source[sourcePos]);
      else
      {
        for (int i = oldResultsCount, j = sourcePos; i < appendedResults.Length; ++i, ++j)
          appendedResults[i] = map(source[j]);
      }

      return appendedResults;
    }

    private static R[] AppendTo<T, S, R>(T[] source, S state, int sourcePos, int count, Func<S, T, R> map, R[] results = null)
    {
      if (results == null || results.Length == 0)
      {
        var newResults = new R[count];
        if (count == 1)
          newResults[0] = map(state, source[sourcePos]);
        else
          for (int i = 0, j = sourcePos; i < count; ++i, ++j)
            newResults[i] = map(state, source[j]);
        return newResults;
      }

      var oldResultsCount = results.Length;
      var appendedResults = new R[oldResultsCount + count];
      if (oldResultsCount == 1)
        appendedResults[0] = results[0];
      else
        Array.Copy(results, 0, appendedResults, 0, oldResultsCount);

      if (count == 1)
        appendedResults[oldResultsCount] = map(state, source[sourcePos]);
      else
      {
        for (int i = oldResultsCount, j = sourcePos; i < appendedResults.Length; ++i, ++j)
          appendedResults[i] = map(state, source[j]);
      }

      return appendedResults;
    }

    /// <summary>MUTATES the source by updating its item or creates another array with the copies,
    /// the source then maybe a partially updated</summary>
    public static T[] UpdateItemOrShrinkUnsafe<T, S>(this T[] source, S state, Func<S, T, T> tryMap) where T : class
    {
      if (source.Length == 1)
      {
        var result0 = tryMap(state, source[0]);
        if (result0 == null)
          return Empty<T>();
        source[0] = result0;
        return source;
      }

      if (source.Length == 2)
      {
        var result0 = tryMap(state, source[0]);
        var result1 = tryMap(state, source[1]);
        if (result0 == null && result1 == null)
          return Empty<T>();
        if (result0 == null)
          return new[] { result1 };
        if (result1 == null)
          return new[] { result0 };
        source[0] = result0;
        source[1] = result1;
        return source;
      }

      var matchStart = 0;
      T[] matches = null;
      T result = null;
      var i = 0;
      for (; i < source.Length; ++i)
      {
        result = tryMap(state, source[i]);
        if (result != null)
          source[i] = result;
        else
        {
          // for accumulated matched items
          if (i != 0 && i > matchStart)
            matches = AppendTo(source, matchStart, i - matchStart, matches);
          matchStart = i + 1; // guess the next match start will be after the non-matched item
        }
      }

      // when last match was found but not all items are matched (hence matchStart != 0)
      if (result != null && matchStart != 0)
        return AppendTo(source, matchStart, i - matchStart, matches);

      return matches ?? (matchStart != 0 ? Empty<T>() : source);
    }

    /// <summary>Where method similar to Enumerable.Where but more performant and non necessary allocating.
    /// It returns source array and does Not create new one if all items match the condition.</summary>
    /// <typeparam name="T">Type of source items.</typeparam>
    /// <param name="source">If null, the null will be returned.</param>
    /// <param name="condition">Condition to keep items.</param>
    /// <returns>New array if some items are filter out. Empty array if all items are filtered out. Original array otherwise.</returns>
    public static T[] Match<T>(this T[] source, Func<T, bool> condition)
    {
      if (source == null || source.Length == 0)
        return source;

      if (source.Length == 1)
        return condition(source[0]) ? source : Empty<T>();

      if (source.Length == 2)
      {
        var condition0 = condition(source[0]);
        var condition1 = condition(source[1]);
        return condition0 && condition1 ? source
            : condition0 ? new[] { source[0] }
            : condition1 ? new[] { source[1] }
            : Empty<T>();
      }

      if (source.Length == 3)
      {
        var condition0 = condition(source[0]);
        var condition1 = condition(source[1]);
        var condition2 = condition(source[2]);
        return condition0 && condition1 && condition2 ? source
            : condition0 ? (condition1 ? new[] { source[0], source[1] } : condition2 ? new[] { source[0], source[2] } : new[] { source[0] })
            : condition1 ? (condition2 ? new[] { source[1], source[2] } : new[] { source[1] })
            : condition2 ? new[] { source[2] }
            : Empty<T>();
      }

      var matchStart = 0;
      T[] matches = null;
      var matchFound = false;
      var i = 0;
      for (; i < source.Length; ++i)
        if (!(matchFound = condition(source[i])))
        {
          // for accumulated matched items
          if (i != 0 && i > matchStart)
            matches = AppendTo(source, matchStart, i - matchStart, matches);
          matchStart = i + 1; // guess the next match start will be after the non-matched item
        }

      // when last match was found but not all items are matched (hence matchStart != 0)
      if (matchFound && matchStart != 0)
        return AppendTo(source, matchStart, i - matchStart, matches);

      return matches ?? (matchStart != 0 ? Empty<T>() : source);
    }

    /// Match with the additional state to use in <paramref name="condition"/> to minimize the allocations in <paramref name="condition"/> lambda closure 
    public static T[] Match<T, S>(this T[] source, S state, Func<S, T, bool> condition)
    {
      if (source == null || source.Length == 0)
        return source;

      if (source.Length == 1)
        return condition(state, source[0]) ? source : Empty<T>();

      if (source.Length == 2)
      {
        var condition0 = condition(state, source[0]);
        var condition1 = condition(state, source[1]);
        return condition0 && condition1 ? source
            : condition0 ? new[] { source[0] }
            : condition1 ? new[] { source[1] }
            : Empty<T>();
      }

      if (source.Length == 3)
      {
        var condition0 = condition(state, source[0]);
        var condition1 = condition(state, source[1]);
        var condition2 = condition(state, source[2]);
        return condition0 && condition1 && condition2 ? source
            : condition0 ? (condition1 ? new[] { source[0], source[1] } : condition2 ? new[] { source[0], source[2] } : new[] { source[0] })
            : condition1 ? (condition2 ? new[] { source[1], source[2] } : new[] { source[1] })
            : condition2 ? new[] { source[2] }
            : Empty<T>();
      }

      var matchStart = 0;
      T[] matches = null;
      var matchFound = false;
      var i = 0;
      for (; i < source.Length; ++i)
        if (!(matchFound = condition(state, source[i])))
        {
          // for accumulated matched items
          if (i != 0 && i > matchStart)
            matches = AppendTo(source, matchStart, i - matchStart, matches);
          matchStart = i + 1; // guess the next match start will be after the non-matched item
        }

      // when last match was found but not all items are matched (hence matchStart != 0)
      if (matchFound && matchStart != 0)
        return AppendTo(source, matchStart, i - matchStart, matches);

      return matches ?? (matchStart != 0 ? Empty<T>() : source);
    }

    /// <summary>Where method similar to Enumerable.Where but more performant and non necessary allocating.
    /// It returns source array and does Not create new one if all items match the condition.</summary>
    /// <typeparam name="T">Type of source items.</typeparam> <typeparam name="R">Type of result items.</typeparam>
    /// <param name="source">If null, the null will be returned.</param>
    /// <param name="condition">Condition to keep items.</param> <param name="map">Converter from source to result item.</param>
    /// <returns>New array of result items.</returns>
    public static R[] Match<T, R>(this T[] source, Func<T, bool> condition, Func<T, R> map)
    {
      if (source == null)
        return null;

      if (source.Length == 0)
        return Empty<R>();

      if (source.Length == 1)
      {
        var item = source[0];
        return condition(item) ? new[] { map(item) } : Empty<R>();
      }

      if (source.Length == 2)
      {
        var condition0 = condition(source[0]);
        var condition1 = condition(source[1]);
        return condition0 && condition1 ? new[] { map(source[0]), map(source[1]) }
            : condition0 ? new[] { map(source[0]) }
            : condition1 ? new[] { map(source[1]) }
            : Empty<R>();
      }

      if (source.Length == 3)
      {
        var condition0 = condition(source[0]);
        var condition1 = condition(source[1]);
        var condition2 = condition(source[2]);
        return condition0 && condition1 && condition2 ? new[] { map(source[0]), map(source[1]), map(source[2]) }
            : condition0 ? (condition1 ? new[] { map(source[0]), map(source[1]) } : condition2 ? new[] { map(source[0]), map(source[2]) } : new[] { map(source[0]) })
            : condition1 ? (condition2 ? new[] { map(source[1]), map(source[2]) } : new[] { map(source[1]) })
            : condition2 ? new[] { map(source[2]) }
            : Empty<R>();
      }

      var matchStart = 0;
      R[] matches = null;
      var matchFound = false;

      var i = 0;
      for (; i < source.Length; ++i)
        if (!(matchFound = condition(source[i])))
        {
          // for accumulated matched items
          if (i != 0 && i > matchStart)
            matches = AppendTo(source, matchStart, i - matchStart, map, matches);
          matchStart = i + 1; // guess the next match start will be after the non-matched item
        }

      // when last match was found but not all items are matched (hence matchStart != 0)
      if (matchFound && matchStart != 0)
        return AppendTo(source, matchStart, i - matchStart, map, matches);

      return matches ?? (matchStart == 0 ? AppendTo(source, 0, source.Length, map) : Empty<R>());
    }

    /// Match with the additional state to use in <paramref name="condition"/> and <paramref name="map"/> to minimize the allocations in <paramref name="condition"/> lambda closure 
    public static R[] Match<T, S, R>(this T[] source, S state, Func<S, T, bool> condition, Func<S, T, R> map)
    {
      if (source == null)
        return null;

      if (source.Length == 0)
        return Empty<R>();

      if (source.Length == 1)
      {
        var item = source[0];
        return condition(state, item) ? new[] { map(state, item) } : Empty<R>();
      }

      if (source.Length == 2)
      {
        var condition0 = condition(state, source[0]);
        var condition1 = condition(state, source[1]);
        return condition0 && condition1 ? new[] { map(state, source[0]), map(state, source[1]) }
            : condition0 ? new[] { map(state, source[0]) }
            : condition1 ? new[] { map(state, source[1]) }
            : Empty<R>();
      }

      if (source.Length == 3)
      {
        var condition0 = condition(state, source[0]);
        var condition1 = condition(state, source[1]);
        var condition2 = condition(state, source[2]);
        return condition0 && condition1 && condition2 ? new[] { map(state, source[0]), map(state, source[1]), map(state, source[2]) }
            : condition0 ? (condition1 ? new[] { map(state, source[0]), map(state, source[1]) } : condition2 ? new[] { map(state, source[0]), map(state, source[2]) } : new[] { map(state, source[0]) })
            : condition1 ? (condition2 ? new[] { map(state, source[1]), map(state, source[2]) } : new[] { map(state, source[1]) })
            : condition2 ? new[] { map(state, source[2]) }
            : Empty<R>();
      }

      var matchStart = 0;
      R[] matches = null;
      var matchFound = false;

      var i = 0;
      for (; i < source.Length; ++i)
        if (!(matchFound = condition(state, source[i])))
        {
          // for accumulated matched items
          if (i != 0 && i > matchStart)
            matches = AppendTo(source, state, matchStart, i - matchStart, map, matches);
          matchStart = i + 1; // guess the next match start will be after the non-matched item
        }

      // when last match was found but not all items are matched (hence matchStart != 0)
      if (matchFound && matchStart != 0)
        return AppendTo(source, state, matchStart, i - matchStart, map, matches);

      return matches ?? (matchStart == 0 ? AppendTo(source, state, 0, source.Length, map) : Empty<R>());
    }

    /// <summary>Maps all items from source to result array.</summary>
    /// <typeparam name="T">Source item type</typeparam> <typeparam name="R">Result item type</typeparam>
    /// <param name="source">Source items</param> <param name="map">Function to convert item from source to result.</param>
    /// <returns>Converted items</returns>
    public static R[] Map<T, R>(this T[] source, Func<T, R> map)
    {
      if (source == null)
        return null;

      var sourceCount = source.Length;
      if (sourceCount == 0)
        return Empty<R>();

      if (sourceCount == 1)
        return new[] { map(source[0]) };

      if (sourceCount == 2)
        return new[] { map(source[0]), map(source[1]) };

      var results = new R[sourceCount];
      for (var i = 0; i < source.Length; i++)
        results[i] = map(source[i]);
      return results;
    }

    /// Map with additional state to use in <paramref name="map"/> to minimize allocations in <paramref name="map"/> lambda closure 
    public static R[] Map<T, S, R>(this T[] source, S state, Func<S, T, R> map)
    {
      if (source == null)
        return null;

      var sourceCount = source.Length;
      if (sourceCount == 0)
        return Empty<R>();

      if (sourceCount == 1)
        return new[] { map(state, source[0]) };

      if (sourceCount == 2)
        return new[] { map(state, source[0]), map(state, source[1]) };

      var results = new R[sourceCount];
      for (var i = 0; i < source.Length; i++)
        results[i] = map(state, source[i]);
      return results;
    }

    /// Map with additional two states to use in <paramref name="map"/> to minimize allocations in <paramref name="map"/> lambda closure 
    public static R[] Map<T, A, B, R>(this T[] source, A a, B b, Func<A, B, T, R> map)
    {
      if (source == null)
        return null;

      var sourceCount = source.Length;
      if (sourceCount == 0)
        return Empty<R>();

      if (sourceCount == 1)
        return new[] { map(a, b, source[0]) };

      if (sourceCount == 2)
        return new[] { map(a, b, source[0]), map(a, b, source[1]) };

      var results = new R[sourceCount];
      for (var i = 0; i < source.Length; i++)
        results[i] = map(a, b, source[i]);
      return results;
    }

    /// <summary>Maps all items from source to result collection. 
    /// If possible uses fast array Map otherwise Enumerable.Select.</summary>
    /// <typeparam name="T">Source item type</typeparam> <typeparam name="R">Result item type</typeparam>
    /// <param name="source">Source items</param> <param name="map">Function to convert item from source to result.</param>
    /// <returns>Converted items</returns>
    public static IEnumerable<R> Map<T, R>(this IEnumerable<T> source, Func<T, R> map) =>
        source is T[] arr ? arr.Map(map) : source?.Select(map);

    /// <summary>If <paramref name="source"/> is array uses more effective Match for array, otherwise just calls Where</summary>
    public static IEnumerable<T> Match<T>(this IEnumerable<T> source, Func<T, bool> condition) =>
        source is T[] arr ? arr.Match(condition) : source?.Where(condition);

    /// <summary>If <paramref name="source"/> is array uses more effective Match for array,otherwise just calls Where, Select</summary>
    public static IEnumerable<R> Match<T, R>(this IEnumerable<T> source, Func<T, bool> condition, Func<T, R> map) =>
        source is T[] arr ? arr.Match(condition, map) : source?.Where(condition).Select(map);
  }

  /// <summary>Wrapper that provides optimistic-concurrency Swap operation implemented using <see cref="Ref.Swap{T}"/>.</summary>
  /// <typeparam name="T">Type of object to wrap.</typeparam>
  public sealed class Ref<T> where T : class
  {
    /// <summary>Gets the wrapped value.</summary>
    public T Value => _value;
    private T _value;

    /// <summary>Creates ref to object, optionally with initial value provided.</summary>
    /// <param name="initialValue">(optional) Initial value.</param>
    public Ref(T initialValue = default) => _value = initialValue;

    /// <summary>Exchanges currently hold object with <paramref name="getNewValue"/> - see <see cref="Ref.Swap{T}"/> for details.</summary>
    /// <param name="getNewValue">Delegate to produce new object value from current one passed as parameter.</param>
    /// <returns>Returns old object value the same way as <see cref="Interlocked.Exchange(ref int,int)"/></returns>
    /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
    public T Swap(Func<T, T> getNewValue) =>
        Ref.Swap(ref _value, getNewValue);

    // todo: @feature A good candidate to implement
    // <summary>The same as `Swap` but instead of the old known value it returns the new one</summary>
    //public T SwapAndGetNewValue(Func<T, T> getNewValue) =>
    //    Ref.Swap(ref _value, getNewValue);

    /// Option without allocation for capturing `a` in closure of `getNewValue`
    public T Swap<A>(A a, Func<T, A, T> getNewValue) => Ref.Swap(ref _value, a, getNewValue);

    // todo: @api @perf the return value of Swap is not used. On the other hand there are case when we need to assign the closed variable inside the `getNewValue` (`getValue`) delegate - so we may consider the variant of returning the state A instead to assign it to prevent the closure.
    /// Option without allocation for capturing `a` and `b` in closure of `getNewValue`
    public T Swap<A, B>(A a, B b, Func<T, A, B, T> getNewValue) => Ref.Swap(ref _value, a, b, getNewValue);

    /// <summary>Just sets new value ignoring any intermingled changes and returns the original value</summary>
    /// <param name="newValue"></param> <returns>old value</returns>
    public T Swap(T newValue) => Interlocked.Exchange(ref _value, newValue);

    /// <summary>Directly sets the value and returns the new value</summary>
    public T SetNonAtomic(T newValue) => _value = newValue;

    /// <summary>Compares current Referred value with <paramref name="currentValue"/> and if equal replaces current with <paramref name="newValue"/></summary>
    /// <param name="currentValue"></param> <param name="newValue"></param>
    /// <returns>True if current value was replaced with new value, and false if current value is outdated (already changed by other party).</returns>
    /// <example><c>[!CDATA[
    /// var value = SomeRef.Value;
    /// if (!SomeRef.TrySwapIfStillCurrent(value, Update(value))
    ///     SomeRef.Swap(v => Update(v)); // fallback to normal Swap with delegate allocation
    /// ]]</c></example>
    public bool TrySwapIfStillCurrent(T currentValue, T newValue) =>
        Interlocked.CompareExchange(ref _value, newValue, currentValue) == currentValue;

    /// <summary>Just sets </summary>
    public void UnsafeSet(T newValue) => _value = newValue;
  }

  /// <summary>Provides optimistic-concurrency consistent <see cref="Swap{T}"/> operation.</summary>
  public static class Ref
  {
    /// The default max retry count - can be overridden by `Swap` optional parameter 
    public const int RETRY_COUNT_UNTIL_THROW = 50;

    /// <summary>Factory for <see cref="Ref{T}"/> with type of value inference.</summary>
    /// <typeparam name="T">Type of value to wrap.</typeparam>
    /// <param name="value">Initial value to wrap.</param>
    /// <returns>New ref.</returns>
    public static Ref<T> Of<T>(T value) where T : class => new Ref<T>(value);

    /// <summary>Creates new ref to the value of original ref.</summary> <typeparam name="T">Ref value type.</typeparam>
    /// <param name="original">Original ref.</param> <returns>New ref to original value.</returns>
    public static Ref<T> NewRef<T>(this Ref<T> original) where T : class => Of(original.Value);

    /// <summary>First, it evaluates new value using <paramref name="getNewValue"/> function. 
    /// Second, it checks that original value is not changed. 
    /// If it is changed it will retry first step, otherwise it assigns new value and returns original (the one used for <paramref name="getNewValue"/>).</summary>
    /// <typeparam name="T">Type of value to swap.</typeparam>
    /// <param name="value">Reference to change to new value</param>
    /// <param name="getNewValue">Delegate to get value from old one.</param>
    /// <param name="retryCountUntilThrow">(optional)</param>
    /// <returns>Old/original value. By analogy with <see cref="Interlocked.Exchange(ref int,int)"/>.</returns>
    /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
    [MethodImpl((MethodImplOptions)256)]
    public static T Swap<T>(ref T value, Func<T, T> getNewValue,
        int retryCountUntilThrow = RETRY_COUNT_UNTIL_THROW)
        where T : class
    {
#if SUPPORTS_SPIN_WAIT
            var spinWait = new SpinWait();
#endif
      var retryCount = 0;
      while (true)
      {
        var oldValue = value;
        var newValue = getNewValue(oldValue);
        if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
          return oldValue;

        if (++retryCount > retryCountUntilThrow)
          ThrowRetryCountExceeded(retryCountUntilThrow);
#if SUPPORTS_SPIN_WAIT
                spinWait.SpinOnce();
#endif
      }
    }

    private static void ThrowRetryCountExceeded(int retryCountExceeded) =>
        throw new InvalidOperationException(
            $"Ref retried to Update for {retryCountExceeded} times But there is always someone else intervened.");

    /// <summary>Swap with the additional state <paramref name="a"/> required for the delegate <paramref name="getNewValue"/>.
    /// May prevent closure creation for the delegate</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static T Swap<T, A>(ref T value, A a, Func<T, A, T> getNewValue,
        int retryCountUntilThrow = RETRY_COUNT_UNTIL_THROW)
        where T : class
    {
#if SUPPORTS_SPIN_WAIT
            var spinWait = new SpinWait();
#endif
      var retryCount = 0;
      while (true)
      {
        var oldValue = value;
        if (Interlocked.CompareExchange(ref value, getNewValue(oldValue, a), oldValue) == oldValue)
          return oldValue;
        if (++retryCount > retryCountUntilThrow)
          ThrowRetryCountExceeded(retryCountUntilThrow);
#if SUPPORTS_SPIN_WAIT
                spinWait.SpinOnce();
#endif
      }
    }

    /// <summary>Swap with the additional state <paramref name="a"/> required for the delegate <paramref name="getNewValue"/>.
    /// May prevent closure creation for the delegate</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static T SwapAndGetNewValue<T, A>(ref T value, A a, Func<T, A, T> getNewValue,
        int retryCountUntilThrow = RETRY_COUNT_UNTIL_THROW)
        where T : class
    {
#if SUPPORTS_SPIN_WAIT
            var spinWait = new SpinWait();
#endif
      var retryCount = 0;
      while (true)
      {
        var oldValue = value;
        var newValue = getNewValue(oldValue, a);
        if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
          return newValue;
        if (++retryCount > retryCountUntilThrow)
          ThrowRetryCountExceeded(retryCountUntilThrow);
#if SUPPORTS_SPIN_WAIT
                spinWait.SpinOnce();
#endif
      }
    }

    /// <summary>Swap with the additional state <paramref name="a"/>, <paramref name="b"/> required for the delegate <paramref name="getNewValue"/>.
    /// May prevent closure creation for the delegate</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static T Swap<T, A, B>(ref T value, A a, B b, Func<T, A, B, T> getNewValue,
        int retryCountUntilThrow = RETRY_COUNT_UNTIL_THROW)
        where T : class
    {
#if SUPPORTS_SPIN_WAIT
            var spinWait = new SpinWait();
#endif
      var retryCount = 0;
      while (true)
      {
        var oldValue = value;
        var newValue = getNewValue(oldValue, a, b);
        if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
          return oldValue;

        if (++retryCount > retryCountUntilThrow)
          ThrowRetryCountExceeded(retryCountUntilThrow);

#if SUPPORTS_SPIN_WAIT
                spinWait.SpinOnce();
#endif
      }
    }

    /// <summary>Swap with the additional state <paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/> required for the delegate <paramref name="getNewValue"/>.
    /// May prevent closure creation for the delegate</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static T Swap<T, A, B, C>(ref T value, A a, B b, C c, Func<T, A, B, C, T> getNewValue,
        int retryCountUntilThrow = RETRY_COUNT_UNTIL_THROW)
        where T : class
    {
#if SUPPORTS_SPIN_WAIT
            var spinWait = new SpinWait();
#endif
      var retryCount = 0;
      while (true)
      {
        var oldValue = value;
        var newValue = getNewValue(oldValue, a, b, c);
        if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
          return oldValue;

        if (++retryCount > retryCountUntilThrow)
          ThrowRetryCountExceeded(retryCountUntilThrow);

#if SUPPORTS_SPIN_WAIT
                spinWait.SpinOnce();
#endif
      }
    }

    // todo: Func of 5 args is not available on all plats
    //        /// Option without allocation for capturing `a`, `b`, `c`, `d` in closure of `getNewValue`
    //        [MethodImpl((MethodImplOptions)256)]
    //        public static T Swap<T, A, B, C, D>(ref T value, A a, B b, C c, D d, Func<T, A, B, C, D, T> getNewValue,
    //            int retryCountUntilThrow = RETRY_COUNT_UNTIL_THROW)
    //            where T : class
    //        {
    //#if SUPPORTS_SPIN_WAIT
    //            var spinWait = new SpinWait();
    //#endif
    //            var retryCount = 0;
    //            while (true)
    //            {
    //                var oldValue = value;
    //                var newValue = getNewValue(oldValue, a, b, c, d);
    //                if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
    //                    return oldValue;

    //                if (++retryCount > retryCountUntilThrow)
    //                    ThrowRetryCountExceeded(retryCountUntilThrow);

    //#if SUPPORTS_SPIN_WAIT
    //                spinWait.SpinOnce();
    //#endif
    //            }
    //        }
  }

  /// <summary>Printable thing via provided printer </summary>
  public interface IPrintable
  {
    /// <summary>Print to the provided string builder via the provided printer.</summary>
    StringBuilder Print(StringBuilder s, Func<StringBuilder, object, StringBuilder> printer);
  }

  /// <summary>Produces good enough hash codes for the fields</summary>
  public static class Hasher
  {
    /// <summary>Combines hashes of two fields</summary>
    public static int Combine<T1, T2>(T1 a, T2 b) =>
        Combine(a?.GetHashCode() ?? 0, b?.GetHashCode() ?? 0);

    /// <summary>Inspired by System.Tuple.CombineHashCodes</summary>
    public static int Combine(int h1, int h2)
    {
      if (h1 == 0) return h2;
      unchecked
      {
        return (h1 << 5) + h1 ^ h2;
      }
    }
  }

  /// Simple unbounded object pool
  public sealed class StackPool<T> where T : class
  {
    /// <summary>Give me an object</summary>
    [MethodImpl((MethodImplOptions)256)]
    public T RentOrDefault() =>
        Interlocked.Exchange(ref _s, _s?.Tail)?.Head;

    /// <summary>Give it back</summary>
    [MethodImpl((MethodImplOptions)256)]
    public void Return(T x) =>
        Interlocked.Exchange(ref _s, new Stack(x, _s));

    private Stack _s;

    private sealed class Stack
    {
      public readonly T Head;
      public readonly Stack Tail;
      public Stack(T h, Stack t)
      {
        Head = h;
        Tail = t;
      }
    }
  }

  /// <summary>Immutable Key-Value pair. It is reference type (could be check for null), 
  /// which is different from System value type <see cref="KeyValuePair{TKey,TValue}"/>.
  /// In addition provides <see cref="Equals"/> and <see cref="GetHashCode"/> implementations.</summary>
  /// <typeparam name="K">Type of Key.</typeparam><typeparam name="V">Type of Value.</typeparam>
  public class KV<K, V> : IPrintable
  {
    /// <summary>Key.</summary>
    public readonly K Key;

    /// <summary>Value.</summary>
    public readonly V Value;

    /// <summary>Creates Key-Value object by providing key and value. Does Not check either one for null.</summary>
    /// <param name="key">key.</param><param name="value">value.</param>
    public KV(K key, V value)
    {
      Key = key;
      Value = value;
    }

    /// <inheritdoc />
    public StringBuilder Print(StringBuilder s, Func<StringBuilder, object, StringBuilder> printer) =>
        s.Append("(").To(b => Key == null ? b : printer(b, Key))
            .Append(", ").To(b => Value == null ? b : printer(b, Value))
            .Append(')');

    /// <summary>Creates nice string view.</summary><returns>String representation.</returns>
    public override string ToString() =>
        Print(new StringBuilder(), (s, x) => s.Append(x)).ToString();

    /// <summary>Returns true if both key and value are equal to corresponding key-value of other object.</summary>
    public override bool Equals(object obj)
    {
      var other = obj as KV<K, V>;
      return other != null
             && (ReferenceEquals(other.Key, Key) || Equals(other.Key, Key))
             && (ReferenceEquals(other.Value, Value) || Equals(other.Value, Value));
    }

    /// <summary>Combines key and value hash code</summary>
    public override int GetHashCode() => Hasher.Combine(Key, Value);
  }

  /// <summary>Helpers for <see cref="KV{K,V}"/>.</summary>
  public static class KV
  {
    /// <summary>Creates the key value pair.</summary>
    public static KV<K, V> Of<K, V>(K key, V value) => new KV<K, V>(key, value);

    /// <summary>Creates the pair with the new value</summary>
    public static KV<K, V> WithValue<K, V>(this KV<K, V> kv, V value) => new KV<K, V>(kv.Key, value);
  }

  /// Simple helper for creation of the pair of two parts.
  public static class KeyValuePair
  {
    /// Pairs key with value.
    public static KeyValuePair<K, V> Pair<K, V>(this K key, V value) => new KeyValuePair<K, V>(key, value);
  }

  /// <summary>Helper structure which allows to distinguish null value from the default value for optional parameter.</summary>
  public struct Opt<T>
  {
    /// <summary>Allows to transparently convert parameter argument to opt structure.</summary>
    public static implicit operator Opt<T>(T value) => new Opt<T>(value);

    /// <summary>Argument value.</summary>
    public readonly T Value;

    /// <summary>Indicates that value is provided.</summary>
    public readonly bool HasValue;

    /// <summary>Wraps passed value in structure. Sets the flag that value is present.</summary>
    public Opt(T value)
    {
      HasValue = true;
      Value = value;
    }

    /// <summary>Helper to get value or default value if value is not present.</summary>
    public T OrDefault(T defaultValue = default) => HasValue ? Value : defaultValue;
  }

  /// <summary>Ever growing list methods</summary>
  public static class GrowingList
  {
    /// <summary>Default initial capacity </summary>
    public const int DefaultInitialCapacity = 2;

    /// Push the new slot and return the ref to it
    public static ref T PushSlot<T>(ref T[] items, int count)
    {
      if (items == null)
        items = new T[DefaultInitialCapacity];
      else if (count >= items.Length)
        Expand(ref items);
      return ref items[count];
    }

    /// Adds the new item possibly extending the item collection
    public static void Push<T>(ref T[] items, int count, T item)
    {
      if (items == null)
        items = new T[DefaultInitialCapacity];
      else if (count >= items.Length)
        Expand(ref items);
      items[count] = item;
    }

    /// Expands the items starting with 2
    internal static void Expand<T>(ref T[] items)
    {
      var count = items.Length;
      var newItems = new T[count << 1]; // count x 2
      if (count < 6)
        for (var i = 0; i < count; ++i)
          newItems[i] = items[i];
      else
        Array.Copy(items, 0, newItems, 0, count);
    }

    ///<summary>Creates the final array out of the list, so that you cannot use after that!</summary>
    public static T[] ResizeToArray<T>(T[] items, int count)
    {
      if (count < items.Length)
        Array.Resize(ref items, count);
      return items;
    }

    /// <inheritdoc />
    public static string ToString<T>(T[] items, int count) =>
        $"Count {count} of {(count == 0 || items == null || items.Length == 0 ? "empty" : "first (" + items[0] + ") and last (" + items[count - 1] + ")")}";
  }

  /// <summary>Ever growing list</summary>
  public struct GrowingList<T>
  {
    /// <summary>Default initial capacity </summary>
    public const int DefaultInitialCapacity = 2;

    /// The items array
    public T[] Items;

    /// The count
    public int Count;

    /// Constructs the thing 
    public GrowingList(T[] items, int count = 0)
    {
      Items = items;
      Count = count;
    }

    /// Push the new slot and return the ref to it
    public ref T PushSlot()
    {
      if (Items == null)
        Items = new T[DefaultInitialCapacity];
      else if (Count >= Items.Length)
        GrowingList.Expand(ref Items);
      return ref Items[Count++];
    }

    /// Adds the new item possibly extending the item collection
    public void Push(T item)
    {
      if (Items == null)
        Items = new T[DefaultInitialCapacity];
      else if (Count >= Items.Length)
        GrowingList.Expand(ref Items);
      Items[Count++] = item;
    }

    /// Pops the item - just moving the counter back
    public void Pop() => --Count;

    ///<summary>Creates the final array out of the list, so that you cannot use after that!</summary>
    public T[] ResizeToArray()
    {
      var items = Items;
      if (Count < items.Length)
        Array.Resize(ref items, Count);
      return items;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Count {Count} of {(Count == 0 || Items == null || Items.Length == 0 ? "empty" : "first (" + Items[0] + ") and last (" + Items[Count - 1] + ")")}";
  }

  /// <summary>The structure of arrays (SOA) to hold the keys and values of the map.
  /// The arrays may have the bigger capacity than the actual item count, so you need to use `Count` to get the valid number of items.</summary>
  public struct KeysAndValues<K, V>
  {
    /// <summary>The keys</summary>
    public K[] Keys;
    /// <summary>The values</summary>
    public V[] Values;
    /// <summary>The actual item count - the same for keys and values</summary>
    public int Count;
    /// <summary>Constructs the structure</summary>
    public KeysAndValues(K[] keys, V[] values, int count)
    {
      Keys = keys;
      Values = values;
      Count = count;
    }
  }


  /// <summary>Immutable list - simplest linked list with the Head and the Tail.</summary>
  public sealed class ImList<T>
  {
    /// <summary>Empty list to Push to.</summary>
    public static readonly ImList<T> Empty = new ImList<T>();

    /// <summary>True for empty list.</summary>
    public bool IsEmpty => Tail == null;

    /// <summary>First value in a list.</summary>
    public readonly T Head;

    /// <summary>The rest of values or Empty if list has a single value.</summary>
    public readonly ImList<T> Tail;

    /// <summary>Prepends new value and returns new list.</summary>
    public ImList<T> Push(T head) => new ImList<T>(head, this);

    /// <summary>Enumerates the list.</summary>
    public IEnumerable<T> Enumerate()
    {
      if (IsEmpty)
        yield break;
      for (var list = this; !list.IsEmpty; list = list.Tail)
        yield return list.Head;
    }

    /// <summary>String representation for debugging purposes</summary>
    public override string ToString() => IsEmpty
        ? "[]" : Tail.IsEmpty
        ? "[" + Head + "]" : Tail.Tail.IsEmpty
        ? "[" + Head + "," + Tail.Head + "]" : Tail.Tail.Tail.IsEmpty
        ? "[" + Head + "," + Tail.Head + "," + Tail.Tail.Head + "]"
        : "[" + Head + "," + Tail.Head + "," + Tail.Tail.Head + ", ...]";

    private ImList() { }

    private ImList(T head, ImList<T> tail)
    {
      Head = head;
      Tail = tail;
    }
  }

  /// <summary>Extension methods providing basic operations on a list.</summary>
  public static class ImList
  {
    /// Split list into (Head, Tail, IsEmpty) tuple
    public static void Deconstruct<T>(this ImList<T> list, out T head, out ImList<T> tail, out bool isEmpty)
    {
      head = list.Head;
      tail = list.Tail;
      isEmpty = list.IsEmpty;
    }

    /// <summary>
    /// Constructs the reversed list from the parameter array of items
    /// </summary>
    public static ImList<T> List<T>(params T[] items)
    {
      var l = ImList<T>.Empty;
      if (items != null)
        for (var i = items.Length - 1; i >= 0; --i)
          l = l.Push(items[i]);
      return l;
    }

    /// <summary>
    /// Constructs the list as the reversed input list
    /// </summary>
    public static ImList<T> ToImList<T>(this IList<T> source)
    {
      var l = ImList<T>.Empty;
      if (source != null)
        for (var i = source.Count - 1; i >= 0; --i)
          l = l.Push(source[i]);
      return l;
    }

    /// <summary>
    /// Constructs the list as the reversed enumerable
    /// </summary>
    public static ImList<T> ToImList<T>(this IEnumerable<T> source)
    {
      if (source is IList<T> list)
        return list.ToImList();
      var l = ImList<T>.Empty;

      if (source != null)
        foreach (var item in source)
          l = l.Push(item);
      return l.Reverse();
    }

    /// <summary>Constructs list of one element</summary>
    public static ImList<T> List<T>(this T head) => ImList<T>.Empty.Push(head);

    /// <summary>Constructs list from head and tail</summary>
    public static ImList<T> List<T>(this T head, ImList<T> tail) => tail.Push(head);

    /// <summary>Apples some effect action to each element</summary>
    public static void ForEach<T>(this ImList<T> list, Action<T> effect)
    {
      for (; !list.IsEmpty; list = list.Tail)
        effect(list.Head);
    }

    /// <summary>Fold list to a single value. The respective name for it in LINQ is Aggregate</summary>
    public static S Fold<T, S>(this ImList<T> list, S state, Func<T, S, S> reduce)
    {
      if (list.IsEmpty)
        return state;
      var result = state;
      for (; !list.IsEmpty; list = list.Tail)
        result = reduce(list.Head, result);
      return result;
    }

    /// <summary>Fold list to a single value with index of item. The respective name for it in LINQ is Aggregate.</summary>
    public static S Fold<T, S>(this ImList<T> list, S state, Func<T, int, S, S> reduce)
    {
      if (list.IsEmpty)
        return state;
      var result = state;
      for (var i = 0; !list.IsEmpty; list = list.Tail, ++i)
        result = reduce(list.Head, i, result);
      return result;
    }

    /// <summary>Returns new list in reverse order.</summary>
    public static ImList<T> Reverse<T>(this ImList<T> list)
    {
      if (list.IsEmpty || list.Tail.IsEmpty)
        return list;
      var reversed = ImList<T>.Empty;
      for (; !list.IsEmpty; list = list.Tail)
        reversed = reversed.Push(list.Head);
      return reversed;
    }

    /// <summary>Maps the items from the first list to the result list.</summary>
    public static ImList<R> Map<T, R>(this ImList<T> list, Func<T, R> map) =>
        list.Fold(ImList<R>.Empty, (x, r) => List(map(x), r)).Reverse();

    /// <summary>Maps with index</summary>
    public static ImList<R> Map<T, R>(this ImList<T> list, Func<T, int, R> map) =>
        list.Fold(ImList<R>.Empty, (x, i, r) => List(map(x, i), r)).Reverse();

    /// <summary>Copies list to array.</summary>
    public static T[] ToArray<T>(this ImList<T> source) =>
        source.IsEmpty ? ArrayTools.Empty<T>()
        : source.Tail.IsEmpty ? new[] { source.Head } : source.Enumerate().ToArray();
  }

  /// Zipper is an immutable persistent data structure, to represent collection with single focused (selected, active) element.
  /// Consist of REVERSED `Left` immutable list, `Focus` element, and the `Right` immutable list. That's why a Zipper name,
  /// where left and right part are joined / zipped in focus item.
  public sealed class ImZipper<T>
  {
    /// Empty singleton instance to start building your zipper
    public static readonly ImZipper<T> Empty = new ImZipper<T>();

    /// True is zipper does not contain items
    public bool IsEmpty => Count == 0;

    /// Index of Focus item, from `0` to `Count-1`
    public readonly int Index;

    /// Number of items
    public readonly int Count;

    /// Left REVERSED list, so the Head of the list is just prior the Focus item 
    public readonly ImList<T> Left;

    /// Right list, where Head is just after the Focus item
    public readonly ImList<T> Right;

    /// Single focus item
    public readonly T Focus;

    /// <inheritdoc />
    public override string ToString() =>
        IsEmpty ? "[||]" : Count + ":" + Left.Reverse() + "|" + Index + ":" + Focus + "|" + Right;

    /// Sets a new focus and pushes the old focus to the Left list. 
    public ImZipper<T> Append(T focus) => PushLeft(focus);

    /// Sets a new focus and pushes the old focus to the Left list.
    public ImZipper<T> PushLeft(T focus) =>
    IsEmpty ? new ImZipper<T>(ImList<T>.Empty, focus, 0, ImList<T>.Empty, 1)
            : new ImZipper<T>(Left.Push(Focus), focus, Index + 1, Right, Count + 1);

    /// Sets a new focus and pushes the old focus to the right list. 
    public ImZipper<T> Insert(T focus) => PushRight(focus);

    /// Sets a new focus and pushes the old focus to the right list. 
    public ImZipper<T> PushRight(T focus) =>
        IsEmpty ? new ImZipper<T>(ImList<T>.Empty, focus, 0, ImList<T>.Empty, 1)
            : new ImZipper<T>(Left, focus, Index, Right.Push(Focus), Count + 1);

    /// Removes a focus, filling the hole with the item from the left list, or from the right if the left is empty
    public ImZipper<T> PopLeft() =>
        IsEmpty ? this
        : Left.IsEmpty && Right.IsEmpty ? Empty
        : !Left.IsEmpty ? new ImZipper<T>(Left.Tail, Left.Head, Index - 1, Right, Count - 1)
        : new ImZipper<T>(Left, Right.Head, Index, Right.Tail, Count - 1);

    /// Removes a focus, filling the hole with the item from the right list, or from the left if the right is empty
    public ImZipper<T> PopRight() =>
        IsEmpty ? this
        : Left.IsEmpty && Right.IsEmpty ? Empty
        : !Right.IsEmpty ? new ImZipper<T>(Left, Right.Head, Index, Right.Tail, Count - 1)
        : new ImZipper<T>(Left.Tail, Left.Head, Index - 1, Right, Count - 1);

    /// Shifts focus one element to the left (decrementing its Index).
    public ImZipper<T> ShiftLeft() =>
        IsEmpty || Left.IsEmpty ? this
        : new ImZipper<T>(Left.Tail, Left.Head, Index - 1, Right.Push(Focus), Count);

    /// Shifts focus one element to the right (incrementing its Index).
    public ImZipper<T> ShiftRight() =>
        IsEmpty || Right.IsEmpty ? this
        : new ImZipper<T>(Left.Push(Focus), Right.Head, Index + 1, Right.Tail, Count);

    /// Sets a new focus and returns a new zipper with the left and right lists unchanged
    public ImZipper<T> WithFocus(T focus) =>
        IsEmpty ? this : new ImZipper<T>(Left, focus, Index, Right, Count);

    /// Maps over the zipper items producing a new zipper
    public ImZipper<R> Map<R>(Func<T, R> map) =>
        IsEmpty ? ImZipper<R>.Empty
            : new ImZipper<R>(Left.Reverse().Fold(ImList<R>.Empty, (x, r) => r.Push(map(x))),
                map(Focus), Index, Right.Map(map), Count);

    /// Maps over the zipper items with item index, producing a new zipper
    public ImZipper<R> Map<R>(Func<T, int, R> map) =>
        IsEmpty ? ImZipper<R>.Empty
            : new ImZipper<R>(
                Left.Reverse().Fold(ImList<R>.Empty, (x, i, r) => r.Push(map(x, i))),
                map(Focus, Index), Index, Right.Map((x, i) => map(x, Index + 1 + i)), Count);

    private ImZipper() => Index = -1;

    private ImZipper(ImList<T> left, T focus, int index, ImList<T> right, int count)
    {
      Left = left;
      Focus = focus;
      Index = index;
      Right = right;
      Count = count;
    }
  }

  /// Other ImZipper methods
  public static class ImZipper
  {
    /// Appends array items to zipper
    public static ImZipper<T> Zip<T>(params T[] items)
    {
      if (items.IsNullOrEmpty())
        return ImZipper<T>.Empty;
      var z = ImZipper<T>.Empty;
      for (var i = 0; i < items.Length; ++i)
        z = z.PushLeft(items[i]);
      return z;
    }

    /// Converts to array.
    public static T[] ToArray<T>(this ImZipper<T> z)
    {
      if (z.IsEmpty)
        return ArrayTools.Empty<T>();
      var a = new T[z.Count];
      z.Fold(a, (x, i, xs) =>
      {
        xs[i] = x;
        return xs;
      });
      return a;
    }

    /// Shifts focus to a specified index, e.g. a random access
    public static ImZipper<T> ShiftTo<T>(this ImZipper<T> z, int i)
    {
      if (i < 0 || i >= z.Count || i == z.Index)
        return z;
      while (i < z.Index)
        z = z.ShiftLeft();
      while (i > z.Index)
        z = z.ShiftRight();
      return z;
    }

    /// Updates a focus element if it is present, otherwise does nothing.
    /// If the focus item is the equal one, then returns the same zipper back.
    public static ImZipper<T> Update<T>(this ImZipper<T> z, Func<T, T> update)
    {
      if (z.IsEmpty)
        return z;
      var result = update(z.Focus);
      if (ReferenceEquals(z.Focus, result) || result != null && result.Equals(z.Focus))
        return z;
      return z.WithFocus(result);
    }

    /// Update the item at random index, by shifting and updating it
    public static ImZipper<T> UpdateAt<T>(this ImZipper<T> z, int i, Func<T, T> update) =>
        i < 0 || i >= z.Count ? z : z.ShiftTo(i).Update(update);

    /// Update the item at random index, by shifting and updating it
    public static ImZipper<T> RemoveAt<T>(this ImZipper<T> z, int i) =>
        i < 0 || i >= z.Count ? z : z.ShiftTo(i).PopLeft();

    /// Folds zipper to a single value
    public static S Fold<T, S>(this ImZipper<T> z, S state, Func<T, S, S> reduce) =>
        z.IsEmpty ? state :
        z.Right.Fold(reduce(z.Focus, z.Left.Reverse().Fold(state, reduce)), reduce);

    /// Folds zipper to a single value by using an item index
    public static S Fold<T, S>(this ImZipper<T> z, S state, Func<T, int, S, S> reduce)
    {
      if (z.IsEmpty)
        return state;
      var focusIndex = z.Index;
      var reducedLeft = z.Left.Reverse().Fold(state, reduce);
      return z.Right.Fold(reduce(z.Focus, focusIndex, reducedLeft),
          (x, i, r) => reduce(x, focusIndex + i + 1, r));
    }

    /// <summary>Apply some effect action on each element</summary>
    public static void ForEach<T>(this ImZipper<T> z, Action<T> effect)
    {
      if (!z.IsEmpty)
      {
        if (!z.Left.IsEmpty)
          z.Left.Reverse().ForEach(effect);
        effect(z.Focus);
        if (!z.Right.IsEmpty)
          z.Right.ForEach(effect);
      }
    }
  }

  /// Given the old value should and the new value should return result updated value.
  public delegate V Update<V>(V oldValue, V newValue);

  /// Update handler including the key
  public delegate V Update<K, V>(K key, V oldValue, V newValue);

  /// <summary>
  /// Fold reducer. Designed as a alternative to `Func{V, S, S}` but with possibility of inlining on the call side.
  /// Note: To get the advantage of inlining the <see cref="Reduce"/> can the interface should be implemented and passed as a NON-GENERIC STRUCT
  /// </summary>
  public interface IFoldReducer<T, S>
  {
    /// <summary>Reduce method</summary>
    S Reduce(T x, S state);
  }

  /// <summary>
  /// Immutable http://en.wikipedia.org/wiki/AVL_tree with integer keys and <typeparamref name="V"/> values.
  /// </summary>
  public class ImMap<V>
  {
    /// <summary>Empty tree to start with.</summary>
    public static readonly ImMap<V> Empty = new ImMap<V>();

    /// <summary>Returns true if tree is empty.</summary>
    public bool IsEmpty => this == Empty;

    /// Prevents multiple creation of an empty tree
    protected ImMap() { }

    /// <summary>Height of the longest sub-tree/branch - 0 for the empty tree</summary>
    public virtual int Height => 0;

    /// <summary>Prints "empty"</summary>
    public override string ToString() => "empty";
  }

  /// <summary>Wraps the stored data with "fixed" reference semantics - when added to the tree it did not change or reconstructed in memory</summary>
  public sealed class ImMapEntry<V> : ImMap<V>
  {
    /// <inheritdoc />
    public override int Height => 1;

    /// The Key is basically the hash, or the Height for ImMapTree
    public readonly int Key;

    /// The value - may be modified if you need a Ref{V} semantics
    public V Value;

    /// <summary>Constructs the entry with the default value</summary>
    public ImMapEntry(int key) => Key = key;

    /// <summary>Constructs the entry</summary>
    public ImMapEntry(int key, V value)
    {
      Key = key;
      Value = value;
    }

    /// Prints the key value pair
    public override string ToString() => Key + ":" + Value;
  }

  /// <summary>
  /// The two level - two node tree with either left or right
  /// </summary>
  public sealed class ImMapBranch<V> : ImMap<V>
  {
    /// <summary>Always two</summary>
    public override int Height => 2;

    /// Contains the once created data node
    public readonly ImMapEntry<V> Entry;

    /// Right branch or empty.
    public ImMapEntry<V> RightEntry;

    /// Constructor
    public ImMapBranch(ImMapEntry<V> entry, ImMapEntry<V> rightEntry)
    {
      Entry = entry;
      RightEntry = rightEntry;
    }

    /// Prints the key value pair
    public override string ToString() => "h2:" + Entry + "->" + RightEntry;
  }

  /// <summary>
  /// The tree always contains Left and Right node, for the missing leaf we have <see cref="ImMapBranch{V}"/>
  /// </summary>
  public sealed class ImMapTree<V> : ImMap<V>
  {
    /// Starts from 2
    public override int Height => TreeHeight;

    /// Starts from 2 - allows to access the field directly when you know it is a Tree
    public int TreeHeight;

    /// Contains the once created data node
    public readonly ImMapEntry<V> Entry;

    /// Left sub-tree/branch, or empty.
    public ImMap<V> Left;

    /// Right sub-tree/branch, or empty.md
    public ImMap<V> Right;

    internal ImMapTree(ImMapEntry<V> entry, ImMap<V> left, ImMap<V> right, int height)
    {
      Entry = entry;
      Left = left;
      Right = right;
      TreeHeight = height;
    }

    internal ImMapTree(ImMapEntry<V> entry, ImMapEntry<V> leftEntry, ImMapEntry<V> rightEntry)
    {
      Entry = entry;
      Left = leftEntry;
      Right = rightEntry;
      TreeHeight = 2;
    }

    /// <summary>Outputs the brief tree info - mostly for debugging purposes</summary>
    public override string ToString() =>
        "h" + Height + ":" + Entry
            + "->(" + (Left is ImMapTree<V> leftTree ? "h" + leftTree.TreeHeight + ":" + leftTree.Entry : "" + Left)
            + ", " + (Right is ImMapTree<V> rightTree ? "h" + rightTree.TreeHeight + ":" + rightTree.Entry : "" + Right)
            + ")";

    /// <summary>Adds or updates the left or right branch</summary>
    public ImMapTree<V> AddOrUpdateLeftOrRightEntry(int key, ImMapEntry<V> entry)
    {
      if (key < Entry.Key)
      {
        var left = Left;
        if (left is ImMapTree<V> leftTree)
        {
          if (key == leftTree.Entry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(entry, leftTree.Left, leftTree.Right, leftTree.TreeHeight),
                Right, TreeHeight);

          var newLeftTree = leftTree.AddOrUpdateLeftOrRightEntry(key, entry);
          return newLeftTree.TreeHeight == leftTree.TreeHeight
              ? new ImMapTree<V>(Entry, newLeftTree, Right, TreeHeight)
              : BalanceNewLeftTree(newLeftTree);
        }

        if (left is ImMapBranch<V> leftBranch)
        {
          if (key < leftBranch.Entry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.Entry, entry, leftBranch.RightEntry),
                Right, TreeHeight);

          if (key > leftBranch.Entry.Key)
          {
            var newLeft =
                //            5                         5
                //       2        ?  =>             3        ?
                //         3                      2   4
                //          4
                key > leftBranch.RightEntry.Key ? new ImMapTree<V>(leftBranch.RightEntry, leftBranch.Entry, entry)
                //            5                         5
                //      2          ?  =>            2.5        ?
                //          3                      2   3
                //       2.5  
                : key < leftBranch.RightEntry.Key ? new ImMapTree<V>(entry, leftBranch.Entry, leftBranch.RightEntry)
                : (ImMap<V>)new ImMapBranch<V>(leftBranch.Entry, entry);

            return new ImMapTree<V>(Entry, newLeft, Right, TreeHeight);
          }

          return new ImMapTree<V>(Entry,
              new ImMapBranch<V>(entry, leftBranch.RightEntry), Right, TreeHeight);
        }

        var leftLeaf = (ImMapEntry<V>)left;
        return key > leftLeaf.Key ? new ImMapTree<V>(Entry, new ImMapBranch<V>(leftLeaf, entry), Right, 3)
            : key < leftLeaf.Key ? new ImMapTree<V>(Entry, new ImMapBranch<V>(entry, leftLeaf), Right, 3)
            : new ImMapTree<V>(Entry, entry, Right, TreeHeight);
      }
      else
      {
        var right = Right;
        if (right is ImMapTree<V> rightTree)
        {
          if (key == rightTree.Entry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(entry, rightTree.Left, rightTree.Right, rightTree.TreeHeight),
                TreeHeight);

          var newRightTree = rightTree.AddOrUpdateLeftOrRightEntry(key, entry);
          return newRightTree.TreeHeight == rightTree.TreeHeight
              ? new ImMapTree<V>(Entry, Left, newRightTree, TreeHeight)
              : BalanceNewRightTree(newRightTree);
        }

        if (right is ImMapBranch<V> rightBranch)
        {
          if (key > rightBranch.Entry.Key)
          {
            var newRight =
                //      5                5      
                //  ?       6    =>  ?       8  
                //            8            6   !
                //              !               
                key > rightBranch.RightEntry.Key ? new ImMapTree<V>(rightBranch.RightEntry, rightBranch.Entry, entry)
                //      5                 5      
                //  ?       6     =>  ?       7  
                //              8            6  8
                //            7               
                : key < rightBranch.RightEntry.Key ? new ImMapTree<V>(entry, rightBranch.Entry, rightBranch.RightEntry)
                : (ImMap<V>)new ImMapBranch<V>(rightBranch.Entry, entry);

            return new ImMapTree<V>(Entry, Left, newRight, TreeHeight);
          }

          if (key < rightBranch.Entry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.Entry, entry, rightBranch.RightEntry),
                TreeHeight);

          return new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(entry, rightBranch.RightEntry), TreeHeight);
        }

        var rightLeaf = (ImMapEntry<V>)right;
        return key > rightLeaf.Key
                ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(rightLeaf, entry), 3)
            : key < rightLeaf.Key
                ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(entry, rightLeaf), 3)
                : new ImMapTree<V>(Entry, Left, entry, TreeHeight);
      }
    }

    /// <summary>Adds the left or right branch</summary>
    public ImMapTree<V> AddUnsafeLeftOrRightEntry(int key, ImMapEntry<V> entry)
    {
      if (key < Entry.Key)
      {
        var left = Left;
        if (left is ImMapTree<V> leftTree)
        {
          var newLeftTree = leftTree.AddUnsafeLeftOrRightEntry(key, entry);
          return newLeftTree.TreeHeight == leftTree.TreeHeight
              ? new ImMapTree<V>(Entry, newLeftTree, Right, TreeHeight)
              : BalanceNewLeftTree(newLeftTree);
        }

        if (left is ImMapBranch<V> leftBranch)
        {
          if (key < leftBranch.Entry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.Entry, entry, leftBranch.RightEntry),
                Right, TreeHeight);

          var newLeft = key > leftBranch.RightEntry.Key
                  ? new ImMapTree<V>(leftBranch.RightEntry, leftBranch.Entry, entry)
                  : new ImMapTree<V>(entry, leftBranch.Entry, leftBranch.RightEntry);

          return new ImMapTree<V>(Entry, newLeft, Right, TreeHeight);
        }

        var leftLeaf = (ImMapEntry<V>)left;
        return key > leftLeaf.Key
            ? new ImMapTree<V>(Entry, new ImMapBranch<V>(leftLeaf, entry), Right, 3)
            : new ImMapTree<V>(Entry, new ImMapBranch<V>(entry, leftLeaf), Right, 3);
      }
      else
      {
        var right = Right;
        if (right is ImMapTree<V> rightTree)
        {
          var newRightTree = rightTree.AddUnsafeLeftOrRightEntry(key, entry);
          return newRightTree.TreeHeight == rightTree.TreeHeight
              ? new ImMapTree<V>(Entry, Left, newRightTree, TreeHeight)
              : BalanceNewRightTree(newRightTree);
        }

        if (right is ImMapBranch<V> rightBranch)
        {
          if (key > rightBranch.Entry.Key)
          {
            var newRight = key > rightBranch.RightEntry.Key
                ? new ImMapTree<V>(rightBranch.RightEntry, rightBranch.Entry, entry)
                : new ImMapTree<V>(entry, rightBranch.Entry, rightBranch.RightEntry);

            return new ImMapTree<V>(Entry, Left, newRight, TreeHeight);
          }

          return new ImMapTree<V>(Entry, Left,
              new ImMapTree<V>(rightBranch.Entry, entry, rightBranch.RightEntry),
              TreeHeight);
        }

        var rightLeaf = (ImMapEntry<V>)right;
        return key > rightLeaf.Key
            ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(rightLeaf, entry), 3)
            : new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(entry, rightLeaf), 3);
      }
    }

    /// <summary>Adds to the left or right branch, or keeps the un-modified map</summary>
    public ImMapTree<V> AddOrKeepLeftOrRight(int key, V value)
    {
      if (key < Entry.Key)
      {
        var left = Left;
        if (left is ImMapTree<V> leftTree)
        {
          if (key == leftTree.Entry.Key)
            return this;

          var newLeftTree = leftTree.AddOrKeepLeftOrRight(key, value);
          return newLeftTree == leftTree ? this
              : newLeftTree.TreeHeight == leftTree.TreeHeight
                  ? new ImMapTree<V>(Entry, newLeftTree, Right, TreeHeight)
                  : BalanceNewLeftTree(newLeftTree);
        }

        if (left is ImMapBranch<V> leftBranch)
        {
          if (key < leftBranch.Entry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.Entry, new ImMapEntry<V>(key, value), leftBranch.RightEntry),
                Right, TreeHeight);
          if (key > leftBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.RightEntry, leftBranch.Entry, new ImMapEntry<V>(key, value)),
                Right, TreeHeight);
          if (key > leftBranch.Entry.Key && key < leftBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(new ImMapEntry<V>(key, value), leftBranch.Entry, leftBranch.RightEntry),
                Right, TreeHeight);
          return this;
        }

        var leftLeaf = (ImMapEntry<V>)left;
        return key > leftLeaf.Key
                ? new ImMapTree<V>(Entry, new ImMapBranch<V>(leftLeaf, new ImMapEntry<V>(key, value)), Right, 3)
            : key < leftLeaf.Key
                ? new ImMapTree<V>(Entry, new ImMapBranch<V>(new ImMapEntry<V>(key, value), leftLeaf), Right, 3)
            : this;
      }
      else
      {
        var right = Right;
        if (right is ImMapTree<V> rightTree)
        {
          if (key == rightTree.Entry.Key)
            return this;

          var newRightTree = rightTree.AddOrKeepLeftOrRight(key, value);
          return newRightTree == rightTree ? this
              : newRightTree.TreeHeight == rightTree.TreeHeight
                  ? new ImMapTree<V>(Entry, Left, newRightTree, TreeHeight)
                  : BalanceNewRightTree(newRightTree);
        }

        if (right is ImMapBranch<V> rightBranch)
        {
          if (key > rightBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.RightEntry, rightBranch.Entry, new ImMapEntry<V>(key, value)),
                TreeHeight);
          if (key < rightBranch.Entry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.Entry, new ImMapEntry<V>(key, value), rightBranch.RightEntry),
                TreeHeight);
          if (key > rightBranch.Entry.Key && key < rightBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(new ImMapEntry<V>(key, value), rightBranch.Entry, rightBranch.RightEntry),
                TreeHeight);
          return this;
        }

        var rightLeaf = (ImMapEntry<V>)right;
        return key > rightLeaf.Key
            ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(rightLeaf, new ImMapEntry<V>(key, value)), 3)
            : key < rightLeaf.Key
                ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(new ImMapEntry<V>(key, value), rightLeaf), 3)
            : this;
      }
    }

    /// <summary>Adds to the left or right branch, or keeps the un-modified map</summary>
    public ImMapTree<V> AddOrKeepLeftOrRight(int key)
    {
      if (key < Entry.Key)
      {
        var left = Left;
        if (left is ImMapTree<V> leftTree)
        {
          if (key == leftTree.Entry.Key)
            return this;

          var newLeftTree = leftTree.AddOrKeepLeftOrRight(key);
          return newLeftTree == leftTree ? this
              : newLeftTree.TreeHeight == leftTree.TreeHeight
                  ? new ImMapTree<V>(Entry, newLeftTree, Right, TreeHeight)
                  : BalanceNewLeftTree(newLeftTree);
        }

        if (left is ImMapBranch<V> leftBranch)
        {
          if (key < leftBranch.Entry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.Entry, new ImMapEntry<V>(key), leftBranch.RightEntry),
                Right, TreeHeight);
          if (key > leftBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.RightEntry, leftBranch.Entry, new ImMapEntry<V>(key)),
                Right, TreeHeight);
          if (key > leftBranch.Entry.Key && key < leftBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(new ImMapEntry<V>(key), leftBranch.Entry, leftBranch.RightEntry),
                Right, TreeHeight);
          return this;
        }

        var leftLeaf = (ImMapEntry<V>)left;
        return key > leftLeaf.Key
                ? new ImMapTree<V>(Entry, new ImMapBranch<V>(leftLeaf, new ImMapEntry<V>(key)), Right, 3)
            : key < leftLeaf.Key
                ? new ImMapTree<V>(Entry, new ImMapBranch<V>(new ImMapEntry<V>(key), leftLeaf), Right, 3)
            : this;
      }
      else
      {
        var right = Right;
        if (right is ImMapTree<V> rightTree)
        {
          if (key == rightTree.Entry.Key)
            return this;

          // note: tree always contains left and right (for the missing leaf we have a Branch)
          var newRightTree = rightTree.AddOrKeepLeftOrRight(key);
          return newRightTree == rightTree ? this
              : newRightTree.TreeHeight == rightTree.TreeHeight
                  ? new ImMapTree<V>(Entry, Left, newRightTree, TreeHeight)
                  : BalanceNewRightTree(newRightTree);
        }

        if (right is ImMapBranch<V> rightBranch)
        {
          if (key > rightBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.RightEntry, rightBranch.Entry, new ImMapEntry<V>(key)),
                TreeHeight);
          if (key < rightBranch.Entry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.Entry, new ImMapEntry<V>(key), rightBranch.RightEntry),
                TreeHeight);
          if (key > rightBranch.Entry.Key && key < rightBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(new ImMapEntry<V>(key), rightBranch.Entry, rightBranch.RightEntry),
                TreeHeight);
          return this;
        }

        var rightLeaf = (ImMapEntry<V>)right;
        return key > rightLeaf.Key
                ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(rightLeaf, new ImMapEntry<V>(key)), 3)
            : key < rightLeaf.Key
                ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(new ImMapEntry<V>(key), rightLeaf), 3)
            : this;
      }
    }

    /// <summary>Adds to the left or right branch, or keeps the un-modified map</summary>
    public ImMapTree<V> AddOrKeepLeftOrRightEntry(int key, ImMapEntry<V> entry)
    {
      if (key < Entry.Key)
      {
        var left = Left;
        if (left is ImMapTree<V> leftTree)
        {
          if (key == leftTree.Entry.Key)
            return this;

          var newLeftTree = leftTree.AddOrKeepLeftOrRightEntry(key, entry);
          return newLeftTree == leftTree ? this
              : newLeftTree.TreeHeight == leftTree.TreeHeight
                  ? new ImMapTree<V>(Entry, newLeftTree, Right, TreeHeight)
                  : BalanceNewLeftTree(newLeftTree);
        }

        if (left is ImMapBranch<V> leftBranch)
        {
          if (key < leftBranch.Entry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.Entry, entry, leftBranch.RightEntry),
                Right, TreeHeight);
          if (key > leftBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(leftBranch.RightEntry, leftBranch.Entry, entry),
                Right, TreeHeight);
          if (key > leftBranch.Entry.Key && key < leftBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry,
                new ImMapTree<V>(entry, leftBranch.Entry, leftBranch.RightEntry),
                Right, TreeHeight);
          return this;
        }

        var leftLeaf = (ImMapEntry<V>)left;
        return key > leftLeaf.Key
                ? new ImMapTree<V>(Entry, new ImMapBranch<V>(leftLeaf, entry), Right, 3)
            : key < leftLeaf.Key
                ? new ImMapTree<V>(Entry, new ImMapBranch<V>(entry, leftLeaf), Right, 3)
            : this;
      }
      else
      {
        var right = Right;
        if (right is ImMapTree<V> rightTree)
        {
          if (key == rightTree.Entry.Key)
            return this;

          // note: tree always contains left and right (for the missing leaf we have a Branch)
          var newRightTree = rightTree.AddOrKeepLeftOrRightEntry(key, entry);
          return newRightTree == rightTree ? this
              : newRightTree.TreeHeight == rightTree.TreeHeight
                  ? new ImMapTree<V>(Entry, Left, newRightTree, TreeHeight)
                  : BalanceNewRightTree(newRightTree);
        }

        if (right is ImMapBranch<V> rightBranch)
        {
          if (key > rightBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.RightEntry, rightBranch.Entry, entry),
                TreeHeight);
          if (key < rightBranch.Entry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(rightBranch.Entry, entry, rightBranch.RightEntry),
                TreeHeight);
          if (key > rightBranch.Entry.Key && key < rightBranch.RightEntry.Key)
            return new ImMapTree<V>(Entry, Left,
                new ImMapTree<V>(entry, rightBranch.Entry, rightBranch.RightEntry),
                TreeHeight);
          return this;
        }

        var rightLeaf = (ImMapEntry<V>)right;
        return key > rightLeaf.Key
            ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(rightLeaf, entry), 3)
            : key < rightLeaf.Key
                ? new ImMapTree<V>(Entry, Left, new ImMapBranch<V>(entry, rightLeaf), 3)
            : this;
      }
    }

    private ImMapTree<V> BalanceNewLeftTree(ImMapTree<V> newLeftTree)
    {
      var rightHeight = Right.Height;
      var delta = newLeftTree.TreeHeight - rightHeight;
      if (delta <= 0)
        return new ImMapTree<V>(Entry, newLeftTree, Right, TreeHeight);

      if (delta == 1)
        return new ImMapTree<V>(Entry, newLeftTree, Right, newLeftTree.TreeHeight + 1);

      // here is the balancing art comes into place
      if (rightHeight == 1)
      {
        if (newLeftTree.Left is ImMapEntry<V> leftLeftLeaf)
        {
          //            30                    15
          //    10            40 =>    10           20
          //  5     15               5    12     30    40
          //     12   20                     
          if (newLeftTree.Right is ImMapTree<V> leftRightTree)
          {
            newLeftTree.Right = leftRightTree.Left;
            newLeftTree.TreeHeight = 2;
            return new ImMapTree<V>(leftRightTree.Entry,
                newLeftTree,
                new ImMapTree<V>(Entry, leftRightTree.Right, Right, 2),
                3);
          }

          // we cannot reuse the new left tree here because it is reduced into the branch
          //           30                     15
          //    10            40 =>    5           20
          //  5    15                    10     30    40
          //         20                     
          var leftRightBranch = (ImMapBranch<V>)newLeftTree.Right;
          return new ImMapTree<V>(leftRightBranch.Entry,
              new ImMapBranch<V>(leftLeftLeaf, newLeftTree.Entry),
              new ImMapTree<V>(Entry, leftRightBranch.RightEntry, Right, 2),
              3);
        }

        newLeftTree.Right = new ImMapTree<V>(Entry, newLeftTree.Right, Right, 2);
        newLeftTree.TreeHeight = 3;
        return newLeftTree;
      }

      var leftLeftHeight = (newLeftTree.Left as ImMapTree<V>)?.TreeHeight ?? 2;
      var leftRightHeight = (newLeftTree.Right as ImMapTree<V>)?.TreeHeight ?? 2;
      if (leftLeftHeight < leftRightHeight)
      {
        var leftRightTree = (ImMapTree<V>)newLeftTree.Right;

        newLeftTree.Right = leftRightTree.Left;
        newLeftTree.TreeHeight = leftLeftHeight + 1;
        return new ImMapTree<V>(leftRightTree.Entry,
            newLeftTree,
            new ImMapTree<V>(Entry, leftRightTree.Right, Right, rightHeight + 1),
            leftLeftHeight + 2);

        //return new ImMapTree<V>(leftRightTree.Entry,
        //    new ImMapTree<V>(newLeftTree.Entry, leftLeftHeight, newLeftTree.Left, leftRightTree.Left),
        //    new ImMapTree<V>(Entry, leftRightTree.Right, rightHeight, Right));
      }

      newLeftTree.Right = new ImMapTree<V>(Entry, newLeftTree.Right, Right, leftRightHeight + 1);
      newLeftTree.TreeHeight = leftRightHeight + 2;
      return newLeftTree;
    }

    private ImMapTree<V> BalanceNewRightTree(ImMapTree<V> newRightTree)
    {
      var leftHeight = Left.Height;
      var delta = newRightTree.Height - leftHeight;
      if (delta <= 0)
        return new ImMapTree<V>(Entry, Left, newRightTree, TreeHeight);
      if (delta == 1)
        return new ImMapTree<V>(Entry, Left, newRightTree, newRightTree.TreeHeight + 1);

      if (leftHeight == 1)
      {
        // here we need to re-balance by default, because the new right tree is at least 3 level (actually exactly 3 or it would be too unbalanced)
        // double rotation needed if only the right-right is a leaf
        if (newRightTree.Right is ImMapEntry<V> == false)
        {
          newRightTree.Left = new ImMapTree<V>(Entry, Left, newRightTree.Left, 2);
          newRightTree.TreeHeight = 3;
          return newRightTree;
        }

        //        20                        30       
        // 10             40     =>    20        40  
        //            30      50     10  25    35  50
        //          25  35                           
        if (newRightTree.Left is ImMapTree<V> rightLeftTree)
        {
          newRightTree.Left = rightLeftTree.Right;
          newRightTree.TreeHeight = 2;
          return new ImMapTree<V>(rightLeftTree.Entry,
              new ImMapTree<V>(Entry, Left, rightLeftTree.Left, 2),
              newRightTree, 3);
        }

        //        20                        30       
        // 10             40     =>    10        40  
        //            30      50         20    35  50
        //              35                           
        var rightLeftBranch = (ImMapBranch<V>)newRightTree.Left;
        newRightTree.Left = rightLeftBranch.RightEntry;
        newRightTree.TreeHeight = 2;
        return new ImMapTree<V>(rightLeftBranch.Entry,
            new ImMapBranch<V>((ImMapEntry<V>)Left, Entry),
            newRightTree, 3);
      }

      var rightRightHeight = (newRightTree.Right as ImMapTree<V>)?.TreeHeight ?? 2;
      var rightLeftHeight = (newRightTree.Left as ImMapTree<V>)?.TreeHeight ?? 2;
      if (rightRightHeight < rightLeftHeight)
      {
        var rightLeftTree = (ImMapTree<V>)newRightTree.Left;
        newRightTree.Left = rightLeftTree.Right;
        // the height now should be defined by rr - because left now is shorter by 1
        newRightTree.TreeHeight = rightRightHeight + 1;
        // the whole height consequentially can be defined by `newRightTree` (rr+1) because left is consist of short Left and -2 rl.Left
        return new ImMapTree<V>(rightLeftTree.Entry,
            // Left should be >= rightLeft.Left because it maybe rightLeft.Right which defines rl height
            new ImMapTree<V>(Entry, Left, rightLeftTree.Left, leftHeight + 1),
            newRightTree,
            rightRightHeight + 2);

        //return new ImMapTree<V>(rightLeftTree.Entry,
        //    new ImMapTree<V>(Entry, leftHeight, Left, rightLeftTree.Left),
        //    new ImMapTree<V>(newRightTree.Entry, rightLeftTree.Right, rightRightHeight, newRightTree.Right));
      }

      Debug.Assert(rightLeftHeight >= leftHeight, "The whole rightHeight > leftHeight by 2, and rightRight >= leftHeight but not more than by 2");

      // we may decide on the height because the Left smaller by 2
      newRightTree.Left = new ImMapTree<V>(Entry, Left, newRightTree.Left, rightLeftHeight + 1);
      // if rr was > rl by 1 than new rl+1 should be equal height to rr now, if rr was == rl than new rl wins anyway
      newRightTree.TreeHeight = rightLeftHeight + 2;
      return newRightTree;
    }
  }

  /// <summary>ImMap methods</summary>
  public static class ImMap
  {
    /// <summary> Adds or updates the value by key in the map, always returns a modified map </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> AddOrUpdateEntry<V>(this ImMap<V> map, ImMapEntry<V> entry)
    {
      if (map == ImMap<V>.Empty)
        return entry;

      var key = entry.Key;
      if (map is ImMapEntry<V> leaf)
        return key > leaf.Key ? new ImMapBranch<V>(leaf, entry)
            : key < leaf.Key ? new ImMapBranch<V>(entry, leaf)
            : (ImMap<V>)entry;

      if (map is ImMapBranch<V> branch)
      {
        if (key > branch.Entry.Key)
          //   5                  10
          //        10     =>  5     11
          //           11           
          return key > branch.RightEntry.Key
              ? new ImMapTree<V>(branch.RightEntry, branch.Entry, entry)
              //   5               7
              //        10  =>  5     10
              //      7           
              : key < branch.RightEntry.Key // rotate if right
                  ? new ImMapTree<V>(entry, branch.Entry, branch.RightEntry)
                  : (ImMap<V>)new ImMapBranch<V>(branch.Entry, entry);

        return key < branch.Entry.Key
            ? new ImMapTree<V>(branch.Entry, entry, branch.RightEntry)
            : (ImMap<V>)new ImMapBranch<V>(entry, branch.RightEntry);
      }

      var tree = (ImMapTree<V>)map;
      return key == tree.Entry.Key
          ? new ImMapTree<V>(entry, tree.Left, tree.Right, tree.TreeHeight)
          : tree.AddOrUpdateLeftOrRightEntry(key, entry);
    }

    /// <summary> Adds or updates the value by key in the map, always returns a modified map </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> AddOrUpdate<V>(this ImMap<V> map, int key, V value) =>
        map.AddOrUpdateEntry(new ImMapEntry<V>(key, value));

    /// <summary> Adds the value by key in the map - ASSUMES that the key is not in the map, always returns a modified map </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> AddEntryUnsafe<V>(this ImMap<V> map, ImMapEntry<V> entry)
    {
      if (map == ImMap<V>.Empty)
        return entry;

      var key = entry.Key;
      if (map is ImMapEntry<V> leaf)
        return key > leaf.Key
            ? new ImMapBranch<V>(leaf, entry)
            : new ImMapBranch<V>(entry, leaf);

      if (map is ImMapBranch<V> branch)
        return key < branch.Entry.Key
                ? new ImMapTree<V>(branch.Entry, entry, branch.RightEntry)
            : key > branch.RightEntry.Key
                ? new ImMapTree<V>(branch.RightEntry, branch.Entry, entry)
                : new ImMapTree<V>(entry, branch.Entry, branch.RightEntry);

      return ((ImMapTree<V>)map).AddUnsafeLeftOrRightEntry(key, entry);
    }

    /// <summary> Adds the value for the key or returns the un-modified map if key is already present </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> AddOrKeep<V>(this ImMap<V> map, int key, V value)
    {
      if (map == ImMap<V>.Empty)
        return new ImMapEntry<V>(key, value);

      if (map is ImMapEntry<V> leaf)
        return key > leaf.Key ? new ImMapBranch<V>(leaf, new ImMapEntry<V>(key, value))
            : key < leaf.Key ? new ImMapBranch<V>(new ImMapEntry<V>(key, value), leaf)
            : map;

      if (map is ImMapBranch<V> branch)
        return key < branch.Entry.Key
                ? new ImMapTree<V>(branch.Entry, new ImMapEntry<V>(key, value), branch.RightEntry)
             : key > branch.RightEntry.Key
                ? new ImMapTree<V>(branch.RightEntry, branch.Entry, new ImMapEntry<V>(key, value))
             : key > branch.Entry.Key && key < branch.RightEntry.Key
                ? new ImMapTree<V>(new ImMapEntry<V>(key, value), branch.Entry, branch.RightEntry)
             : map;

      var tree = (ImMapTree<V>)map;
      return key != tree.Entry.Key ? tree.AddOrKeepLeftOrRight(key, value) : map;
    }

    /// <summary> Adds the entry with default value for the key or returns the un-modified map if key is already present </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> AddOrKeep<V>(this ImMap<V> map, int key)
    {
      if (map == ImMap<V>.Empty)
        return new ImMapEntry<V>(key);

      if (map is ImMapEntry<V> leaf)
        return key > leaf.Key ? new ImMapBranch<V>(leaf, new ImMapEntry<V>(key))
            : key < leaf.Key ? new ImMapBranch<V>(new ImMapEntry<V>(key), leaf)
            : map;

      if (map is ImMapBranch<V> branch)
        return key < branch.Entry.Key
                ? new ImMapTree<V>(branch.Entry, new ImMapEntry<V>(key), branch.RightEntry)
             : key > branch.RightEntry.Key
                ? new ImMapTree<V>(branch.RightEntry, branch.Entry, new ImMapEntry<V>(key))
             : key > branch.Entry.Key && key < branch.RightEntry.Key
                ? new ImMapTree<V>(new ImMapEntry<V>(key), branch.Entry, branch.RightEntry)
             : map;

      var tree = (ImMapTree<V>)map;
      return key != tree.Entry.Key ? tree.AddOrKeepLeftOrRight(key) : map;
    }

    /// <summary> Adds the entry for the key or returns the un-modified map if key is already present </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> AddOrKeepEntry<V>(this ImMap<V> map, ImMapEntry<V> entry)
    {
      if (map == ImMap<V>.Empty)
        return entry;

      var key = entry.Key;
      if (map is ImMapEntry<V> leaf)
        return key > leaf.Key ? new ImMapBranch<V>(leaf, entry)
            : key < leaf.Key ? new ImMapBranch<V>(entry, leaf)
            : map;

      if (map is ImMapBranch<V> branch)
        return key < branch.Entry.Key
                ? new ImMapTree<V>(branch.Entry, entry, branch.RightEntry)
             : key > branch.RightEntry.Key
                 ? new ImMapTree<V>(branch.RightEntry, branch.Entry, entry)
             : key > branch.Entry.Key && key < branch.RightEntry.Key
                ? new ImMapTree<V>(entry, branch.Entry, branch.RightEntry)
             : map;

      var tree = (ImMapTree<V>)map;
      return key != tree.Entry.Key ? tree.AddOrKeepLeftOrRightEntry(key, entry) : map;
    }

    ///<summary>Returns the new map with the updated value for the key, or the same map if the key was not found.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> Update<V>(this ImMap<V> map, int key, V value) =>
        map.Contains(key) ? map.UpdateImpl(key, new ImMapEntry<V>(key, value)) : map;

    ///<summary>Returns the new map with the updated value for the key, ASSUMES that the key is not in the map.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> UpdateEntryUnsafe<V>(this ImMap<V> map, ImMapEntry<V> entry) =>
        map.UpdateImpl(entry.Key, entry);

    internal static ImMap<V> UpdateImpl<V>(this ImMap<V> map, int key, ImMapEntry<V> entry)
    {
      if (map is ImMapTree<V> tree)
        return key > tree.Entry.Key ? new ImMapTree<V>(tree.Entry, tree.Left, tree.Right.UpdateImpl(key, entry), tree.TreeHeight)
            : key < tree.Entry.Key ? new ImMapTree<V>(tree.Entry, tree.Left.UpdateImpl(key, entry), tree.Right, tree.TreeHeight)
            : new ImMapTree<V>(entry, tree.Left, tree.Right, tree.TreeHeight);

      // the key was found - so it should be either entry or right entry
      if (map is ImMapBranch<V> branch)
        return key == branch.Entry.Key
            ? new ImMapBranch<V>(entry, branch.RightEntry)
            : new ImMapBranch<V>(branch.Entry, entry);

      return entry;
    }

    ///<summary>Returns the new map with the value set to default, or the same map if the key was not found.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V> UpdateToDefault<V>(this ImMap<V> map, int key) =>
        map.Contains(key) ? map.UpdateToDefaultImpl(key) : map;

    internal static ImMap<V> UpdateToDefaultImpl<V>(this ImMap<V> map, int key)
    {
      if (map is ImMapTree<V> tree)
        return key > tree.Entry.Key
            ? new ImMapTree<V>(tree.Entry, tree.Left, tree.Right.UpdateToDefaultImpl(key), tree.TreeHeight)
            : key < tree.Entry.Key
                ? new ImMapTree<V>(tree.Entry, tree.Left.UpdateToDefaultImpl(key), tree.Right, tree.TreeHeight)
                : new ImMapTree<V>(new ImMapEntry<V>(key), tree.Left, tree.Right, tree.TreeHeight);

      // the key was found - so it should be either entry or right entry
      if (map is ImMapBranch<V> branch)
        return key == branch.Entry.Key
            ? new ImMapBranch<V>(new ImMapEntry<V>(key), branch.RightEntry)
            : new ImMapBranch<V>(branch.Entry, new ImMapEntry<V>(key));

      return new ImMapEntry<V>(key);
    }

    /// <summary> Returns `true` if key is found or `false` otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool Contains<V>(this ImMap<V> map, int key)
    {
      ImMapEntry<V> entry;
      while (map is ImMapTree<V> tree)
      {
        entry = tree.Entry;
        if (key > entry.Key)
          map = tree.Right;
        else if (key < entry.Key)
          map = tree.Left;
        else
          return true;
      }

      if (map is ImMapBranch<V> branch)
        return branch.Entry.Key == key || branch.RightEntry.Key == key;

      entry = map as ImMapEntry<V>;
      return entry != null && entry.Key == key;
    }

    /// <summary> Returns the entry if key is found or null otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMapEntry<V> GetEntryOrDefault<V>(this ImMap<V> map, int key)
    {
      ImMapEntry<V> entry;
      while (map is ImMapTree<V> tree)
      {
        entry = tree.Entry;
        if (key > entry.Key)
          map = tree.Right;
        else if (key < entry.Key)
          map = tree.Left;
        else
          return entry;
      }

      if (map is ImMapBranch<V> branch)
        return branch.Entry.Key == key ? branch.Entry
            : branch.RightEntry.Key == key ? branch.RightEntry
            : null;

      entry = map as ImMapEntry<V>;
      return entry != null && entry.Key == key ? entry : null;
    }

    /// <summary>Looks for the sure present entry - in cases when we know for certain that the map contains the entry</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMapEntry<V> GetSurePresentEntry<V>(this ImMap<V> map, int key)
    {
      ImMapEntry<V> entry;
      while (map is ImMapTree<V> tree)
      {
        entry = tree.Entry;
        if (key > entry.Key)
          map = tree.Right;
        else if (key < entry.Key)
          map = tree.Left;
        else
          return entry;
      }

      if (map is ImMapBranch<V> branch)
        return branch.Entry.Key == key ? branch.Entry : branch.RightEntry;

      return (ImMapEntry<V>)map;
    }

    /// <summary> Returns the value if key is found or default value otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static V GetValueOrDefault<V>(this ImMap<V> map, int key)
    {
      ImMapEntry<V> entry;
      while (map is ImMapTree<V> tree)
      {
        entry = tree.Entry;
        if (key > entry.Key)
          map = tree.Right;
        else if (key < entry.Key)
          map = tree.Left;
        else
          return entry.Value;
      }

      if (map is ImMapBranch<V> branch)
        return branch.Entry.Key == key ? branch.Entry.Value
            : branch.RightEntry.Key == key ? branch.RightEntry.Value
            : default;

      entry = map as ImMapEntry<V>;
      if (entry != null && entry.Key == key)
        return entry.Value;

      return default;
    }

    /// <summary> Returns true if key is found and sets the value. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFind<V>(this ImMap<V> map, int key, out V value)
    {
      ImMapEntry<V> entry;
      while (map is ImMapTree<V> tree)
      {
        entry = tree.Entry;
        if (key > entry.Key)
          map = tree.Right;
        else if (key < entry.Key)
          map = tree.Left;
        else
        {
          value = entry.Value;
          return true;
        }
      }

      if (map is ImMapBranch<V> branch)
      {
        if (branch.Entry.Key == key)
        {
          value = branch.Entry.Value;
          return true;
        }

        if (branch.RightEntry.Key == key)
        {
          value = branch.RightEntry.Value;
          return true;
        }

        value = default;
        return false;
      }

      entry = map as ImMapEntry<V>;
      if (entry != null && entry.Key == key)
      {
        value = entry.Value;
        return true;
      }

      value = default;
      return false;
    }

    /// <summary> Returns true if key is found and sets the value. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFindEntry<V>(this ImMap<V> map, int key, out ImMapEntry<V> result)
    {
      ImMapEntry<V> entry;
      while (map is ImMapTree<V> tree)
      {
        entry = tree.Entry;
        if (key > entry.Key)
          map = tree.Right;
        else if (key < entry.Key)
          map = tree.Left;
        else
        {
          result = entry;
          return true;
        }
      }

      if (map is ImMapBranch<V> branch)
      {
        if (branch.Entry.Key == key)
        {
          result = branch.Entry;
          return true;
        }

        if (branch.RightEntry.Key == key)
        {
          result = branch.RightEntry;
          return true;
        }

        result = null;
        return false;
      }

      entry = map as ImMapEntry<V>;
      if (entry != null && entry.Key == key)
      {
        result = entry;
        return true;
      }

      result = null;
      return false;
    }

    /// <summary>
    /// Enumerates all the map nodes from the left to the right and from the bottom to top
    /// You may pass `parentStacks` to reuse the array memory.
    /// NOTE: the length of `parentStack` should be at least of map (height - 2) - the stack want be used for 0, 1, 2 height maps,
    /// the content of the stack is not important and could be erased.
    /// </summary>
    public static IEnumerable<ImMapEntry<V>> Enumerate<V>(this ImMap<V> map, ImMapTree<V>[] parentStack = null)
    {
      if (map == ImMap<V>.Empty)
        yield break;

      if (map is ImMapEntry<V> leaf)
        yield return leaf;
      else if (map is ImMapBranch<V> branch)
      {
        yield return branch.Entry;
        yield return branch.RightEntry;
      }
      else if (map is ImMapTree<V> tree)
      {
        if (tree.TreeHeight == 2)
        {
          yield return (ImMapEntry<V>)tree.Left;
          yield return tree.Entry;
          yield return (ImMapEntry<V>)tree.Right;
        }
        else
        {
          parentStack = parentStack ?? new ImMapTree<V>[tree.TreeHeight - 2];
          var parentIndex = -1;
          while (true)
          {
            if ((tree = map as ImMapTree<V>) != null)
            {
              if (tree.TreeHeight == 2)
              {
                yield return (ImMapEntry<V>)tree.Left;
                yield return tree.Entry;
                yield return (ImMapEntry<V>)tree.Right;
                if (parentIndex == -1)
                  break;
                tree = parentStack[parentIndex--];
                yield return tree.Entry;
                map = tree.Right;
              }
              else
              {
                parentStack[++parentIndex] = tree;
                map = tree.Left;
              }
            }
            else if ((branch = map as ImMapBranch<V>) != null)
            {
              yield return branch.Entry;
              yield return branch.RightEntry;
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              yield return tree.Entry;
              map = tree.Right;
            }
            else
            {
              yield return (ImMapEntry<V>)map;
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              yield return tree.Entry;
              map = tree.Right;
            }
          }
        }
      }
    }

    /// <summary>
    /// Folds all the map nodes with the state from left to right and from the bottom to top
    /// You may pass `parentStacks` to reuse the array memory.
    /// NOTE: the length of `parentStack` should be at least of map (height - 2) - the stack want be used for 0, 1, 2 height maps,
    /// the content of the stack is not important and could be erased.
    /// </summary>
    public static S Fold<V, S>(this ImMap<V> map, S state, Func<ImMapEntry<V>, S, S> reduce, ImMapTree<V>[] parentStack = null)
    {
      if (map == ImMap<V>.Empty)
        return state;

      if (map is ImMapEntry<V> leaf)
        state = reduce(leaf, state);
      else if (map is ImMapBranch<V> branch)
      {
        state = reduce(branch.Entry, state);
        state = reduce(branch.RightEntry, state);
      }
      else if (map is ImMapTree<V> tree)
      {
        if (tree.TreeHeight == 2)
        {
          state = reduce((ImMapEntry<V>)tree.Left, state);
          state = reduce(tree.Entry, state);
          state = reduce((ImMapEntry<V>)tree.Right, state);
        }
        else
        {
          parentStack = parentStack ?? new ImMapTree<V>[tree.TreeHeight - 2];
          var parentIndex = -1;
          while (true)
          {
            if ((tree = map as ImMapTree<V>) != null)
            {
              if (tree.TreeHeight == 2)
              {
                state = reduce((ImMapEntry<V>)tree.Left, state);
                state = reduce(tree.Entry, state);
                state = reduce((ImMapEntry<V>)tree.Right, state);
                if (parentIndex == -1)
                  break;
                tree = parentStack[parentIndex--];
                state = reduce(tree.Entry, state);
                map = tree.Right;
              }
              else
              {
                parentStack[++parentIndex] = tree;
                map = tree.Left;
              }
            }
            else if ((branch = map as ImMapBranch<V>) != null)
            {
              state = reduce(branch.Entry, state);
              state = reduce(branch.RightEntry, state);
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              state = reduce(tree.Entry, state);
              map = tree.Right;
            }
            else
            {
              state = reduce((ImMapEntry<V>)map, state);
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              state = reduce(tree.Entry, state);
              map = tree.Right;
            }
          }
        }
      }

      return state;
    }

    /// <summary>
    /// Folds all the map nodes with the state from left to right and from the bottom to top
    /// You may pass `parentStacks` to reuse the array memory.
    /// NOTE: the length of `parentStack` should be at least of map (height - 2) - the stack want be used for 0, 1, 2 height maps,
    /// the content of the stack is not important and could be erased.
    /// </summary>
    public static S Fold<V, S, A>(this ImMap<V> map, S state, A a, Func<ImMapEntry<V>, S, A, S> reduce, ImMapTree<V>[] parentStack = null)
    {
      if (map == ImMap<V>.Empty)
        return state;

      if (map is ImMapEntry<V> leaf)
        state = reduce(leaf, state, a);
      else if (map is ImMapBranch<V> branch)
      {
        state = reduce(branch.Entry, state, a);
        state = reduce(branch.RightEntry, state, a);
      }
      else if (map is ImMapTree<V> tree)
      {
        if (tree.TreeHeight == 2)
        {
          state = reduce((ImMapEntry<V>)tree.Left, state, a);
          state = reduce(tree.Entry, state, a);
          state = reduce((ImMapEntry<V>)tree.Right, state, a);
        }
        else
        {
          parentStack = parentStack ?? new ImMapTree<V>[tree.TreeHeight - 2];
          var parentIndex = -1;
          while (true)
          {
            if ((tree = map as ImMapTree<V>) != null)
            {
              if (tree.TreeHeight == 2)
              {
                state = reduce((ImMapEntry<V>)tree.Left, state, a);
                state = reduce(tree.Entry, state, a);
                state = reduce((ImMapEntry<V>)tree.Right, state, a);
                if (parentIndex == -1)
                  break;
                tree = parentStack[parentIndex--];
                state = reduce(tree.Entry, state, a);
                map = tree.Right;
              }
              else
              {
                parentStack[++parentIndex] = tree;
                map = tree.Left;
              }
            }
            else if ((branch = map as ImMapBranch<V>) != null)
            {
              state = reduce(branch.Entry, state, a);
              state = reduce(branch.RightEntry, state, a);
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              state = reduce(tree.Entry, state, a);
              map = tree.Right;
            }
            else
            {
              state = reduce((ImMapEntry<V>)map, state, a);
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              state = reduce(tree.Entry, state, a);
              map = tree.Right;
            }
          }
        }
      }

      return state;
    }

    /// <summary>
    /// Visits all the map nodes with from the left to the right and from the bottom to the top
    /// You may pass `parentStacks` to reuse the array memory.
    /// NOTE: the length of `parentStack` should be at least of map height, content is not important and could be erased.
    /// </summary>
    public static void Visit<V>(this ImMap<V> map, Action<ImMapEntry<V>> visit, ImMapTree<V>[] parentStack = null)
    {
      if (map == ImMap<V>.Empty)
        return;

      if (map is ImMapEntry<V> leaf)
        visit(leaf);
      else if (map is ImMapBranch<V> branch)
      {
        visit(branch.Entry);
        visit(branch.RightEntry);
      }
      else if (map is ImMapTree<V> tree)
      {
        if (tree.TreeHeight == 2)
        {
          visit((ImMapEntry<V>)tree.Left);
          visit(tree.Entry);
          visit((ImMapEntry<V>)tree.Right);
        }
        else
        {
          parentStack = parentStack ?? new ImMapTree<V>[tree.TreeHeight - 2];
          var parentIndex = -1;
          while (true)
          {
            if ((tree = map as ImMapTree<V>) != null)
            {
              if (tree.TreeHeight == 2)
              {
                visit((ImMapEntry<V>)tree.Left);
                visit(tree.Entry);
                visit((ImMapEntry<V>)tree.Right);
                if (parentIndex == -1)
                  break;
                tree = parentStack[parentIndex--];
                visit(tree.Entry);
                map = tree.Right;
              }
              else
              {
                parentStack[++parentIndex] = tree;
                map = tree.Left;
              }
            }
            else if ((branch = map as ImMapBranch<V>) != null)
            {
              visit(branch.Entry);
              visit(branch.RightEntry);
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              visit(tree.Entry);
              map = tree.Right;
            }
            else
            {
              visit((ImMapEntry<V>)map);
              if (parentIndex == -1)
                break;
              tree = parentStack[parentIndex--];
              visit(tree.Entry);
              map = tree.Right;
            }
          }
        }
      }
    }

    /// <summary>Wraps Key and Value payload to store inside ImMapEntry</summary>
    public struct KValue<K>
    {
      /// <summary>The key</summary>
      public K Key;
      /// <summary>The value</summary>
      public object Value;

      /// <summary>Constructs a pair</summary>
      public KValue(K key, object value)
      {
        Key = key;
        Value = value;
      }
    }

    /// <summary>Uses the user provided hash and adds or updates the tree with passed key-value. Returns a new tree.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<KValue<K>> AddOrUpdate<K>(this ImMap<KValue<K>> map, int hash, K key, object value, Update<K, object> update)
    {
      var oldEntry = map.GetEntryOrDefault(hash);
      return oldEntry == null
          ? map.AddEntryUnsafe(CreateKValueEntry(hash, key, value))
          : UpdateEntryOrAddOrUpdateConflict(map, hash, oldEntry, key, value, update);
    }

    private static ImMap<KValue<K>> UpdateEntryOrAddOrUpdateConflict<K>(ImMap<KValue<K>> map, int hash,
        ImMapEntry<KValue<K>> oldEntry, K key, object value, Update<K, object> update = null)
    {
      if (key.Equals(oldEntry.Value.Key))
      {
        value = update == null ? value : update(key, oldEntry.Value.Value, value);
        return map.UpdateEntryUnsafe(CreateKValueEntry(hash, key, value));
      }

      // add a new conflicting key value
      ImMapEntry<KValue<K>>[] newConflicts;
      if (oldEntry.Value.Value is ImMapEntry<KValue<Type>>[] conflicts)
      {
        // entry is already containing the conflicted entries
        var conflictCount = conflicts.Length;
        var conflictIndex = conflictCount - 1;
        while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Value.Key))
          --conflictIndex;

        if (conflictIndex != -1)
        {
          // update the existing conflict
          newConflicts = new ImMapEntry<KValue<K>>[conflictCount];
          Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
          value = update == null ? value : update(key, conflicts[conflictIndex].Value.Value, value);
          newConflicts[conflictIndex] = CreateKValueEntry(hash, key, value);
        }
        else
        {
          // add the new conflicting value
          newConflicts = new ImMapEntry<KValue<K>>[conflictCount + 1];
          Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
          newConflicts[conflictCount] = CreateKValueEntry(hash, key, value);
        }
      }
      else
      {
        newConflicts = new[] { oldEntry, CreateKValueEntry(hash, key, value) };
      }

      var conflictsEntry = new ImMapEntry<KValue<K>>(hash);
      conflictsEntry.Value.Value = newConflicts;
      return map.UpdateEntryUnsafe(conflictsEntry);
    }

    /// <summary>Efficiently creates the new entry</summary>
    [MethodImpl((MethodImplOptions)256)]
    private static ImMapEntry<KValue<K>> CreateKValueEntry<K>(int hash, K key, object value)
    {
      var newEntry = new ImMapEntry<KValue<K>>(hash);
      newEntry.Value.Key = key;
      newEntry.Value.Value = value;
      return newEntry;
    }

    /// <summary>Efficiently creates the new entry</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMapEntry<KValue<K>> CreateKValueEntry<K>(int hash, K key)
    {
      var newEntry = new ImMapEntry<KValue<K>>(hash);
      newEntry.Value.Key = key;
      return newEntry;
    }

    /// <summary>Uses the user provided hash and adds or updates the tree with passed key-value. Returns a new tree.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<KValue<K>> AddOrUpdate<K>(this ImMap<KValue<K>> map, int hash, K key, object value) =>
        map.AddOrUpdate(hash, CreateKValueEntry(hash, key, value));

    /// <summary>Adds or updates the Type-keyed entry with the value. Returns a new tree.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<KValue<Type>> AddOrUpdate(this ImMap<KValue<Type>> map, Type type, object value)
    {
      var hash = RuntimeHelpers.GetHashCode(type);
      return map.AddOrUpdate(hash, CreateKValueEntry(hash, type, value));
    }

    /// <summary>Uses the provided hash and adds or updates the tree with the passed key-value. Returns a new tree.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<KValue<K>> AddOrUpdate<K>(this ImMap<KValue<K>> map, int hash, ImMapEntry<KValue<K>> entry)
    {
      var oldEntry = map.GetEntryOrDefault(hash);
      return oldEntry == null
          ? map.AddEntryUnsafe(entry)
          : UpdateEntryOrAddOrUpdateConflict(map, hash, oldEntry, entry);
    }

    private static ImMap<KValue<K>> UpdateEntryOrAddOrUpdateConflict<K>(ImMap<KValue<K>> map, int hash,
        ImMapEntry<KValue<K>> oldEntry, ImMapEntry<KValue<K>> newEntry)
    {
      if (newEntry.Value.Key.Equals(oldEntry.Value.Key))
        return map.UpdateEntryUnsafe(newEntry);

      // add a new conflicting key value
      ImMapEntry<KValue<K>>[] newConflicts;
      if (oldEntry.Value.Value is ImMapEntry<KValue<Type>>[] conflicts)
      {
        // entry is already containing the conflicted entries
        var key = newEntry.Value.Key;
        var conflictCount = conflicts.Length;
        var conflictIndex = conflictCount - 1;
        while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Value.Key))
          --conflictIndex;

        if (conflictIndex != -1)
        {
          // update the existing conflict
          newConflicts = new ImMapEntry<KValue<K>>[conflictCount];
          Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
          newConflicts[conflictIndex] = newEntry;
        }
        else
        {
          // add the new conflicting value
          newConflicts = new ImMapEntry<KValue<K>>[conflictCount + 1];
          Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
          newConflicts[conflictCount] = newEntry;
        }
      }
      else
      {
        newConflicts = new[] { oldEntry, newEntry };
      }

      return map.UpdateEntryUnsafe(CreateKValueEntry(hash, default(K), newConflicts));
    }

    /// <summary>Adds the new entry or keeps the current map if entry key is already present</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<KValue<K>> AddOrKeep<K>(this ImMap<KValue<K>> map, int hash, K key)
    {
      var oldEntry = map.GetEntryOrDefault(hash);
      return oldEntry == null
          ? map.AddEntryUnsafe(CreateKValueEntry(hash, key))
          : AddOrKeepConflict(map, hash, oldEntry, key);
    }

    /// <summary>Adds the new entry or keeps the current map if entry key is already present</summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<KValue<K>> AddOrKeep<K>(this ImMap<KValue<K>> map, int hash, K key, object value)
    {
      var oldEntry = map.GetEntryOrDefault(hash);
      return oldEntry == null
          ? map.AddEntryUnsafe(CreateKValueEntry(hash, key, value))
          : AddOrKeepConflict(map, hash, oldEntry, key, value);
    }

    private static ImMap<KValue<K>> AddOrKeepConflict<K>(ImMap<KValue<K>> map, int hash,
        ImMapEntry<KValue<K>> oldEntry, K key, object value = null)
    {
      if (key.Equals(oldEntry.Value.Key))
        return map;

      // add a new conflicting key value
      ImMapEntry<KValue<K>>[] newConflicts;
      if (oldEntry.Value.Value is ImMapEntry<KValue<Type>>[] conflicts)
      {
        // entry is already containing the conflicted entries
        var conflictCount = conflicts.Length;
        var conflictIndex = conflictCount - 1;
        while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Value.Key))
          --conflictIndex;

        if (conflictIndex != -1)
          return map;

        // add the new conflicting value
        newConflicts = new ImMapEntry<KValue<K>>[conflictCount + 1];
        Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
        newConflicts[conflictCount] = CreateKValueEntry(hash, key, value);
      }
      else
      {
        newConflicts = new[] { oldEntry, CreateKValueEntry(hash, key, value) };
      }

      return map.UpdateEntryUnsafe(CreateKValueEntry(hash, default(K), newConflicts));
    }

    /// <summary>Updates the map with the new value if key is found, otherwise returns the same unchanged map.</summary>
    public static ImMap<KValue<K>> Update<K>(this ImMap<KValue<K>> map, int hash, K key, object value, Update<K, object> update = null)
    {
      var oldEntry = map.GetEntryOrDefault(hash);
      return oldEntry == null ? map : UpdateEntryOrReturnSelf(map, hash, oldEntry, key, value, update);
    }

    private static ImMap<KValue<K>> UpdateEntryOrReturnSelf<K>(ImMap<KValue<K>> map,
        int hash, ImMapEntry<KValue<K>> oldEntry, K key, object value, Update<K, object> update = null)
    {
      if (key.Equals(oldEntry.Value.Key))
      {
        value = update == null ? value : update(key, oldEntry.Value.Value, value);
        return map.UpdateEntryUnsafe(CreateKValueEntry(hash, key, value));
      }

      // add a new conflicting key value
      ImMapEntry<KValue<K>>[] newConflicts;
      if (oldEntry.Value.Value is ImMapEntry<KValue<Type>>[] conflicts)
      {
        // entry is already containing the conflicted entries
        var conflictCount = conflicts.Length;
        var conflictIndex = conflictCount - 1;
        while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Value.Key))
          --conflictIndex;

        if (conflictIndex == -1)
          return map;

        // update the existing conflict
        newConflicts = new ImMapEntry<KValue<K>>[conflictCount];
        Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
        value = update == null ? value : update(key, conflicts[conflictIndex].Value.Value, value);
        newConflicts[conflictIndex] = CreateKValueEntry(hash, key, value);
      }
      else
      {
        return map;
      }

      return map.UpdateEntryUnsafe(CreateKValueEntry(hash, default(K), newConflicts));
    }

    /// <summary>Updates the map with the default value if the key is found, otherwise returns the same unchanged map.</summary>
    public static ImMap<KValue<K>> UpdateToDefault<K>(this ImMap<KValue<K>> map, int hash, K key)
    {
      var oldEntry = map.GetEntryOrDefault(hash);
      return oldEntry == null ? map : UpdateEntryOrReturnSelf(map, hash, oldEntry, key);
    }

    private static ImMap<KValue<K>> UpdateEntryOrReturnSelf<K>(ImMap<KValue<K>> map,
        int hash, ImMapEntry<KValue<K>> oldEntry, K key)
    {
      if (key.Equals(oldEntry.Value.Key))
        return map.UpdateEntryUnsafe(CreateKValueEntry(hash, key));

      // add a new conflicting key value
      ImMapEntry<KValue<K>>[] newConflicts;
      if (oldEntry.Value.Value is ImMapEntry<KValue<Type>>[] conflicts)
      {
        // entry is already containing the conflicted entries
        var conflictCount = conflicts.Length;
        var conflictIndex = conflictCount - 1;
        while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Value.Key))
          --conflictIndex;

        if (conflictIndex == -1)
          return map;

        // update the existing conflict
        newConflicts = new ImMapEntry<KValue<K>>[conflictCount];
        Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
        newConflicts[conflictIndex] = CreateKValueEntry(hash, key);
      }
      else
      {
        return map;
      }

      var conflictsEntry = new ImMapEntry<KValue<K>>(hash);
      conflictsEntry.Value.Value = newConflicts;
      return map.UpdateEntryUnsafe(conflictsEntry);
    }

    /// <summary> Returns the entry if key is found or default value otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMapEntry<KValue<K>> GetEntryOrDefault<K>(this ImMap<KValue<K>> map, int hash, K key)
    {
      var entry = map.GetEntryOrDefault(hash);
      return entry != null
          ? key.Equals(entry.Value.Key) ? entry : GetConflictedEntryOrDefault(entry, key)
          : null;
    }

    /// <summary> Returns the value if key is found or default value otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static object GetValueOrDefault<K>(this ImMap<KValue<K>> map, int hash, K key) =>
        map.GetEntryOrDefault(hash, key)?.Value.Value;

    /// <summary> Sets the value if key is found or returns false otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFind<K>(this ImMap<KValue<K>> map, int hash, K key, out object value)
    {
      var entry = map.GetEntryOrDefault(hash, key);
      if (entry != null)
      {
        value = entry.Value.Value;
        return true;
      }

      value = null;
      return false;
    }

    /// <summary> Returns the entry if key is found or `null` otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static ImMapEntry<KValue<Type>> GetEntryOrDefault(this ImMap<KValue<Type>> map, int hash, Type type)
    {
      var entry = map.GetEntryOrDefault(hash);
      return entry != null
          ? entry.Value.Key == type ? entry : GetConflictedEntryOrDefault(entry, type)
          : null;
    }

    /// <summary> Returns the value if the Type key is found or default value otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static object GetValueOrDefault(this ImMap<KValue<Type>> map, int hash, Type typeKey) =>
        map.GetEntryOrDefault(hash, typeKey)?.Value.Value;

    /// <summary> Returns the value if the Type key is found or default value otherwise. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static object GetValueOrDefault(this ImMap<KValue<Type>> map, Type typeKey) =>
        map.GetEntryOrDefault(RuntimeHelpers.GetHashCode(typeKey), typeKey)?.Value.Value;

    internal static ImMapEntry<KValue<K>> GetConflictedEntryOrDefault<K>(ImMapEntry<KValue<K>> entry, K key)
    {
      if (entry.Value.Value is ImMapEntry<KValue<K>>[] conflicts)
        for (var i = 0; i < conflicts.Length; ++i)
          if (key.Equals(conflicts[i].Value.Key))
            return conflicts[i];
      return null;
    }

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// </summary>
    public static IEnumerable<ImMapEntry<KValue<K>>> Enumerate<K>(this ImMap<KValue<K>> map)
    {
      foreach (var entry in map.Enumerate(null))
      {
        if (entry.Value.Value is ImMapEntry<KValue<K>>[] conflicts)
          for (var i = 0; i < conflicts.Length; i++)
            yield return conflicts[i];
        else
          yield return entry;
      }
    }

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public static S Fold<K, S>(this ImMap<KValue<K>> map,
        S state, Func<ImMapEntry<KValue<K>>, S, S> reduce, ImMapTree<KValue<K>>[] parentsStack = null) =>
            map.Fold(state, reduce, (entry, s, r) =>
            {
              if (entry.Value.Value is ImMapEntry<KValue<K>>[] conflicts)
                for (var i = 0; i < conflicts.Length; i++)
                  s = r(conflicts[i], s);
              else
                s = r(entry, s);
              return s;
            },
            parentsStack);

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public static S Visit<K, S>(this ImMap<KValue<K>> map,
        S state, Action<ImMapEntry<KValue<K>>, S> effect, ImMapTree<KValue<K>>[] parentsStack = null) =>
        map.Fold(state, effect, (entry, s, eff) =>
        {
          if (entry.Value.Value is ImMapEntry<KValue<K>>[] conflicts)
            for (var i = 0; i < conflicts.Length; i++)
              eff(conflicts[i], s);
          else
            eff(entry, s);
          return s;
        },
        parentsStack);

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public static void Visit<K>(this ImMap<KValue<K>> map,
        Action<ImMapEntry<KValue<K>>> effect, ImMapTree<KValue<K>>[] parentsStack = null) =>
        map.Fold(false, effect, (entry, s, eff) =>
        {
          if (entry.Value.Value is ImMapEntry<KValue<K>>[] conflicts)
            for (var i = 0; i < conflicts.Length; i++)
              eff(conflicts[i]);
          else
            eff(entry);
          return false;
        },
        parentsStack);
  }

  /// <summary>
  /// The array of ImMap slots where the key first bits are used for FAST slot location
  /// and the slot is the reference to ImMap that can be swapped with its updated value
  /// </summary>
  public static class ImMapSlots
  {
    /// Default number of slots
    public const int SLOT_COUNT_POWER_OF_TWO = 32;

    /// The default mask to partition the key to the target slot
    public const int KEY_MASK_TO_FIND_SLOT = SLOT_COUNT_POWER_OF_TWO - 1;

    /// Creates the array with the empty slots
    [MethodImpl((MethodImplOptions)256)]
    public static ImMap<V>[] CreateWithEmpty<V>(int slotCountPowerOfTwo = SLOT_COUNT_POWER_OF_TWO)
    {
      var slots = new ImMap<V>[slotCountPowerOfTwo];
      for (var i = 0; i < slots.Length; ++i)
        slots[i] = ImMap<V>.Empty;
      return slots;
    }

    /// Returns a new tree with added or updated value for specified key.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrUpdate<V>(this ImMap<V>[] slots, int key, V value, int keyMaskToFindSlot = KEY_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[key & keyMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.AddOrUpdate(key, value), copy) != copy)
        RefAddOrUpdateSlot(ref slot, key, value);
    }

    /// Update the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefAddOrUpdateSlot<V>(ref ImMap<V> slot, int key, V value) =>
        Ref.Swap(ref slot, key, value, (x, k, v) => x.AddOrUpdate(k, v));

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrKeep<V>(this ImMap<V>[] slots, int key, V value, int keyMaskToFindSlot = KEY_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[key & keyMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.AddOrKeep(key, value), copy) != copy)
        RefAddOrKeepSlot(ref slot, key, value);
    }

    /// Update the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefAddOrKeepSlot<V>(ref ImMap<V> slot, int key, V value) =>
        Ref.Swap(ref slot, key, value, (s, k, v) => s.AddOrKeep(k, v));

    /// Adds a default value entry for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrKeep<V>(this ImMap<V>[] slots, int key, int keyMaskToFindSlot = KEY_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[key & keyMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.AddOrKeep(key), copy) != copy)
        RefAddOrKeepSlot(ref slot, key);
    }

    /// Update the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefAddOrKeepSlot<V>(ref ImMap<V> slot, int key) =>
        Ref.Swap(ref slot, key, (s, k) => s.AddOrKeep(k));

    /// <summary> Folds all map nodes without the order </summary>
    public static S Fold<V, S>(this ImMap<V>[] slots, S state, Func<ImMapEntry<V>, S, S> reduce)
    {
      var parentStack = ArrayTools.Empty<ImMapTree<V>>();
      for (var i = 0; i < slots.Length; ++i)
      {
        var map = slots[i];
        if (map == ImMap<V>.Empty)
          continue;

        if (map is ImMapEntry<V> leaf)
          state = reduce(leaf, state);
        else if (map is ImMapBranch<V> branch)
        {
          state = reduce(branch.Entry, state);
          state = reduce(branch.RightEntry, state);
        }
        else if (map is ImMapTree<V> tree)
        {
          if (tree.TreeHeight == 2)
          {
            state = reduce((ImMapEntry<V>)tree.Left, state);
            state = reduce(tree.Entry, state);
            state = reduce((ImMapEntry<V>)tree.Right, state);
          }
          else
          {
            if (parentStack.Length < tree.TreeHeight - 2)
              parentStack = new ImMapTree<V>[tree.TreeHeight - 2];
            var parentIndex = -1;
            while (true)
            {
              if ((tree = map as ImMapTree<V>) != null)
              {
                if (tree.TreeHeight == 2)
                {
                  state = reduce((ImMapEntry<V>)tree.Left, state);
                  state = reduce(tree.Entry, state);
                  state = reduce((ImMapEntry<V>)tree.Right, state);
                  if (parentIndex == -1)
                    break;
                  tree = parentStack[parentIndex--];
                  state = reduce(tree.Entry, state);
                  map = tree.Right;
                }
                else
                {
                  parentStack[++parentIndex] = tree;
                  map = tree.Left;
                }
              }
              else if ((branch = map as ImMapBranch<V>) != null)
              {
                state = reduce(branch.Entry, state);
                state = reduce(branch.RightEntry, state);
                if (parentIndex == -1)
                  break;
                tree = parentStack[parentIndex--];
                state = reduce(tree.Entry, state);
                map = tree.Right;
              }
              else
              {
                state = reduce((ImMapEntry<V>)map, state);
                if (parentIndex == -1)
                  break;
                tree = parentStack[parentIndex--];
                state = reduce(tree.Entry, state);
                map = tree.Right;
              }
            }
          }
        }
      }

      return state;
    }
  }

  /// <summary>Wraps the stored data with "fixed" reference semantics - when added to the tree it did not change or reconstructed in memory</summary>
  public class ImHashMapEntry<K, V>
  {
    /// Empty thingy
    public static readonly ImHashMapEntry<K, V> Empty = new ImHashMapEntry<K, V>();

    /// Key hash
    public readonly int Hash;

    ///  The key
    public readonly K Key;

    /// The value - may be mutated implementing the Ref CAS semantics if needed
    public V Value;

    private ImHashMapEntry() { }

    /// Constructs the data
    public ImHashMapEntry(int hash, K key, V value)
    {
      Hash = hash;
      Key = key;
      Value = value;
    }

    /// Constructs the data with the default value
    public ImHashMapEntry(int hash, K key)
    {
      Hash = hash;
      Key = key;
    }

    /// <summary>Outputs the brief tree info - mostly for debugging purposes</summary>
    public override string ToString() => Key + ": " + Value;
  }

  /// Stores ALL the data in `Conflicts` array, the fields except the `hash` are just fillers.
  /// This way we preserve the once created `ImHashMapData` so that client can hold the reference to it and update the Value if needed.
  public sealed class ImHashMapConflicts<K, V> : ImHashMapEntry<K, V>
  {
    /// Conflicted data
    public readonly ImHashMapEntry<K, V>[] Conflicts;

    /// <inheritdoc />
    public ImHashMapConflicts(int hash, params ImHashMapEntry<K, V>[] conflicts) : base(hash, default, default) =>
        Conflicts = conflicts;
  }

  /// Immutable http://en.wikipedia.org/wiki/AVL_tree 
  /// where node key is the hash code of <typeparamref name="K"/>
  public sealed class ImHashMap<K, V>
  {
    /// Empty map to start with.
    public static readonly ImHashMap<K, V> Empty = new ImHashMap<K, V>();

    /// <summary>Calculated key hash.</summary>
    public int Hash
    {
      [MethodImpl((MethodImplOptions)256)]
      get => Entry.Hash;
    }

    /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
    public K Key
    {
      [MethodImpl((MethodImplOptions)256)]
      get => Entry.Key;
    }

    /// <summary>Value of any type V.</summary>
    public V Value
    {
      [MethodImpl((MethodImplOptions)256)]
      get => Entry.Value;
    }

    /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
    public ImHashMapEntry<K, V>[] Conflicts
    {
      [MethodImpl((MethodImplOptions)256)]
      get => (Entry as ImHashMapConflicts<K, V>)?.Conflicts;
    }

    /// <summary>Left sub-tree/branch, or empty.</summary>
    public ImHashMap<K, V> Left;

    /// <summary>Right sub-tree/branch, or empty.</summary>
    public ImHashMap<K, V> Right;

    /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
    public int Height;

    /// <summary>Returns true if tree is empty.</summary>
    public bool IsEmpty => Height == 0;

    /// <summary>The entry which is allocated once and can be used as a "fixed" reference to the Key and Value</summary>
    public readonly ImHashMapEntry<K, V> Entry;

    internal ImHashMap() => Entry = ImHashMapEntry<K, V>.Empty;

    /// Creates  leaf node
    public ImHashMap(int hash, K key, V value)
    {
      Entry = new ImHashMapEntry<K, V>(hash, key, value);
      Left = Empty;
      Right = Empty;
      Height = 1;
    }

    /// Creates a leaf node with default value
    public ImHashMap(int hash, K key)
    {
      Entry = new ImHashMapEntry<K, V>(hash, key);
      Left = Empty;
      Right = Empty;
      Height = 1;
    }

    /// Creates a leaf node
    public ImHashMap(ImHashMapEntry<K, V> entry)
    {
      Entry = entry;
      Left = Empty;
      Right = Empty;
      Height = 1;
    }

    /// Creates the tree and calculates the height for you
    public ImHashMap(ImHashMapEntry<K, V> entry, ImHashMap<K, V> left, ImHashMap<K, V> right)
    {
      Entry = entry;
      Left = left;
      Right = right;
      Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
    }

    /// Creates the tree with the known height
    public ImHashMap(ImHashMapEntry<K, V> entry, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
    {
      Entry = entry;
      Left = left;
      Right = right;
      Height = height;
    }

    /// <summary>Outputs the brief tree info - mostly for debugging purposes</summary>
    public override string ToString() => Height == 0 ? "empty"
        : "(" + Entry
        + ") -> (" + (Left.Height == 0 ? "empty" : Left.Entry + " of height " + Left.Height)
        + ", " + (Right.Height == 0 ? "empty" : Right.Entry + " of height " + Right.Height)
        + ")";

    /// <summary>Uses the user provided hash and adds and updates the tree with passed key-value. Returns a new tree.</summary>
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrUpdate(int hash, K key, V value) =>
        Height == 0 ? new ImHashMap<K, V>(hash, key, value)
        : hash == Hash ? UpdateValueOrAddOrUpdateConflict(hash, key, value)
        : AddOrUpdateLeftOrRight(hash, key, value);

    /// Adds and updates the tree with passed key-value. Returns a new tree.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrUpdate(K key, V value) =>
        AddOrUpdate(key.GetHashCode(), key, value);

    private ImHashMap<K, V> UpdateValueOrAddOrUpdateConflict(int hash, K key, V value)
    {
      var conflictsData = Entry as ImHashMapConflicts<K, V>;
      return conflictsData == null && (ReferenceEquals(key, Key) || key.Equals(Key))
          ? new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value), Left, Right, Height)
          : AddOrUpdateConflict(conflictsData, hash, key, value);
    }

    internal enum DoAddOrUpdateConflicts { AddOrUpdate, AddOrKeep, Update }

    private ImHashMap<K, V> AddOrUpdateConflict(ImHashMapConflicts<K, V> conflictsData, int hash, K key, V value,
        Update<K, V> update = null, DoAddOrUpdateConflicts doWhat = DoAddOrUpdateConflicts.AddOrUpdate)
    {
      if (conflictsData == null)
        return doWhat == DoAddOrUpdateConflicts.Update
            ? this
            : new ImHashMap<K, V>(
                new ImHashMapConflicts<K, V>(hash, Entry, new ImHashMapEntry<K, V>(hash, key, value)),
                Left, Right, Height);

      var conflicts = conflictsData.Conflicts;
      var conflictCount = conflicts.Length;
      var conflictIndex = conflictCount - 1;
      while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Key))
        --conflictIndex;

      ImHashMapEntry<K, V>[] newConflicts;
      if (conflictIndex != -1)
      {
        if (doWhat == DoAddOrUpdateConflicts.AddOrKeep)
          return this;

        // update the existing conflicted value
        newConflicts = new ImHashMapEntry<K, V>[conflictCount];
        Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
        var newValue = update == null ? value : update(key, conflicts[conflictIndex].Value, value);
        newConflicts[conflictIndex] = new ImHashMapEntry<K, V>(hash, key, newValue);
      }
      else
      {
        if (doWhat == DoAddOrUpdateConflicts.Update)
          return this;

        // add the new conflicting value
        newConflicts = new ImHashMapEntry<K, V>[conflictCount + 1];
        Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
        newConflicts[conflictCount] = new ImHashMapEntry<K, V>(hash, key, value);
      }

      return new ImHashMap<K, V>(new ImHashMapConflicts<K, V>(hash, newConflicts), Left, Right, Height);
    }

    private ImHashMap<K, V> AddOrUpdateLeftOrRight(int hash, K key, V value)
    {
      if (hash < Hash)
      {
        if (Left.Height == 0)
          return new ImHashMap<K, V>(Entry, new ImHashMap<K, V>(hash, key, value), Right, 2);

        if (Left.Hash == hash)
          return new ImHashMap<K, V>(Entry, Left.UpdateValueOrAddOrUpdateConflict(hash, key, value), Right, Height);

        if (Right.Height == 0)
        {
          if (hash < Left.Hash)
            return new ImHashMap<K, V>(Left.Entry,
                new ImHashMap<K, V>(hash, key, value), new ImHashMap<K, V>(Entry), 2);

          return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value),
              new ImHashMap<K, V>(Left.Entry), new ImHashMap<K, V>(Entry), 2);
        }

        var left = Left.AddOrUpdateLeftOrRight(hash, key, value);
        return left.Height > Right.Height + 1
            ? BalanceNewLeftTree(left)
            : new ImHashMap<K, V>(Entry, left, Right);
      }
      else
      {
        if (Right.Height == 0)
          return new ImHashMap<K, V>(Entry, Left, new ImHashMap<K, V>(hash, key, value), 2);

        if (Right.Hash == hash)
          return new ImHashMap<K, V>(Entry, Left, Right.UpdateValueOrAddOrUpdateConflict(hash, key, value), Height);

        if (Left.Height == 0)
        {
          if (hash < Right.Hash)
            return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value),
                new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(Right.Entry), 2);

          return new ImHashMap<K, V>(Right.Entry,
              new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(hash, key, value), 2);
        }

        var right = Right.AddOrUpdateLeftOrRight(hash, key, value);
        return right.Height > Left.Height + 1
            ? BalanceNewRightTree(right)
            : new ImHashMap<K, V>(Entry, Left, right);
      }
    }

    private ImHashMap<K, V> BalanceNewLeftTree(ImHashMap<K, V> newLeftTree)
    {
      var leftLeft = newLeftTree.Left;
      var leftLeftHeight = leftLeft.Height;

      var leftRight = newLeftTree.Right;
      var leftRightHeight = leftRight.Height;

      if (leftRightHeight > leftLeftHeight)
      {
        newLeftTree.Right = leftRight.Left;
        newLeftTree.Height = leftLeftHeight + 1;
        return new ImHashMap<K, V>(leftRight.Entry,
            newLeftTree,
            new ImHashMap<K, V>(Entry, leftRight.Right, Right, Right.Height + 1),
            leftLeftHeight + 2);

        //return new ImHashMap<K, V>(leftRight.Entry,
        //    new ImHashMap<K, V>(newLeftTree.Entry, leftLeft, leftRight.Left),
        //    new ImHashMap<K, V>(Entry, leftRight.Right, Right));
      }

      newLeftTree.Right = new ImHashMap<K, V>(Entry, leftRight, Right, leftRightHeight + 1);
      newLeftTree.Height = leftRightHeight + 2;
      return newLeftTree;

      //return new ImHashMap<K, V>(newLeftTree.Entry,
      //    leftLeft, new ImHashMap<K, V>(Entry, leftRight, Right));
    }

    // Note that Left is by 2 less deep than `newRightTree` - means that at `newRightTree.Left/Right` is at least of Left height or deeper
    private ImHashMap<K, V> BalanceNewRightTree(ImHashMap<K, V> newRightTree)
    {
      var rightLeft = newRightTree.Left;
      var rightLeftHeight = rightLeft.Height;

      var rightRight = newRightTree.Right;
      var rightRightHeight = rightRight.Height;

      if (rightLeftHeight > rightRightHeight) // 1 greater - not 2 greater because it would be too unbalanced
      {
        newRightTree.Left = rightLeft.Right;
        // the height now should be defined by rr - because left now is shorter by 1
        newRightTree.Height = rightRightHeight + 1;
        // the whole height consequentially can be defined by `newRightTree` (rr+1) because left is consist of short Left and -2 rl.Left
        return new ImHashMap<K, V>(rightLeft.Entry,
            // Left should be >= rightLeft.Left because it maybe rightLeft.Right which defines rl height
            new ImHashMap<K, V>(Entry, Left, rightLeft.Left, height: Left.Height + 1),
            newRightTree, rightRightHeight + 2);

        //return new ImHashMap<K, V>(rightLeft.Entry,
        //    new ImHashMap<K, V>(Entry, Left, rightLeft.Left),
        //    new ImHashMap<K, V>(newRightTree.Entry, rightLeft.Right, rightRight));
      }

      // we may decide on the height because the Left smaller by 2
      newRightTree.Left = new ImHashMap<K, V>(Entry, Left, rightLeft, rightLeftHeight + 1);
      // if rr was > rl by 1 than new rl+1 should be equal height to rr now, if rr was == rl than new rl wins anyway
      newRightTree.Height = rightLeftHeight + 2;
      return newRightTree;

      //return new ImHashMap<K, V>(newRightTree.Entry, new ImHashMap<K, V>(Entry, Left, rightLeft), rightRight);
    }

    /// Uses the user provided hash and adds and updates the tree with passed key-value and the update function for the existing value. Returns a new tree.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<K, V> update) =>
        Height == 0 ? new ImHashMap<K, V>(hash, key, value)
        : hash == Hash ? UpdateValueOrAddOrUpdateConflict(hash, key, value, update)
        : AddOrUpdateLeftOrRightWithUpdate(hash, key, value, update);

    private ImHashMap<K, V> UpdateValueOrAddOrUpdateConflict(int hash, K key, V value, Update<K, V> update)
    {
      var conflictsData = Entry as ImHashMapConflicts<K, V>;
      return conflictsData == null && (ReferenceEquals(Key, key) || Key.Equals(key))
          ? new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, update(key, Value, value)), Left, Right, Height)
          : AddOrUpdateConflict(conflictsData, hash, key, value, update);
    }

    private ImHashMap<K, V> AddOrUpdateLeftOrRightWithUpdate(int hash, K key, V value, Update<K, V> update)
    {
      if (hash < Hash)
      {
        if (Left.Height == 0)
          return new ImHashMap<K, V>(Entry, new ImHashMap<K, V>(hash, key, value), Right, 2);

        if (Left.Hash == hash)
          return new ImHashMap<K, V>(Entry, Left.UpdateValueOrAddOrUpdateConflict(hash, key, value, update), Right, Height);

        if (Right.Height == 0)
        {
          if (hash < Left.Hash)
            return new ImHashMap<K, V>(Left.Entry, new ImHashMap<K, V>(hash, key, value), new ImHashMap<K, V>(Entry), 2);

          return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value),
              new ImHashMap<K, V>(Left.Entry), new ImHashMap<K, V>(Entry), 2);
        }

        var left = Left.AddOrUpdateLeftOrRightWithUpdate(hash, key, value, update);
        return left.Height > Right.Height + 1
            ? BalanceNewLeftTree(left)
            : new ImHashMap<K, V>(Entry, left, Right);
      }
      else
      {
        if (Right.Height == 0)
          return new ImHashMap<K, V>(Entry, Left, new ImHashMap<K, V>(hash, key, value), 2);

        if (Right.Hash == hash)
          return new ImHashMap<K, V>(Entry, Left, Right.UpdateValueOrAddOrUpdateConflict(hash, key, value, update), Height);

        if (Left.Height == 0)
        {
          if (hash < Right.Hash)
            return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value),
                new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(Right.Entry), 2);

          return new ImHashMap<K, V>(Right.Entry,
              new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(hash, key, value), 2);
        }

        var right = Right.AddOrUpdateLeftOrRightWithUpdate(hash, key, value, update);
        return right.Height > Left.Height + 1
            ? BalanceNewRightTree(right)
            : new ImHashMap<K, V>(Entry, Left, right);
      }
    }

    /// Returns a new tree with added or updated key-value. Uses the provided <paramref name="update"/> for updating the existing value.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<K, V> update) =>
        AddOrUpdate(key.GetHashCode(), key, value, update);

    /// Returns a new tree with added or updated key-value. Uses the provided <paramref name="update"/> for updating the existing value.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update) =>
        AddOrUpdate(key.GetHashCode(), key, value, update.IgnoreKey);

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrKeep(int hash, K key, V value) =>
        Height == 0 ? new ImHashMap<K, V>(hash, key, value)
        : hash == Hash ? KeepValueOrAddConflict(hash, key, value)
        : AddOrKeepLeftOrRight(hash, key, value);

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrKeep(K key, V value) =>
        AddOrKeep(key.GetHashCode(), key, value);

    private ImHashMap<K, V> KeepValueOrAddConflict(int hash, K key, V value)
    {
      var conflictsData = Entry as ImHashMapConflicts<K, V>;
      return conflictsData == null && (ReferenceEquals(Key, key) || Key.Equals(key)) ? this
          : AddOrUpdateConflict(conflictsData, hash, key, value, null, DoAddOrUpdateConflicts.AddOrKeep);
    }
    private ImHashMap<K, V> AddOrKeepLeftOrRight(int hash, K key, V value)
    {
      if (hash < Hash)
      {
        if (Left.Height == 0)
          return new ImHashMap<K, V>(Entry, new ImHashMap<K, V>(hash, key, value), Right, 2);

        if (Left.Hash == hash)
        {
          var leftWithNewConflict = Left.KeepValueOrAddConflict(hash, key, value);
          return ReferenceEquals(leftWithNewConflict, Left) ? this
              : new ImHashMap<K, V>(Entry, leftWithNewConflict, Right, Height);
        }

        if (Right.Height == 0)
        {
          if (hash < Left.Hash)
            return new ImHashMap<K, V>(Left.Entry,
                new ImHashMap<K, V>(hash, key, value), new ImHashMap<K, V>(Entry), 2);

          return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value),
              new ImHashMap<K, V>(Left.Entry), new ImHashMap<K, V>(Entry), 2);
        }

        var left = Left.AddOrKeepLeftOrRight(hash, key, value);
        if (ReferenceEquals(left, Left))
          return this;

        return left.Height > Right.Height + 1
            ? BalanceNewLeftTree(left)
            : new ImHashMap<K, V>(Entry, left, Right);
      }
      else
      {
        if (Right.Height == 0)
          return new ImHashMap<K, V>(Entry, Left, new ImHashMap<K, V>(hash, key, value), 2);

        if (Right.Hash == hash)
        {
          var rightWithNewConflict = Right.KeepValueOrAddConflict(hash, key, value);
          return ReferenceEquals(rightWithNewConflict, Right) ? this
              : new ImHashMap<K, V>(Entry, Left, rightWithNewConflict, Height);
        }

        if (Left.Height == 0)
        {
          if (hash < Right.Hash)
            return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key, value),
                new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(Right.Entry), 2);

          return new ImHashMap<K, V>(Right.Entry,
              new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(hash, key, value), 2);
        }

        var right = Right.AddOrKeepLeftOrRight(hash, key, value);
        if (ReferenceEquals(right, Right))
          return this;

        return right.Height > Left.Height + 1
            ? BalanceNewRightTree(right)
            : new ImHashMap<K, V>(Entry, Left, right);
      }
    }

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrKeep(int hash, K key) =>
        Height == 0 ? new ImHashMap<K, V>(hash, key)
        : hash == Hash ? KeepValueOrAddConflict(hash, key)
        : AddOrKeepLeftOrRight(hash, key);

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> AddOrKeep(K key) =>
        AddOrKeep(key.GetHashCode(), key);

    private ImHashMap<K, V> KeepValueOrAddConflict(int hash, K key)
    {
      var conflictsData = Entry as ImHashMapConflicts<K, V>;
      return conflictsData == null && (ReferenceEquals(Key, key) || Key.Equals(key))
          ? this : AddOrKeepConflict(conflictsData, hash, key);
    }

    private ImHashMap<K, V> AddOrKeepConflict(ImHashMapConflicts<K, V> conflictsData, int hash, K key)
    {
      if (conflictsData == null)
        return new ImHashMap<K, V>(
            new ImHashMapConflicts<K, V>(hash, Entry, new ImHashMapEntry<K, V>(hash, key)),
            Left, Right, Height);

      var conflicts = conflictsData.Conflicts;
      var conflictCount = conflicts.Length;
      var conflictIndex = conflictCount - 1;
      while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Key))
        --conflictIndex;

      if (conflictIndex != -1)
        return this;

      // add the new conflicting value
      var newConflicts = new ImHashMapEntry<K, V>[conflictCount + 1];
      Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
      newConflicts[conflictCount] = new ImHashMapEntry<K, V>(hash, key);

      return new ImHashMap<K, V>(new ImHashMapConflicts<K, V>(hash, newConflicts), Left, Right, Height);
    }

    private ImHashMap<K, V> AddOrKeepLeftOrRight(int hash, K key)
    {
      if (hash < Hash)
      {
        if (Left.Height == 0)
          return new ImHashMap<K, V>(Entry, new ImHashMap<K, V>(hash, key), Right, 2);

        if (Left.Hash == hash)
        {
          var leftWithNewConflict = Left.KeepValueOrAddConflict(hash, key);
          return ReferenceEquals(leftWithNewConflict, Left) ? this
              : new ImHashMap<K, V>(Entry, leftWithNewConflict, Right, Height);
        }

        if (Right.Height == 0)
        {
          if (hash < Left.Hash)
            return new ImHashMap<K, V>(Left.Entry,
                new ImHashMap<K, V>(hash, key), new ImHashMap<K, V>(Entry), 2);

          return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key),
              new ImHashMap<K, V>(Left.Entry), new ImHashMap<K, V>(Entry), 2);
        }

        var left = Left.AddOrKeepLeftOrRight(hash, key);
        if (ReferenceEquals(left, Left))
          return this;

        return left.Height > Right.Height + 1
            ? BalanceNewLeftTree(left)
            : new ImHashMap<K, V>(Entry, left, Right);
      }
      else
      {
        if (Right.Height == 0)
          return new ImHashMap<K, V>(Entry, Left, new ImHashMap<K, V>(hash, key), 2);

        if (Right.Hash == hash)
        {
          var rightWithNewConflict = Right.KeepValueOrAddConflict(hash, key);
          return ReferenceEquals(rightWithNewConflict, Right) ? this
              : new ImHashMap<K, V>(Entry, Left, rightWithNewConflict, Height);
        }

        if (Left.Height == 0)
        {
          if (hash < Right.Hash)
            return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key),
                new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(Right.Entry), 2);

          return new ImHashMap<K, V>(Right.Entry,
              new ImHashMap<K, V>(Entry), new ImHashMap<K, V>(hash, key), 2);
        }

        var right = Right.AddOrKeepLeftOrRight(hash, key);
        if (ReferenceEquals(right, Right))
          return this;

        return right.Height > Left.Height + 1
            ? BalanceNewRightTree(right)
            : new ImHashMap<K, V>(Entry, Left, right);
      }
    }

    /// Updates the map with the new value if key is found, otherwise returns the same unchanged map.
    public ImHashMap<K, V> Update(int hash, K key, V value, Update<K, V> update = null)
    {
      if (Height == 0)
        return this;

      // No need to balance cause we not adding or removing nodes
      if (hash < Hash)
      {
        var left = Left.Update(hash, key, value, update);
        return ReferenceEquals(left, Left) ? this : new ImHashMap<K, V>(Entry, left, Right, Height);
      }

      if (hash > Hash)
      {
        var right = Right.Update(hash, key, value, update);
        return ReferenceEquals(right, Right) ? this : new ImHashMap<K, V>(Entry, Left, right, Height);
      }

      var conflictsData = Entry as ImHashMapConflicts<K, V>;
      if (conflictsData == null && (ReferenceEquals(Key, key) || Key.Equals(key)))
        return new ImHashMap<K, V>(
            new ImHashMapEntry<K, V>(hash, key, update == null ? value : update(key, Value, value)),
            Left, Right, Height);

      return AddOrUpdateConflict(conflictsData, hash, key, value, update, DoAddOrUpdateConflicts.Update);
    }

    /// Updates the map with the new value if key is found, otherwise returns the same unchanged map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> Update(K key, V value) =>
        Update(key.GetHashCode(), key, value);

    /// Updates the map with the new value if key is found, otherwise returns the same unchanged map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> Update(K key, V value, Update<V> update) =>
        Update(key.GetHashCode(), key, value, update.IgnoreKey);

    /// Updates the map with the Default (null for reference types) value if key is found, otherwise returns the same unchanged map.
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> UpdateToDefault(int hash, K key)
    {
      if (Height == 0)
        return this;

      // No need to balance cause we not adding or removing nodes
      if (hash < Hash)
      {
        var left = Left.UpdateToDefault(hash, key);
        return left == Left ? this : new ImHashMap<K, V>(Entry, left, Right, Height);
      }

      if (hash > Hash)
      {
        var right = Right.UpdateToDefault(hash, key);
        return right == Right ? this : new ImHashMap<K, V>(Entry, Left, right, Height);
      }

      var conflictsData = Entry as ImHashMapConflicts<K, V>;
      if (conflictsData == null && (ReferenceEquals(Key, key) || Key.Equals(key)))
        return new ImHashMap<K, V>(new ImHashMapEntry<K, V>(hash, key), Left, Right, Height);

      return UpdateConflictToDefault(conflictsData, hash, key);
    }

    private ImHashMap<K, V> UpdateConflictToDefault(ImHashMapConflicts<K, V> conflictsData, int hash, K key)
    {
      if (conflictsData == null)
        return this;

      var conflicts = conflictsData.Conflicts;
      var conflictCount = conflicts.Length;
      var conflictIndex = conflictCount - 1;
      while (conflictIndex != -1 && !key.Equals(conflicts[conflictIndex].Key))
        --conflictIndex;

      if (conflictIndex == -1)
        return this;

      // update the existing conflicted value
      var newConflicts = new ImHashMapEntry<K, V>[conflictCount];
      Array.Copy(conflicts, 0, newConflicts, 0, conflictCount);
      newConflicts[conflictIndex] = new ImHashMapEntry<K, V>(hash, key);
      return new ImHashMap<K, V>(new ImHashMapConflicts<K, V>(hash, newConflicts), Left, Right, Height);
    }

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// </summary>
    public IEnumerable<ImHashMapEntry<K, V>> Enumerate()
    {
      if (Height != 0)
      {
        var parents = new ImHashMap<K, V>[Height];
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parents[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parents[parentCount--];
            if (node.Entry is ImHashMapConflicts<K, V> conflictsData)
            {
              var conflicts = conflictsData.Conflicts;
              for (var i = 0; i < conflicts.Length; i++)
                yield return conflicts[i];
            }
            else
            {
              yield return node.Entry;
            }

            node = node.Right;
          }
        }
      }
    }

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public S Fold<S>(S state, Func<ImHashMapEntry<K, V>, S, S> reduce, ImHashMap<K, V>[] parentsStack = null)
    {
      if (Height == 1 && Entry is ImHashMapConflicts<K, V> == false)
        return reduce(Entry, state);

      if (Height != 0)
      {
        parentsStack = parentsStack ?? new ImHashMap<K, V>[Height];
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parentsStack[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parentsStack[parentCount--];

            if (!(node.Entry is ImHashMapConflicts<K, V> conflicts))
              state = reduce(node.Entry, state);
            else
            {
              var conflict = conflicts.Conflicts;
              for (var i = 0; i < conflict.Length; i++)
                state = reduce(conflict[i], state);
            }

            node = node.Right;
          }
        }
      }

      return state;
    }

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public S Fold<S>(S state, Func<ImHashMapEntry<K, V>, int, S, S> reduce, ImHashMap<K, V>[] parentsStack = null)
    {
      if (Height == 1 && Entry is ImHashMapConflicts<K, V> == false)
        return reduce(Entry, 0, state);

      if (Height != 0)
      {
        parentsStack = parentsStack ?? new ImHashMap<K, V>[Height];
        var index = 0;
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parentsStack[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parentsStack[parentCount--];

            if (!(node.Entry is ImHashMapConflicts<K, V> conflicts))
              state = reduce(node.Entry, index++, state);
            else
            {
              var conflictData = conflicts.Conflicts;
              for (var i = 0; i < conflictData.Length; i++)
                state = reduce(conflictData[i], index++, state);
            }

            node = node.Right;
          }
        }
      }

      return state;
    }

    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public S Visit<S>(S state, Action<ImHashMapEntry<K, V>, S> effect, ImHashMap<K, V>[] parentsStack = null)
    {
      if (Height == 1 && Entry is ImHashMapConflicts<K, V> == false)
      {
        effect(Entry, state);
      }
      else if (Height != 0)
      {
        parentsStack = parentsStack ?? new ImHashMap<K, V>[Height];
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parentsStack[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parentsStack[parentCount--];

            if (!(node.Entry is ImHashMapConflicts<K, V> conflicts))
              effect(node.Entry, state);
            else
            {
              var conflict = conflicts.Conflicts;
              for (var i = 0; i < conflict.Length; i++)
                effect(conflict[i], state);
            }

            node = node.Right;
          }
        }
      }

      return state;
    }

    private static void Push(ImHashMapEntry<K, V> entry, ref KeysAndValues<K, V> keysAndValues)
    {
      var n = keysAndValues.Count;
      GrowingList.Push(ref keysAndValues.Keys, n, entry.Key);
      GrowingList.Push(ref keysAndValues.Values, n, entry.Value);
      keysAndValues.Count = n + 1;
    }

    /// <summary>Split the map into the keys and values in the data-oriented way (SOA - a structure of arrays instead of array of structures), 
    /// producing the shapeless homogenous arrays of keys and values instead of heterogenous pairs of specific shape which are harder to compose and adapt to the required API.
    /// The passed `keysAndValues` may already contain the data, the new keys and values will be appended to the same arrays, enabling the concat operation
    /// using the already pre-allocated space.</summary>
    public int ToKeysAndValues<S>(S state, Func<ImHashMapEntry<K, V>, S, bool> condition, ref KeysAndValues<K, V> keysAndValues, ImHashMap<K, V>[] parentsStack = null)
    {
      var count = keysAndValues.Count;
      var entry = Entry;
      if (Height == 1 && entry is ImHashMapConflicts<K, V> == false)
      {
        if (condition(entry, state))
          Push(entry, ref keysAndValues);
      }
      else if (Height != 0)
      {
        parentsStack = parentsStack ?? new ImHashMap<K, V>[Height];
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parentsStack[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parentsStack[parentCount--];
            entry = node.Entry;
            if (!(entry is ImHashMapConflicts<K, V> conflicts))
            {
              if (condition(entry, state))
                Push(entry, ref keysAndValues);
            }
            else
            {
              var conflict = conflicts.Conflicts;
              for (var i = 0; i < conflict.Length; i++)
              {
                entry = conflict[i];
                if (condition(entry, state))
                  Push(entry, ref keysAndValues);
              }
            }

            node = node.Right;
          }
        }
      }

      return keysAndValues.Count - count;
    }


    /// <summary>
    /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
    /// The only difference is using fixed size array instead of stack for speed-up.
    /// Note: By passing <paramref name="parentsStack"/> you may reuse the stack array between different method calls,
    /// but it should be at least <see cref="ImHashMap{K,V}.Height"/> length. The contents of array are not important.
    /// </summary>
    public void Visit(Action<ImHashMapEntry<K, V>> effect, ImHashMap<K, V>[] parentsStack = null)
    {
      if (Height == 1 && Entry is ImHashMapConflicts<K, V> == false)
        effect(Entry);
      else if (Height != 0)
      {
        parentsStack = parentsStack ?? new ImHashMap<K, V>[Height];
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parentsStack[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parentsStack[parentCount--];

            if (!(node.Entry is ImHashMapConflicts<K, V> conflicts))
              effect(node.Entry);
            else
            {
              var conflict = conflicts.Conflicts;
              for (var i = 0; i < conflict.Length; i++)
                effect(conflict[i]);
            }

            node = node.Right;
          }
        }
      }
    }

    /// <summary> Finds the first entry matching the condition, returns `null` if not found </summary>
    public ImHashMapEntry<K, V> FindFirstOrDefault(Func<ImHashMapEntry<K, V>, bool> condition, ImHashMap<K, V>[] parentsStack = null)
    {
      if (Height == 1 && Entry is ImHashMapConflicts<K, V> == false)
      {
        if (condition(Entry))
          return Entry;
      }
      else if (Height != 0)
      {
        parentsStack = parentsStack ?? new ImHashMap<K, V>[Height];
        var node = this;
        var parentCount = -1;
        while (node.Height != 0 || parentCount != -1)
        {
          if (node.Height != 0)
          {
            parentsStack[++parentCount] = node;
            node = node.Left;
          }
          else
          {
            node = parentsStack[parentCount--];

            if (!(node.Entry is ImHashMapConflicts<K, V> conflicts))
            {
              if (condition(node.Entry))
                return node.Entry;
            }
            else
            {
              var conflictedEntries = conflicts.Conflicts;
              for (var i = 0; i < conflictedEntries.Length; i++)
                if (condition(conflictedEntries[i]))
                  return conflictedEntries[i];
            }

            node = node.Right;
          }
        }
      }

      return null;
    }

    /// Removes or updates value for specified key, or does nothing if the key is not found (returns the unchanged map)
    /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx
    public ImHashMap<K, V> Remove(int hash, K key) =>
        RemoveImpl(hash, key);

    /// Removes or updates value for specified key, or does nothing if the key is not found (returns the unchanged map)
    /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx
    [MethodImpl((MethodImplOptions)256)]
    public ImHashMap<K, V> Remove(K key) =>
        RemoveImpl(key.GetHashCode(), key);

    private ImHashMap<K, V> RemoveImpl(int hash, K key, bool ignoreKey = false)
    {
      if (Height == 0)
        return this;

      ImHashMap<K, V> result;
      if (hash == Hash) // found node
      {
        if (ignoreKey || Equals(Key, key))
        {
          if (Height == 1) // remove node
            return Empty;

          if (Right.IsEmpty)
            result = Left;
          else if (Left.IsEmpty)
            result = Right;
          else
          {
            // we have two children, so remove the next highest node and replace this node with it.
            var next = Right;
            while (!next.Left.IsEmpty)
              next = next.Left;
            result = new ImHashMap<K, V>(next.Entry, Left, Right.RemoveImpl(next.Hash, default, ignoreKey: true));
          }
        }
        else if (Entry is ImHashMapConflicts<K, V> conflictsData)
          return TryRemoveConflicted(conflictsData, hash, key);
        else
          return this; // if key is not matching and no conflicts to lookup - just return
      }
      else
        result = hash < Hash
            ? Balance(Entry, Left.RemoveImpl(hash, key, ignoreKey), Right)
            : Balance(Entry, Left, Right.RemoveImpl(hash, key, ignoreKey));

      return result;
    }

    /// <summary> Searches for the key in the conflicts and returns true if found </summary>
    public bool ContainsConflictedData(K key)
    {
      if (Conflicts != null)
      {
        var conflicts = Conflicts;
        for (var i = 0; i < conflicts.Length; ++i)
          if (key.Equals(conflicts[i].Key))
            return true;
      }
      return false;
    }

    /// <summary> Searches for the key in the node conflicts </summary>
    public ImHashMapEntry<K, V> GetConflictedEntryOrDefault(K key)
    {
      if (Conflicts != null)
      {
        var conflicts = Conflicts;
        for (var i = 0; i < conflicts.Length; ++i)
          if (key.Equals(conflicts[i].Key))
            return conflicts[i];
      }
      return null;
    }

    /// Searches for the key in the node conflicts
    public V GetConflictedValueOrDefault(K key, V defaultValue)
    {
      if (Conflicts != null)
      {
        var conflicts = Conflicts;
        for (var i = 0; i < conflicts.Length; ++i)
          if (key.Equals(conflicts[i].Key))
            return conflicts[i].Value;
      }
      return defaultValue;
    }

    /// Searches for the key in the node conflicts
    public bool TryFindConflictedValue(K key, out V value)
    {
      if (Conflicts != null)
      {
        var conflicts = Conflicts;
        for (var i = 0; i < conflicts.Length; ++i)
          if (Equals(conflicts[i].Key, key))
          {
            value = conflicts[i].Value;
            return true;
          }
      }
      value = default;
      return false;
    }

    // todo: implement in terms of BalanceNewLeftTree | BalanceNewRightTree
    private static ImHashMap<K, V> Balance(ImHashMapEntry<K, V> entry, ImHashMap<K, V> left, ImHashMap<K, V> right)
    {
      var delta = left.Height - right.Height;
      if (delta > 1) // left is longer by 2, rotate left
      {
        var leftLeft = left.Left;
        var leftRight = left.Right;
        if (leftRight.Height > leftLeft.Height)
        {
          // double rotation:
          //      5     =>     5     =>     4
          //   2     6      4     6      2     5
          // 1   4        2   3        1   3     6
          //    3        1
          return new ImHashMap<K, V>(leftRight.Entry,
              new ImHashMap<K, V>(left.Entry, leftLeft, leftRight.Left),
              new ImHashMap<K, V>(entry, leftRight.Right, right));
        }

        // one rotation:
        //      5     =>     2
        //   2     6      1     5
        // 1   4              4   6
        return new ImHashMap<K, V>(left.Entry,
            leftLeft, new ImHashMap<K, V>(entry, leftRight, right));
      }

      if (delta < -1)
      {
        var rightLeft = right.Left;
        var rightRight = right.Right;
        return rightLeft.Height > rightRight.Height
            ? new ImHashMap<K, V>(rightLeft.Entry,
                new ImHashMap<K, V>(entry, left, rightLeft.Left),
                new ImHashMap<K, V>(right.Entry, rightLeft.Right, rightRight))
            : new ImHashMap<K, V>(right.Entry, new ImHashMap<K, V>(entry, left, rightLeft), rightRight);
      }

      return new ImHashMap<K, V>(entry, left, right);
    }

    private ImHashMap<K, V> TryRemoveConflicted(ImHashMapConflicts<K, V> conflictsData, int hash, K key)
    {
      var conflicts = conflictsData.Conflicts;
      var index = conflicts.Length - 1;
      while (index != -1 && !conflicts[index].Key.Equals(key)) --index;
      if (index == -1) // key is not found in conflicts - just return
        return this;

      // we removing the one from the 2 items, so we can reference the remaining item directly from the map node 
      if (conflicts.Length == 2)
        return new ImHashMap<K, V>(index == 0 ? conflicts[1] : conflicts[0], Left, Right, Height);

      // copy all except the `index`ed data into shrinked conflicts
      var shrinkedConflicts = new ImHashMapEntry<K, V>[conflicts.Length - 1];
      var newIndex = 0;
      for (var i = 0; i < conflicts.Length; ++i)
        if (i != index)
          shrinkedConflicts[newIndex++] = conflicts[i];
      return new ImHashMap<K, V>(new ImHashMapConflicts<K, V>(hash, shrinkedConflicts), Left, Right, Height);
    }
  }

  /// ImHashMap methods for faster performance
  public static class ImHashMap
  {
    internal static V IgnoreKey<K, V>(this Update<V> update, K _, V oldValue, V newValue) => update(oldValue, newValue);

    /// <summary> Looks for key in a tree and returns `true` if found. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool Contains<K, V>(this ImHashMap<K, V> map, int hash, K key)
    {
      while (map.Height != 0 && map.Hash != hash)
        map = hash < map.Hash ? map.Left : map.Right;
      return map.Height != 0 && (key.Equals(map.Key) || map.ContainsConflictedData(key));
    }

    /// <summary> Looks for key in a tree and returns `true` if found. </summary>
    [MethodImpl((MethodImplOptions)256)]
    public static bool Contains<K, V>(this ImHashMap<K, V> map, K key) =>
        map.Height != 0 && map.Contains(key.GetHashCode(), key);

    /// Looks for key in a tree and returns the Data object if found or `null` otherwise.
    [MethodImpl((MethodImplOptions)256)]
    public static ImHashMapEntry<K, V> GetEntryOrDefault<K, V>(this ImHashMap<K, V> map, int hash, K key)
    {
      while (map.Height != 0 && map.Hash != hash)
        map = hash < map.Hash ? map.Left : map.Right;

      return map.Height == 0 ? null :
          key.Equals(map.Key) ? map.Entry :
          map.GetConflictedEntryOrDefault(key);
    }

    /// <summary> Looks for key in a tree and returns the Data object if found or `null` otherwise. </summary> 
    [MethodImpl((MethodImplOptions)256)]
    public static ImHashMapEntry<K, V> GetEntryOrDefault<K, V>(this ImHashMap<K, V> map, K key)
    {
      if (map.Height == 0)
        return null;

      var hash = key.GetHashCode();

      while (map.Hash != hash)
      {
        map = hash < map.Hash ? map.Left : map.Right;
        if (map.Height == 0)
          return null;
      }

      return key.Equals(map.Key) ? map.Entry : map.GetConflictedEntryOrDefault(key);
    }

    /// Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.
    [MethodImpl((MethodImplOptions)256)]
    public static V GetValueOrDefault<K, V>(this ImHashMap<K, V> map, K key, V defaultValue = default)
    {
      if (map.Height == 0)
        return defaultValue;

      var hash = key.GetHashCode();

      while (map.Hash != hash)
      {
        map = hash < map.Hash ? map.Left : map.Right;
        if (map.Height == 0)
          return defaultValue;
      }

      return key.Equals(map.Key) ? map.Value : map.GetConflictedValueOrDefault(key, defaultValue);
    }

    /// Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.
    [MethodImpl((MethodImplOptions)256)]
    public static V GetValueOrDefault<K, V>(this ImHashMap<K, V> map, int hash, K key, V defaultValue = default)
    {
      if (map.Height == 0)
        return defaultValue;

      while (map.Hash != hash)
      {
        map = hash < map.Hash ? map.Left : map.Right;
        if (map.Height == 0)
          return defaultValue;
      }

      return key.Equals(map.Key) ? map.Value : map.GetConflictedValueOrDefault(key, defaultValue);
    }

    /// Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.
    [MethodImpl((MethodImplOptions)256)]
    public static V GetValueOrDefault<V>(this ImHashMap<Type, V> map, Type key, V defaultValue = default)
    {
      if (map.Height == 0)
        return defaultValue;

      var hash = RuntimeHelpers.GetHashCode(key);
      while (hash != map.Hash)
      {
        map = hash < map.Hash ? map.Left : map.Right;
        if (map.Height == 0)
          return defaultValue;
      }

      // we don't need to check `Height != 0` again cause in that case `key` will be `null` and `ReferenceEquals` will fail
      return ReferenceEquals(key, map.Key) ? map.Value : map.GetConflictedValueOrDefault(key, defaultValue);
    }

    /// Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.
    [MethodImpl((MethodImplOptions)256)]
    public static V GetValueOrDefault<V>(this ImHashMap<Type, V> map, int hash, Type key, V defaultValue = default)
    {
      if (map.Height == 0)
        return defaultValue;

      while (hash != map.Hash)
      {
        map = hash < map.Hash ? map.Left : map.Right;
        if (map.Height == 0)
          return defaultValue;
      }

      // we don't need to check `Height != 0` again cause in that case `key` will be `null` and `ReferenceEquals` will fail
      return ReferenceEquals(key, map.Key) ? map.Value : map.GetConflictedValueOrDefault(key, defaultValue);
    }

    /// Returns true if key is found and sets the value.
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFind<K, V>(this ImHashMap<K, V> map, K key, out V value)
    {
      if (map.Height != 0)
      {
        var hash = key.GetHashCode();

        while (hash != map.Hash && map.Height != 0)
          map = hash < map.Hash ? map.Left : map.Right;

        if (map.Height != 0)
        {
          if (key.Equals(map.Key))
          {
            value = map.Value;
            return true;
          }

          return map.TryFindConflictedValue(key, out value);
        }
      }

      value = default;
      return false;
    }

    /// Returns true if key is found and sets the value.
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFind<K, V>(this ImHashMap<K, V> map, int hash, K key, out V value)
    {
      if (map.Height != 0)
      {
        while (hash != map.Hash && map.Height != 0)
          map = hash < map.Hash ? map.Left : map.Right;

        if (map.Height != 0)
        {
          if (key.Equals(map.Key))
          {
            value = map.Value;
            return true;
          }

          return map.TryFindConflictedValue(key, out value);
        }
      }

      value = default;
      return false;
    }

    /// Returns true if key is found and the result value.
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFind<V>(this ImHashMap<Type, V> map, Type key, out V value)
    {
      if (map.Height != 0)
      {
        var hash = RuntimeHelpers.GetHashCode(key);
        while (hash != map.Hash && map.Height != 0)
          map = hash < map.Hash ? map.Left : map.Right;

        if (map.Height != 0)
        {
          // assign to `var data = ...`
          if (ReferenceEquals(key, map.Key))
          {
            value = map.Value;
            return true;
          }

          return map.TryFindConflictedValue(key, out value);
        }
      }

      value = default;
      return false;
    }

    /// Returns true if hash and key are found and the result value, or the false otherwise
    [MethodImpl((MethodImplOptions)256)]
    public static bool TryFind<V>(this ImHashMap<Type, V> map, int hash, Type key, out V value)
    {
      if (map.Height != 0)
      {
        while (hash != map.Hash && map.Height != 0)
          map = hash < map.Hash ? map.Left : map.Right;

        if (map.Height != 0)
        {
          if (ReferenceEquals(key, map.Key))
          {
            value = map.Value;
            return true;
          }

          return map.TryFindConflictedValue(key, out value);
        }
      }

      value = default;
      return false;
    }

    /// <summary>Uses `RuntimeHelpers.GetHashCode()`</summary>
    public static ImHashMap<Type, V> AddOrUpdate<V>(this ImHashMap<Type, V> map, Type key, V value) =>
        map.AddOrUpdate(RuntimeHelpers.GetHashCode(key), key, value);
  }

  /// The array of ImHashMap slots where the key first bits are used for FAST slot location
  /// and the slot is the reference to ImHashMap that can be swapped with its updated value
  public static class ImHashMapSlots
  {
    /// Default number of slots
    public const int SLOT_COUNT_POWER_OF_TWO = 32;

    /// The default mask to partition the key to the target slot
    public const int HASH_MASK_TO_FIND_SLOT = SLOT_COUNT_POWER_OF_TWO - 1;

    /// Creates the array with the empty slots
    [MethodImpl((MethodImplOptions)256)]
    public static ImHashMap<K, V>[] CreateWithEmpty<K, V>(int slotCountPowerOfTwo = SLOT_COUNT_POWER_OF_TWO)
    {
      var slots = new ImHashMap<K, V>[slotCountPowerOfTwo];
      for (var i = 0; i < slots.Length; ++i)
        slots[i] = ImHashMap<K, V>.Empty;
      return slots;
    }

    /// Returns a new tree with added or updated value for specified key.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrUpdate<K, V>(this ImHashMap<K, V>[] slots, int hash, K key, V value, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[hash & hashMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.AddOrUpdate(hash, key, value), copy) != copy)
        RefAddOrUpdateSlot(ref slot, hash, key, value);
    }

    /// Returns a new tree with added or updated value for specified key.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrUpdate<K, V>(this ImHashMap<K, V>[] slots, K key, V value, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT) =>
        slots.AddOrUpdate(key.GetHashCode(), key, value, hashMaskToFindSlot);

    /// Updates the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefAddOrUpdateSlot<K, V>(ref ImHashMap<K, V> slot, int hash, K key, V value) =>
        Ref.Swap(ref slot, hash, key, value, (x, h, k, v) => x.AddOrUpdate(h, k, v));

    /// Updates the value with help of `updateValue` function
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrUpdate<K, V>(this ImHashMap<K, V>[] slots, int hash, K key, V value, Update<K, V> update, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[hash & hashMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.AddOrUpdate(hash, key, value, update), copy) != copy)
        RefAddOrUpdateSlot(ref slot, hash, key, value, update);
    }

    /// Updates the value with help of `updateValue` function
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrUpdate<K, V>(this ImHashMap<K, V>[] slots, K key, V value, Update<K, V> updateValue, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT) =>
        slots.AddOrUpdate(key.GetHashCode(), key, value, updateValue, hashMaskToFindSlot);

    /// Update the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefAddOrUpdateSlot<K, V>(ref ImHashMap<K, V> slot, int hash, K key, V value, Update<K, V> update) =>
        Ref.Swap(ref slot, hash, key, value, (x, h, k, v) => x.AddOrUpdate(h, k, v, update));

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrKeep<K, V>(this ImHashMap<K, V>[] slots, int hash, K key, V value, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[hash & hashMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.AddOrKeep(hash, key, value), copy) != copy)
        RefAddOrKeepSlot(ref slot, hash, key, value);
    }

    /// Adds a new value for the specified key or keeps the existing map if the key is already in the map.
    [MethodImpl((MethodImplOptions)256)]
    public static void AddOrKeep<K, V>(this ImHashMap<K, V>[] slots, K key, V value, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT) =>
        slots.AddOrKeep(key.GetHashCode(), key, value, hashMaskToFindSlot);

    /// Update the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefAddOrKeepSlot<K, V>(ref ImHashMap<K, V> slot, int hash, K key, V value) =>
        Ref.Swap(ref slot, hash, key, value, (s, h, k, v) => s.AddOrKeep(h, k, v));

    /// Updates the specified slot or does not change it
    [MethodImpl((MethodImplOptions)256)]
    public static void Update<K, V>(this ImHashMap<K, V>[] slots, int hash, K key, V value, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT)
    {
      ref var slot = ref slots[hash & hashMaskToFindSlot];
      var copy = slot;
      if (Interlocked.CompareExchange(ref slot, copy.Update(hash, key, value), copy) != copy)
        RefUpdateSlot(ref slot, hash, key, value);
    }

    /// Updates the specified slot or does not change it
    [MethodImpl((MethodImplOptions)256)]
    public static void Update<K, V>(this ImHashMap<K, V>[] slots, K key, V value, int hashMaskToFindSlot = HASH_MASK_TO_FIND_SLOT) =>
        slots.Update(key.GetHashCode(), key, value, hashMaskToFindSlot);

    /// Update the ref to the slot with the new version - retry if the someone changed the slot in between
    public static void RefUpdateSlot<K, V>(ref ImHashMap<K, V> slot, int hash, K key, V value) =>
        Ref.Swap(ref slot, key, value, (s, k, v) => s.Update(k, v));

    /// Returns all map tree nodes without the order
    public static S Fold<K, V, S>(this ImHashMap<K, V>[] slots, S state, Func<ImHashMapEntry<K, V>, S, S> reduce)
    {
      var parentStack = ArrayTools.Empty<ImHashMap<K, V>>();
      for (var s = 0; s < slots.Length; s++)
      {
        var map = slots[s];
        var height = map.Height;
        if (height != 0)
        {
          if (height > 1 && parentStack.Length < height)
            parentStack = new ImHashMap<K, V>[height];
          state = map.Fold(state, reduce, parentStack);
        }
      }

      return state;
    }
  }
}
