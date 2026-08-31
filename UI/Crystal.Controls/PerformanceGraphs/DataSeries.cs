using Crystal.Controls.PerformanceGraphs.Buffers;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Styles;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs;

/// <summary>
/// One named line on a <see cref="PerformanceGraphMultipleDS"/> - its own color, thickness, and
/// optional fill, plotting its own buffered data against the parent graph's shared
/// MinValue/MaxValue/Capacity axes. Declare these under
/// <see cref="PerformanceGraphMultipleDS.Series"/> directly in XAML, or build the collection in a
/// view-model and assign/bind it to <see cref="PerformanceGraphMultipleDS.Series"/> itself.
/// </summary>
/// <remarks>
/// A <see cref="DataSeries"/> only has buffered data - and only reacts to
/// <see cref="ValuesSource"/> - once it's actually attached to a graph (added to that graph's
/// <see cref="PerformanceGraphMultipleDS.Series"/> collection), since <see cref="Capacity"/>
/// (needed to size its buffer) belongs to the graph, not the series. Setting properties on an
/// unattached instance is fine - they just take effect once it's attached, and <see cref="AddValue"/>/
/// <see cref="ClearValues"/> are safe no-ops on an unattached instance rather than throwing.
/// <para>
/// <b><see cref="ValuesSource"/> and XAML <c>{Binding}</c>.</b> <see cref="DataSeries"/> derives
/// from plain <see cref="DependencyObject"/>, not <see cref="FrameworkElement"/> - unlike
/// <see cref="PerformanceGraphLite"/>, it has no <c>DataContext</c> of its own and doesn't
/// automatically inherit one through a plain <see cref="ObservableCollection{T}"/> assigned to
/// <see cref="PerformanceGraphMultipleDS.Series"/> (that inheritance-context propagation is a
/// <see cref="Freezable"/>/<c>FreezableCollection</c>-specific mechanism this class doesn't use,
/// to avoid the added complexity of <see cref="Freezable"/>'s clone/instantiation ceremony for a
/// first version of this control). A XAML <c>ValuesSource="{Binding SomeCollection}"</c> on a
/// <see cref="DataSeries"/> is consequently likely to resolve against no DataContext at all.
/// Set <see cref="ValuesSource"/> from code-behind instead (a plain property assignment, e.g.
/// <c>mySeries.ValuesSource = someCollection;</c>), or use
/// <see cref="System.Windows.Data.BindingOperations.SetBinding(DependencyObject, DependencyProperty, System.Windows.Data.BindingBase)"/>
/// with an explicit <c>Binding.Source</c> that doesn't rely on inherited DataContext.
/// </para>
/// </remarks>
public sealed class DataSeries : DependencyObject {
  /// <summary>Identifies the <see cref="Name"/> dependency property.</summary>
  public static readonly DependencyProperty NameProperty =
      DependencyProperty.Register(nameof(Name), typeof(string), typeof(DataSeries));

  /// <summary>A label for this series - not rendered by the graph itself (there is no built-in
  /// legend), but useful for a legend or tooltip built alongside it.</summary>
  public string? Name {
    get => (string?)GetValue(NameProperty);
    set => SetValue(NameProperty, value);
  }

