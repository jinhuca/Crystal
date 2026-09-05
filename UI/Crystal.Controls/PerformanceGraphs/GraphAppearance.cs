using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>How every dashboard history graph draws its samples: a continuous filled line
/// (a real <see cref="PerformanceGraph"/>) or a dot-matrix gauge (a real
/// <see cref="PerformanceGraphLite"/>).</summary>
public enum GraphRenderMode {
  /// <summary>Filled line — <see cref="PerformanceGraph"/> with <see cref="Kinds.GraphKind.Line"/>.</summary>
  Line,

  /// <summary>Dot-matrix gauge — <see cref="PerformanceGraphLite"/>.</summary>
  Dot,
}

/// <summary>
/// The global render mode shared by every <see cref="AdaptiveGraph"/> on the dashboard, shared
/// across the shell/module assembly boundary the same way <see cref="Meters.CoreBarAppearance"/>
/// is: each tile's graph binds to this singleton, and the shell's title-bar Line/Dot toggle writes
/// the user's choice here so a change takes effect on every graph immediately and is reproduced on
/// the next launch. Kept in the shared control library because modules cannot reference the shell.
/// </summary>
public sealed class GraphAppearance : INotifyPropertyChanged {
  /// <summary>The single instance every <see cref="AdaptiveGraph"/> binds to and the shell writes.</summary>
  public static GraphAppearance Current { get; } = new();

  private GraphRenderMode _mode = GraphRenderMode.Line;

  /// <summary>Line (filled line) or Dot (dot-matrix gauge) for all dashboard graphs.</summary>
  public GraphRenderMode Mode {
    get => _mode;
    set {
      if (_mode == value) return;
      _mode = value;
      PropertyChanged?.Invoke(this, ModeChangedArgs);
    }
  }

  private static readonly PropertyChangedEventArgs ModeChangedArgs = new(nameof(Mode));

  public event PropertyChangedEventHandler? PropertyChanged;
}
