using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;

namespace Crystal.Plot2D.DataSources.OneDimensional;

// todo I don't think that we should create data source which supports 
// suspending its DataChanged event - it is better to create
// collection with the same functionality - then it would be able to be used
// as a source in many data sources.
public class ObservableDataSource<T> : IPointDataSource {
  public ObservableDataSource() {
    Collection.CollectionChanged += OnCollectionChanged;

    // todo this is hack
    if(typeof(T) == typeof(Point)) {
      XyMapping = t => (Point)(object)t;
    }
  }

  public ObservableDataSource(IEnumerable<T> data) : this() {
    ArgumentNullException.ThrowIfNull(data);
    foreach(var item_ in data) {
      Collection.Add(item: item_);
    }
  }

  public void SuspendUpdate() => UpdatesEnabled = false;

  public void ResumeUpdate() {
    UpdatesEnabled = true;
    if(!CollectionChanged) return;
    CollectionChanged = false;
    RaiseDataChanged();
  }

  private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    if(UpdatesEnabled) {
      RaiseDataChanged();
    }
    else {
      CollectionChanged = true;
    }
  }

  private ObservableCollection<T> Collection { get; } = new();
  private bool CollectionChanged { get; set; }
  private bool UpdatesEnabled { get; set; } = true;

  private List<Mapping<T>> Mappings { get; } = new();
  private Func<T, double> XMapping { get; set; }
  private Func<T, double> YMapping { get; set; }
  private Func<T, Point> XyMapping { get; set; }

  public void AppendMany(IEnumerable<T> data) {
    ArgumentNullException.ThrowIfNull(data);
    UpdatesEnabled = false;
    foreach(var p_ in data) {
      Collection.Add(item: p_);
    }

    UpdatesEnabled = true;
    RaiseDataChanged();
  }

  public void AppendAsync(Dispatcher dispatcher, T item) {
    dispatcher.Invoke(priority: DispatcherPriority.Normal,
      method: new Action(() => {
        Collection.Add(item: item);
        RaiseDataChanged();
      }));
  }

  public void SetXMapping(Func<T, double> mapping) => XMapping = mapping ?? throw new ArgumentNullException(paramName: nameof(mapping));
  public void SetYMapping(Func<T, double> mapping) => YMapping = mapping ?? throw new ArgumentNullException(paramName: nameof(mapping));
  public void SetXYMapping(Func<T, Point> mapping) => XyMapping = mapping ?? throw new ArgumentNullException(paramName: nameof(mapping));

  #region IChartDataSource Members

  private sealed class ObservableIterator : IPointEnumerator {
    private ObservableDataSource<T> DataSource { get; }

    private IEnumerator<T> Enumerator { get; }

    public ObservableIterator(ObservableDataSource<T> dataSource) {
      DataSource = dataSource;
      Enumerator = dataSource.Collection.GetEnumerator();
    }

    #region IChartPointEnumerator Members

    public bool MoveNext() => Enumerator.MoveNext();

    public void GetCurrent(ref Point p) => DataSource.FillPoint(elem: Enumerator.Current, point: ref p);

    public void ApplyMappings(DependencyObject target) => DataSource.ApplyMappings(target: target, elem: Enumerator.Current);

    public void Dispose() {
      Enumerator.Dispose();
      GC.SuppressFinalize(this);
    }

    #endregion
  }

  private void FillPoint(T elem, ref Point point) {
    if(XyMapping != null) {
      point = XyMapping(arg: elem);
    }
    else {
      if(XMapping != null) {
        point.X = XMapping(arg: elem);
      }

      if(YMapping != null) {
        point.Y = YMapping(arg: elem);
      }
    }
  }

  private void ApplyMappings(DependencyObject target, T elem) {
    if(target == null) return;
    foreach(var mapping_ in Mappings) {
      target.SetValue(dp: mapping_.Property, value: mapping_.F(arg: elem));
    }
  }

  public IPointEnumerator GetEnumerator(DependencyObject context) {
    return new ObservableIterator(dataSource: this);
  }

  public event EventHandler DataChanged;
  private void RaiseDataChanged() => DataChanged?.Invoke(sender: this, e: EventArgs.Empty);

  #endregion
}