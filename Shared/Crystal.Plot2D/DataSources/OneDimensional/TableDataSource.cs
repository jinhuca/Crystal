using System.Data;

namespace Crystal.Plot2D.DataSources.OneDimensional;

/// <summary>
///   Data source that extracts sequence of points and their attributes from DataTable.
/// </summary>
public sealed class TableDataSource : EnumerableDataSource<DataRow> {
  public TableDataSource(DataTable table) : base(data: table.Rows) {
    // Subscribe to DataTable events
    table.TableNewRow += NewRowInsertedHandler;
    table.RowChanged += RowChangedHandler;
    table.RowDeleted += RowChangedHandler;
  }

  private void RowChangedHandler(object sender, DataRowChangeEventArgs e) {
    RaiseDataChanged();
  }

  private void NewRowInsertedHandler(object sender, DataTableNewRowEventArgs e) {
    // Raise DataChanged event. Plotter should redraw graph.
    // This will be done automatically when rows are added to table.
    RaiseDataChanged();
  }
}