  /// <summary>Identifies the <see cref="LineBrush"/> dependency property.</summary>
  public static readonly DependencyProperty LineBrushProperty =
      DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(DataSeries),
          new PropertyMetadata(Brushes.DeepSkyBlue, OnLineBrushChanged));

  /// <summary>Stroke color of this series' line. Defaults to DeepSkyBlue - set a distinct color
  /// per series so multiple lines on the same graph stay visually distinguishable.</summary>
  public Brush LineBrush {
    get => (Brush)GetValue(LineBrushProperty);
    set => SetValue(LineBrushProperty, value);
  }

  /// <summary>Identifies the <see cref="LineThickness"/> dependency property.</summary>
  public static readonly DependencyProperty LineThicknessProperty =
      DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(DataSeries),
          new PropertyMetadata(1.5, OnLineThicknessChanged));

  /// <summary>Stroke thickness of this series' line, in pixels.</summary>
  public double LineThickness {
    get => (double)GetValue(LineThicknessProperty);
    set => SetValue(LineThicknessProperty, value);
  }

  /// <summary>Identifies the <see cref="FillBrush"/> dependency property.</summary>
  public static readonly DependencyProperty FillBrushProperty =
      DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(DataSeries),
          new PropertyMetadata(null, OnFillBrushChanged));

  /// <summary>Fill brush painted under this series' line. Null (the default) draws a plain line -
  /// the usual choice once more than one or two series share a graph, since overlapping filled
  /// areas occlude each other (the same reasoning <see cref="PerformanceGraph.AddSeries"/>'s own
  /// optional fillBrush parameter documents).</summary>
  public Brush? FillBrush {
    get => (Brush?)GetValue(FillBrushProperty);
    set => SetValue(FillBrushProperty, value);
  }

  /// <summary>Identifies the <see cref="ValuesSource"/> dependency property.</summary>
  public static readonly DependencyProperty ValuesSourceProperty =
      DependencyProperty.Register(nameof(ValuesSource), typeof(ObservableCollection<double>), typeof(DataSeries),
          new PropertyMetadata(null, OnValuesSourceChanged));

  /// <summary>
  /// Binds this series' data to an <see cref="ObservableCollection{T}"/> of <see cref="double"/>,
  /// mirroring <see cref="PerformanceGraphLite.ValuesSource"/>'s own behavior exactly: assigning it
  /// clears this series' buffer and seeds it with the collection's current contents (once this
  /// series is attached to a graph - see the class remarks), then every subsequent
  /// <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> that
  /// carries new items appends them the same way <see cref="AddValue"/> does. A
  /// <see cref="NotifyCollectionChangedAction.Reset"/> re-seeds from the collection's post-reset
  /// contents; Remove/Replace/Move aren't translated into buffer edits beyond appending any
  /// NewItems they carry.
  /// <para>
  /// <b>Threading:</b> unlike <see cref="AddValue"/>, which accepts calls from a background thread
  /// and hops onto the owning graph's UI thread itself, <see cref="ObservableCollection{T}"/> is
  /// not safe to mutate from a background thread - only the thread that owns it may call
  /// Add/Clear on it.
  /// </para>
  /// </summary>
  public ObservableCollection<double>? ValuesSource {
    get => (ObservableCollection<double>?)GetValue(ValuesSourceProperty);
    set => SetValue(ValuesSourceProperty, value);
  }

  // Owned by whichever PerformanceGraphMultipleDS this series is currently attached to. Null until
  // Attach runs - lets the property-changed callbacks below tell "not attached yet" (defer to
  // Attach, which reads the then-current property values itself) from "attached, react now".
  internal CircularBuffer<double>? Buffer;
  internal readonly FilledLineRenderer Renderer = new();
  internal Pen ResolvedLinePen = Helpers.CreateFrozenPen(Brushes.DeepSkyBlue, 1.5);
  internal PerformanceGraphMultipleDS? Owner;

  private static void OnLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var series = (DataSeries)d;
    series.ResolvedLinePen = Helpers.CreateFrozenPen((Brush)e.NewValue, series.LineThickness);
    series.Owner?.RequestRender();
  }

  private static void OnLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var series = (DataSeries)d;
    series.ResolvedLinePen = Helpers.CreateFrozenPen(series.LineBrush, (double)e.NewValue);
    series.Owner?.RequestRender();
  }

  private static void OnFillBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      ((DataSeries)d).Owner?.RequestRender();

  private static void OnValuesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var series = (DataSeries)d;

    if (e.OldValue is ObservableCollection<double> oldSource)
      oldSource.CollectionChanged -= series.OnValuesSourceCollectionChanged;

    // Not attached to a graph yet - there's no Buffer to seed and no Capacity to size one with.
    // Attach() reads ValuesSource itself once this series actually joins a graph's Series
    // collection, so there's nothing more to do here until then.
    if (series.Buffer == null) return;

    series.Buffer.Clear();
    if (e.NewValue is ObservableCollection<double> newSource) {
      foreach (double value in newSource) series.Buffer.Add(value);
      newSource.CollectionChanged += series.OnValuesSourceCollectionChanged;
    }
    series.Owner?.RequestRender();
  }

  private void OnValuesSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    if (Buffer == null) return; // Detached mid-flight (e.g. removed from Series) - ignore stale events.

    if (e.Action == NotifyCollectionChangedAction.Reset) {
      Buffer.Clear();
      if (sender is ObservableCollection<double> source)
        foreach (double value in source) Buffer.Add(value);
    } else if (e.NewItems != null) {
      // Add, Replace, and Move all surface their new elements via NewItems - appending them
      // covers the live-append scenario this property exists for. A bare Remove carries no
      // NewItems and is a no-op here, same choice PerformanceGraphLite.ValuesSource makes.
      foreach (double value in e.NewItems) Buffer.Add(value);
    }

    Owner?.RequestRender();
  }

  /// <summary>Appends a new sample, dropping the oldest once the owning graph's
  /// <see cref="PerformanceGraphMultipleDS.Capacity"/> is exceeded. A safe no-op if this series
  /// isn't currently attached to a graph. O(1).</summary>
  public void AddValue(double value) {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(() => AddValue(value));
      return;
    }
    Buffer?.Add(value);
    Owner?.RequestRender();
  }

  /// <summary>Discards this series' buffered samples. A safe no-op if not currently attached.</summary>
  public void ClearValues() {
    if (!CheckAccess()) {
      Dispatcher.BeginInvoke(ClearValues);
      return;
    }
    Buffer?.Clear();
    Owner?.RequestRender();
  }

  // Called by PerformanceGraphMultipleDS when this series is added to its Series collection (or
  // when the whole collection is replaced and this series is part of the new one). Reads
  // ValuesSource (and every other property) as they stand right now, so it doesn't matter whether
  // those were set before or after this series was added to a Series collection.
  internal void Attach(PerformanceGraphMultipleDS owner, int capacity) {
    Owner = owner;
    Buffer = new CircularBuffer<double>(capacity);
    if (ValuesSource is { } source) {
      foreach (double value in source) Buffer.Add(value);
      source.CollectionChanged += OnValuesSourceCollectionChanged;
    }
  }

  // Called when this series is removed from a graph's Series collection (or the whole collection
  // is replaced), so a stray ValuesSource subscription doesn't keep this series reacting to a
  // collection it no longer plots anywhere.
  internal void Detach() {
    if (ValuesSource is { } source) source.CollectionChanged -= OnValuesSourceCollectionChanged;
    Buffer = null;
    Owner = null;
  }

  // Rebuilds this series' buffer at a new capacity, carrying over the most recent samples that
  // still fit - mirrors PerformanceGraph's own CopyMostRecent exactly. Called by the owning graph
  // when its Capacity changes. A no-op if this series isn't currently attached.
  internal void Resize(int newCapacity) {
    if (Buffer == null) return;
    var next = new CircularBuffer<double>(newCapacity);
    int start = Buffer.Count > newCapacity ? Buffer.Count - newCapacity : 0;
    for (int i = start; i < Buffer.Count; i++) next.Add(Buffer[i]);
    Buffer = next;
  }
}
