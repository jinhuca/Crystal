using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// Tags an <see cref="IPerformanceGraph"/> with a stable string id so an external consumer (the shell's
/// graph-settings feature) can find it and apply the user's saved appearance. The control library does
/// not interpret the id itself — it only keeps a live registry of tagged graphs and raises
/// <see cref="GraphRegistered"/> as they appear. That keeps the consumer decoupled from the async,
/// tile-by-tile way dashboard views are realized: a graph tagged in XAML announces itself the moment
/// its id is set, and the consumer can also sweep <see cref="LiveGraphs"/> for anything already up.
/// <para>
/// Typed to the <see cref="IPerformanceGraph"/> interface rather than a concrete control, so any of
/// the graph controls (<see cref="PerformanceGraph"/>, <see cref="PerformanceGraphLite"/>,
/// <see cref="PerformanceGraphMultipleDS"/>) can opt in the same way.
/// </para>
/// </summary>
public static class GraphIdentity {
  /// <summary>Identifies the attached <c>Id</c> property.</summary>
  public static readonly DependencyProperty IdProperty =
      DependencyProperty.RegisterAttached("Id", typeof(string), typeof(GraphIdentity),
          new PropertyMetadata(null, OnIdChanged));

  public static string? GetId(DependencyObject obj) => (string?)obj.GetValue(IdProperty);
  public static void SetId(DependencyObject obj, string? value) => obj.SetValue(IdProperty, value);

  // Tagged graphs held weakly, so a graph in a closed detail window (or a replaced dashboard view)
  // can still be collected. Dead entries are pruned on each registration and enumeration.
  private static readonly List<WeakReference<IPerformanceGraph>> Registered = new();

  /// <summary>Raised when a graph is tagged with a non-empty id (on the thread that set the id — the
  /// UI dispatcher during view load).</summary>
  public static event Action<IPerformanceGraph>? GraphRegistered;

  private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not IPerformanceGraph graph) return;
    if (e.NewValue is not string id || string.IsNullOrEmpty(id)) return;

    Prune();
    Registered.Add(new WeakReference<IPerformanceGraph>(graph));
    GraphRegistered?.Invoke(graph);
  }

  /// <summary>A snapshot of the currently-live tagged graphs.</summary>
  public static IReadOnlyList<IPerformanceGraph> LiveGraphs() {
    Prune();
    var live = new List<IPerformanceGraph>(Registered.Count);
    foreach (var weak in Registered)
      if (weak.TryGetTarget(out var graph)) live.Add(graph);
    return live;
  }

  private static void Prune() => Registered.RemoveAll(w => !w.TryGetTarget(out _));
}
