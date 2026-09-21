using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crystal.Controls.Meters;

/// <summary>
/// The global look of the CPU per-core meter bars (<see cref="SegmentedBar"/>), shared across the
/// assembly boundary the way <see cref="Crystal.Controls.PerformanceGraphs.GraphIdentity"/> is: the CPU
/// module binds its core bars to this singleton, and the shell's graph-settings feature writes the
/// user's choice here so a change takes effect immediately and is reproduced on the next launch. Kept
/// in the shared control library because modules cannot reference the shell.
/// </summary>
public sealed class CoreBarAppearance : INotifyPropertyChanged {
  /// <summary>The single instance every core bar binds to and the shell writes.</summary>
  public static CoreBarAppearance Current { get; } = new();

  /// <summary>
  /// True to draw the core bars as a discrete LED-meter (segmented); false for a solid fill.
  /// </summary>
  private bool _segmented = true;

  /// <summary>
  /// True to paint every core bar a uniform muted grey; false to keep the per-metric colours.
  /// </summary>
  private bool _monochrome;

  /// <summary>
  /// True to draw the core bars as a discrete LED-meter (segmented); false for a solid fill.
  /// </summary>
  public bool Segmented {
    get => _segmented;
    set => Set(ref _segmented, value);
  }

  /// <summary>
  /// True to paint every core bar a uniform muted grey; false to keep the per-metric colours.
  /// </summary>
  public bool Monochrome {
    get => _monochrome;
    set => Set(ref _monochrome, value);
  }

  /// <summary>
  /// Occurs when a property value changes. Used to notify the UI of changes to <see cref="Segmented"/> or
  /// <see cref="Monochrome"/>.
  /// </summary>
  public event PropertyChangedEventHandler? PropertyChanged;

  /// <summary>
  /// Sets the field to the new value and raises <see cref="PropertyChanged"/> if it changed.
  /// </summary>
  /// <param name="field">The field to set.</param>
  /// <param name="value">The new value.</param>
  /// <param name="name">The name of the property.</param>
  private void Set(ref bool field, bool value, [CallerMemberName] string? name = null) {
    if (field == value) return;
    field = value;
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}
