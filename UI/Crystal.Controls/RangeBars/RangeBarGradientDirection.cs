namespace Crystal.Controls.RangeBars;

/// <summary>
/// Controls how the accent-color alpha gradient runs across a <see cref="RangeBar"/>'s scale
/// (see <see cref="RangeBar.AccentColor"/>).
/// </summary>
public enum RangeBarGradientDirection {
  /// <summary>Alpha rises from <see cref="RangeBar.LowestAlpha"/> at the min edge to
  /// <see cref="RangeBar.HighestAlpha"/> at the max edge.</summary>
  Ascending,

  /// <summary>Alpha falls from <see cref="RangeBar.HighestAlpha"/> at the min edge to
  /// <see cref="RangeBar.LowestAlpha"/> at the max edge.</summary>
  Descending
}
