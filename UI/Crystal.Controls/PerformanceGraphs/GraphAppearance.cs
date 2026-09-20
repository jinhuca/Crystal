using System.ComponentModel;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// How every dashboard history graph draws its samples: a continuous filled line or a
/// dot-matrix gauge. Maps to a <see cref="PerformanceGraph"/>'s <see cref="DisplayMode"/>
/// (<see cref="DisplayMode.Line"/> / <see cref="DisplayMode.Dot"/>).
/// </summary>
public enum GraphRenderMode {
  /// <summary>
  /// Filled line — <see cref="PerformanceGraph"/> with <see cref="DisplayMode.Line"/>.
  /// </summary>
  Line,

  /// <summary>
  /// Dot-matrix gauge — <see cref="PerformanceGraph"/> with <see cref="DisplayMode.Dot"/>.
  /// </summary>
  Dot,
}

/// <summary>
/// The global render mode shared by every dashboard <see cref="PerformanceGraph"/>, shared
/// across the shell/module assembly boundary the same way <see cref="Meters.CoreBarAppearance"/>
/// is: each tile's graph binds its <see cref="PerformanceGraph.DisplayMode"/> to this singleton, and
/// the shell's title-bar Line/Dot toggle writes the user's choice here so a change takes effect on
/// every graph immediately and is reproduced on the next launch. Kept in the shared control library
/// because modules cannot reference the shell.
/// </summary>
public sealed class GraphAppearance : INotifyPropertyChanged {
  /// <summary>
  /// The single instance every dashboard <see cref="PerformanceGraph"/> binds to and the shell writes.
  /// </summary>
  public static GraphAppearance Current { get; } = new();

  /// <summary>
  /// The current render mode for all dashboard graphs. Defaults to <see cref="GraphRenderMode.Line"/>.
  /// </summary>
  private GraphRenderMode _mode = GraphRenderMode.Line;

  /// <summary>
  /// Line (filled line) or Dot (dot-matrix gauge) for all dashboard graphs.
  /// </summary>
  public GraphRenderMode Mode {
    get => _mode;
    set {
      if (_mode == value) return;
      _mode = value;
      PropertyChanged?.Invoke(this, ModeChangedArgs);
    }
  }

  /// <summary>
  /// The <see cref="PropertyChangedEventArgs"/> for <see cref="Mode"/> changes, cached to avoid allocations.
  /// </summary>
  private static readonly PropertyChangedEventArgs ModeChangedArgs = new(nameof(Mode));

  /// <summary>
  /// Raised when <see cref="Mode"/> changes, so every dashboard graph can re-render itself.
  /// </summary>
  public event PropertyChangedEventHandler? PropertyChanged;
}
