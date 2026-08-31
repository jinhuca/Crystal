using System;

namespace Crystal.Controls.Demo.Support;

/// <summary>
/// Produces a smoothly wandering value within [<paramref name="min"/>, <paramref name="max"/>]:
/// each call to <see cref="Next"/> nudges the previous value by a small random step and clamps
/// it back into range, so a demo tile fed from this looks like plausible sensor data instead of
/// jumping randomly between extremes every tick.
/// </summary>
internal sealed class RandomWalk {
  private readonly Random _random;
  private readonly double _min;
  private readonly double _max;
  private readonly double _maxStep;
  private double _value;

  /// <param name="min">Lower clamp bound.</param>
  /// <param name="max">Upper clamp bound.</param>
  /// <param name="start">Initial value. Defaults to the midpoint of [min, max].</param>
  /// <param name="maxStep">Largest possible change per <see cref="Next"/> call. Defaults to 8% of the range.</param>
  /// <param name="seed">Optional RNG seed, so a demo's tiles can each move independently but reproducibly across runs.</param>
  public RandomWalk(double min, double max, double? start = null, double maxStep = 0, int? seed = null) {
    _random = seed.HasValue ? new Random(seed.Value) : new Random();
    _min = min;
    _max = max;
    _maxStep = maxStep > 0 ? maxStep : (max - min) * 0.08;
    _value = start ?? (min + max) / 2;
  }

  /// <summary>Advances the walk by one step and returns the new value.</summary>
  public double Next() {
    double step = (_random.NextDouble() * 2 - 1) * _maxStep;
    _value = Math.Clamp(_value + step, _min, _max);
    return _value;
  }
}
