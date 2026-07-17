using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional;

/// <summary>
/// Data source that is a composer from several other data sources.
/// </summary>
public sealed class CompositeDataSource : IPointDataSource {
  #region Constructors

  /// <summary>
  /// Initializes a new instance of the <see cref="CompositeDataSource"/> class
  /// </summary>
  public CompositeDataSource() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="CompositeDataSource"/> class.
  /// </summary>
  /// <param name="dataSources">
  /// Data sources.
  /// </param>
  public CompositeDataSource(params IPointDataSource[] dataSources) {
    ArgumentNullException.ThrowIfNull(dataSources);

    foreach(var dataSource in dataSources) {
      AddDataPart(dataPart: dataSource);
    }
  }

  #endregion Constructors

  #region 

  private readonly List<IPointDataSource> dataParts = new();

  public IEnumerable<IPointDataSource> DataParts => dataParts;

  /// <summary>
  ///   Adds data part.
  /// </summary>
  /// <param name="dataPart">
  ///   The data part.
  /// </param>
  public void AddDataPart(IPointDataSource dataPart) {
    ArgumentNullException.ThrowIfNull(dataPart);

    dataParts.Add(item: dataPart);
    dataPart.DataChanged += OnPartDataChanged;
  }

  private void OnPartDataChanged(object sender, EventArgs e) => RaiseDataChanged();

  #endregion 

  #region IPointSource Members

  public event EventHandler DataChanged;
  private void RaiseDataChanged() => DataChanged?.Invoke(sender: this, e: EventArgs.Empty);
  public IPointEnumerator GetEnumerator(DependencyObject context) => new CompositeEnumerator(dataSource: this, context: context);

  #endregion

  private sealed class CompositeEnumerator : IPointEnumerator {
    private readonly IEnumerable<IPointEnumerator> enumerators;

    public CompositeEnumerator(CompositeDataSource dataSource, DependencyObject context) {
      enumerators = dataSource.dataParts.Select(selector: part => part.GetEnumerator(context: context)).ToList();
    }

    #region IChartPointEnumerator Members

    public bool MoveNext() {
      var res = false;
      foreach(var enumerator in enumerators) {
        res |= enumerator.MoveNext();
      }
      return res;
    }

    public void ApplyMappings(DependencyObject glyph) {
      foreach(var enumerator in enumerators) {
        enumerator.ApplyMappings(target: glyph);
      }
    }

    public void GetCurrent(ref Point p) {
      foreach(var enumerator in enumerators) {
        enumerator.GetCurrent(p: ref p);
      }
    }

    public void Dispose() {
      foreach(var enumerator in enumerators) {
        enumerator.Dispose();
      }
    }

    #endregion
  }
}