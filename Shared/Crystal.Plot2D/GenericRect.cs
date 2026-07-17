using System;
using System.Diagnostics;

namespace Crystal.Plot2D;

/// <summary>
///   Represents simple rectangle with corner coordinates of specified types.
/// </summary>
/// <typeparam name="THorizontal">
///   The horizontal values type.
/// </typeparam>
/// <typeparam name="TVertical">
///   The vertical values type.
/// </typeparam>
public readonly struct GenericRect<THorizontal, TVertical> : IEquatable<GenericRect<THorizontal, TVertical>> {
  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly THorizontal _xMin;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly TVertical _yMin;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly THorizontal _xMax;

  [DebuggerBrowsable(state: DebuggerBrowsableState.Never)]
  private readonly TVertical _yMax;

  /// <summary>
  ///   Initializes a new instance of the <see cref="GenericRect&lt;THorizontal, TVertical&gt;"/> struct.
  /// </summary>
  /// <param name="xMin">The minimal x value.</param>
  /// <param name="yMin">The minimal y value.</param>
  /// <param name="xMax">The maximal x value.</param>
  /// <param name="yMax">The maximal y value.</param>
  public GenericRect(THorizontal xMin, TVertical yMin, THorizontal xMax, TVertical yMax) {
    _xMin = xMin;
    _xMax = xMax;
    _yMin = yMin;
    _yMax = yMax;
  }

  /// <summary>
  ///   Gets the minimal X value.
  /// </summary>
  /// <value>The X min.</value>
  public THorizontal XMin => _xMin;

  /// <summary>
  ///   Gets the minimal Y value.
  /// </summary>
  /// <value>The Y min.</value>
  public TVertical YMin => _yMin;

  /// <summary>
  ///   Gets the maximal X value.
  /// </summary>
  /// <value>The X max.</value>
  public THorizontal XMax => _xMax;

  /// <summary>
  ///   Gets the maximal Y value.
  /// </summary>
  /// <value>The Y max.</value>
  public TVertical YMax => _yMax;

  #region Object overrides

  /// <summary>
  /// Indicates whether this instance and a specified object are equal.
  /// </summary>
  /// <param name="obj">Another object to compare to.</param>
  /// <returns>
  /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
  /// </returns>
  public override bool Equals(object obj) {
    if(obj == null) {
      return false;
    }

    if(!(obj is GenericRect<THorizontal, TVertical>)) {
      return false;
    }

    var other = (GenericRect<THorizontal, TVertical>)obj;

    return Equals(other: other);
  }

  /// <summary>
  /// Returns the hash code for this instance.
  /// </summary>
  /// <returns>
  /// A 32-bit signed integer that is the hash code for this instance.
  /// </returns>
  public override int GetHashCode() => _xMin.GetHashCode() ^ _xMax.GetHashCode() ^ _yMin.GetHashCode() ^ _yMax.GetHashCode();

  /// <summary>
  /// Returns the fully qualified type name of this instance.
  /// </summary>
  /// <returns>
  /// A <see cref="T:System.String"/> containing a fully qualified type name.
  /// </returns>
  public override string ToString() => $"({_xMin},{_yMin}) - ({_xMax},{_yMax})";

  #endregion

  #region IEquatable<GenericRect<THorizontal,TVertical>> Members

  /// <summary>
  /// Indicates whether the current object is equal to another object of the same type.
  /// </summary>
  /// <param name="other">An object to compare with this object.</param>
  /// <returns>
  /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
  /// </returns>
  public bool Equals(GenericRect<THorizontal, TVertical> other) {
    return
      _xMin.Equals(obj: other._xMin) &&
      _xMax.Equals(obj: other._xMax) &&
      _yMin.Equals(obj: other._yMin) &&
      _yMax.Equals(obj: other._yMax);
  }

  #endregion

  /// <summary>
  /// Implements the operator ==.
  /// </summary>
  /// <param name="rect1">The rect1.</param>
  /// <param name="rect2">The rect2.</param>
  /// <returns>The result of the operator.</returns>
  public static bool operator ==(GenericRect<THorizontal, TVertical> rect1, GenericRect<THorizontal, TVertical> rect2) => rect1.Equals(other: rect2);

  /// <summary>
  /// Implements the operator !=.
  /// </summary>
  /// <param name="rect1">The rect1.</param>
  /// <param name="rect2">The rect2.</param>
  /// <returns>The result of the operator.</returns>
  public static bool operator !=(GenericRect<THorizontal, TVertical> rect1, GenericRect<THorizontal, TVertical> rect2) => !rect1.Equals(other: rect2);
}
