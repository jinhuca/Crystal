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
/// Typed to the <see cref="IPerformanceGraph"/> interface rather than a concrete control, so any
/// graph control implementing it (<see cref="PerformanceGraph"/>) can opt in the same way.
/// </para>
/// </summary>
public static class GraphIdentity {
  /// <summary>
  /// Identifies the attached <c>Id</c> property.
  /// </summary>
  public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached(
    "Id", 
    typeof(string), 
    typeof(GraphIdentity),
    new PropertyMetadata(null, OnIdChanged));

  /// <summary>
  /// Gets the string id attached to a graph. Returns null if none is set.
  /// </summary>
  /// <param name="obj">The graph for which to get the id.</param>
  /// <returns>The id, or null if none is set.</returns>
  public static string? GetId(DependencyObject obj) => (string?)obj.GetValue(IdProperty);

  /// <summary>
  /// Sets the string id attached to a graph. Setting a non-empty id registers the graph in the live registry 
  /// and raises <see cref="GraphRegistered"/> on the thread that set the id (the UI dispatcher during view load). 
  /// Setting null or empty unregisters it.
  /// </summary>
  /// <param name="obj">The graph for which to set the id.</param>
  /// <param name="value">The id to set, or null to unregister.</param>
  public static void SetId(DependencyObject obj, string? value) => obj.SetValue(IdProperty, value);

  /// <summary>
  /// Tagged graphs held weakly, so a graph in a closed detail window (or a replaced dashboard view)
  /// can still be collected. Dead entries are pruned on each registration and enumeration.
  /// </summary>
  private static readonly List<WeakReference<IPerformanceGraph>> Registered = new();

  /// <summary>
  /// Raised when a graph is tagged with a non-empty id (on the thread that set the id — the
  /// UI dispatcher during view load).
  /// </summary>
  public static event Action<IPerformanceGraph>? GraphRegistered;

  /// <summary>
  /// Called when the attached <c>Id</c> property changes. If the new value is a non-empty string, 
  /// registers the graph in the live registry and raises <see cref="GraphRegistered"/>. 
  /// If the new value is null or empty, unregisters it.
  /// </summary>
  /// <param name="d">d</param>
  /// <param name="e">e</param>
  private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not IPerformanceGraph graph) return;
    if (e.NewValue is not string id || string.IsNullOrEmpty(id)) return;

    Prune();
    Registered.Add(new WeakReference<IPerformanceGraph>(graph));
    GraphRegistered?.Invoke(graph);
  }

  /// <summary>
  /// A snapshot of the currently-live tagged graphs.
  /// </summary>
  public static IReadOnlyList<IPerformanceGraph> LiveGraphs() {
    Prune();
    var live = new List<IPerformanceGraph>(Registered.Count);
    foreach (var weak in Registered)
      if (weak.TryGetTarget(out var graph)) live.Add(graph);
    return live;
  }

  /// <summary>
  /// Removes any dead entries from the <see cref="Registered"/> list. 
  /// Called on each registration and enumeration.
  /// </summary>
  private static void Prune() => Registered.RemoveAll(w => !w.TryGetTarget(out _));
}
