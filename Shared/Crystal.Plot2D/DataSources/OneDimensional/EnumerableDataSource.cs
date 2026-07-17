using Crystal.Plot2D.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.DataSources.OneDimensional;

public class EnumerableDataSource<T> : EnumerableDataSourceBase<T> {
  public EnumerableDataSource(IEnumerable<T> data) : base(data: data) { }
  public EnumerableDataSource(IEnumerable data) : base(data: data) { }

  private Func<T, Point> _xyMapping;

  [NotNull]
  public Func<T, Point> XYMapping {
    get => _xyMapping;
    set {
      _xyMapping = value ?? throw new ArgumentNullException(paramName: nameof(XYMapping));
      RaiseDataChanged();
    }
  }

  private Func<T, double> _xMapping;

  [NotNull]
  public Func<T, double> XMapping {
    get => _xMapping;
    set {
      _xMapping = value ?? throw new ArgumentNullException(paramName: nameof(XMapping));
      RaiseDataChanged();
    }
  }

  private Func<T, double> _yMapping;

  [NotNull]
  public Func<T, double> YMapping {
    get => _yMapping;
    set {
      _yMapping = value ?? throw new ArgumentNullException(paramName: nameof(YMapping));
      RaiseDataChanged();
    }
  }

  public void AddMapping(DependencyProperty property, Func<T, object> mapping) {
    ArgumentNullException.ThrowIfNull(property);
    ArgumentNullException.ThrowIfNull(mapping);
    Mappings.Add(item: new Mapping<T> { Property = property, F = mapping });
  }

  public override IPointEnumerator GetEnumerator(DependencyObject context)
    => new EnumerablePointEnumerator<T>(dataSource: this);

  private List<Mapping<T>> Mappings { get; } = new();

  internal void FillPoint(T elem, ref Point point) {
    if(XYMapping != null) {
      point = XYMapping(arg: elem);
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

  internal void ApplyMappings(DependencyObject target, T elem) {
    if(target == null) return;
    foreach(var mapping_ in Mappings) {
      target.SetValue(dp: mapping_.Property, value: mapping_.F(arg: elem));
    }
  }
}