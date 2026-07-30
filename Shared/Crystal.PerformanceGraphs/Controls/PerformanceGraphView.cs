using System.Windows;
using System.Windows.Controls;

namespace Crystal.PerformanceGraphs.Controls;

/// <summary>
/// Groups a labeled header, a <see cref="PerformanceGraph"/>, and a labeled footer into a
/// single templated control.
/// <para>
/// The header shows a <see cref="Title"/> (left) and the graph's <see cref="MaxValue"/> (right);
/// the footer shows <see cref="MaxTime"/> (left) and <see cref="MinTime"/> (right). The three
/// numeric properties are rendered through their companion format strings
/// (<see cref="MaxValueFormat"/>, <see cref="MaxTimeFormat"/>, <see cref="MinTimeFormat"/>), so a
/// value of 100 with format "{0}%" reads "100%", 3 with "{0}V" reads "3V", etc.
/// </para>
/// <para>
/// <see cref="MaxValue"/> and <see cref="MinValue"/> are pushed onto the wrapped
/// <see cref="PerformanceGraph"/> automatically, so binding them to a view model scales the plot
/// and updates the header label together. The inner graph is also exposed via <see cref="Graph"/>
/// for setting kind/theme and pushing samples with <see cref="PerformanceGraph.AddValue"/>.
/// </para>
/// </summary>
[TemplatePart(Name = PartGraph, Type = typeof(PerformanceGraph))]
public class PerformanceGraphView : Control {
  private const string PartGraph = "PART_Graph";

  static PerformanceGraphView() {
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(PerformanceGraphView),
        new FrameworkPropertyMetadata(typeof(PerformanceGraphView)));
  }

  /// <summary>Header title, e.g. "% Utilization" or "Voltage".</summary>
  public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata(string.Empty));

  public string Title {
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  /// <summary>Top of the graph's value scale; shown in the header. Drives <see cref="PerformanceGraph.MaxValue"/>.</summary>
  public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata(100.0, OnMaxValueChanged));

  public double MaxValue {
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
  }

  /// <summary>Composite format applied to <see cref="MaxValue"/> for the header label, e.g. "{0}%" or "{0}V".</summary>
  public static readonly DependencyProperty MaxValueFormatProperty =
      DependencyProperty.Register(nameof(MaxValueFormat), typeof(string), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata("{0}"));

  public string MaxValueFormat {
    get => (string)GetValue(MaxValueFormatProperty);
    set => SetValue(MaxValueFormatProperty, value);
  }

  /// <summary>Bottom of the graph's value scale. Drives <see cref="PerformanceGraph.MinValue"/>.</summary>
  public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata(0.0, OnMinValueChanged));

  public double MinValue {
    get => (double)GetValue(MinValueProperty);
    set => SetValue(MinValueProperty, value);
  }

  /// <summary>Footer max-time value (oldest sample edge), e.g. 60; shown at the footer's left.</summary>
  public static readonly DependencyProperty MaxTimeProperty =
      DependencyProperty.Register(nameof(MaxTime), typeof(double), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata(60.0));

  public double MaxTime {
    get => (double)GetValue(MaxTimeProperty);
    set => SetValue(MaxTimeProperty, value);
  }

  /// <summary>Composite format applied to <see cref="MaxTime"/>, e.g. "{0} seconds".</summary>
  public static readonly DependencyProperty MaxTimeFormatProperty =
      DependencyProperty.Register(nameof(MaxTimeFormat), typeof(string), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata("{0}"));

  public string MaxTimeFormat {
    get => (string)GetValue(MaxTimeFormatProperty);
    set => SetValue(MaxTimeFormatProperty, value);
  }

  /// <summary>Footer min-time value (newest sample edge), e.g. 0; shown at the footer's right.</summary>
  public static readonly DependencyProperty MinTimeProperty =
      DependencyProperty.Register(nameof(MinTime), typeof(double), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata(0.0));

  public double MinTime {
    get => (double)GetValue(MinTimeProperty);
    set => SetValue(MinTimeProperty, value);
  }

  /// <summary>Composite format applied to <see cref="MinTime"/>, e.g. "{0}" or "{0} s".</summary>
  public static readonly DependencyProperty MinTimeFormatProperty =
      DependencyProperty.Register(nameof(MinTimeFormat), typeof(string), typeof(PerformanceGraphView),
          new FrameworkPropertyMetadata("{0}"));

  public string MinTimeFormat {
    get => (string)GetValue(MinTimeFormatProperty);
    set => SetValue(MinTimeFormatProperty, value);
  }

  /// <summary>The wrapped graph, available once the template is applied.</summary>
  public PerformanceGraph? Graph { get; private set; }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    Graph = GetTemplateChild(PartGraph) as PerformanceGraph;
    if (Graph != null) {
      // The scale DPs may have been set (or bound) before the template produced the graph;
      // push their current values so the plot starts in sync.
      Graph.MaxValue = MaxValue;
      Graph.MinValue = MinValue;
    }
  }

  private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (((PerformanceGraphView)d).Graph is { } graph) graph.MaxValue = (double)e.NewValue;
  }

  private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (((PerformanceGraphView)d).Graph is { } graph) graph.MinValue = (double)e.NewValue;
  }
}
