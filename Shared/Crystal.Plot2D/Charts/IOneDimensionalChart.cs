using Crystal.Plot2D.DataSources.OneDimensional;
using System;

namespace Crystal.Plot2D.Charts;

internal interface IOneDimensionalChart {
  IPointDataSource DataSource { get; set; }
  event EventHandler DataChanged;
}
