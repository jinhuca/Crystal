using Crystal.Plot2D.Common;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Shapes;

/// <inheritdoc cref="Shape" />
/// <summary>
/// Represents a base class for simple shapes with viewport-bound coordinates.
/// </summary>
public abstract class ViewportShape : Shape, IPlotterElement {
  static ViewportShape() {
    var type_ = typeof(ViewportShape);
    StrokeProperty.AddOwner(ownerType: type_, typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Blue));
    StrokeThicknessProperty.AddOwner(ownerType: type_, typeMetadata: new FrameworkPropertyMetadata(defaultValue: 2.0));
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ViewportShape"/> class.
  /// </summary>
  protected ViewportShape() {
  }

  protected void UpdateUIRepresentation() {
    if(Plotter == null) {
      return;
    }

    UpdateUIRepresentationCore();
  }

  protected virtual void UpdateUIRepresentationCore() {
  }

  #region IPlotterElement Members

  private PlotterBase plotter;

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) {
    plotter.CentralGrid.Children.Add(element: this);

    var plotter2d_ = (PlotterBase)plotter;
    this.plotter = plotter2d_;
    plotter2d_.Viewport.PropertyChanged += Viewport_PropertyChanged;

    UpdateUIRepresentation();
  }

  private void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    OnViewportPropertyChanged();
  }

  private void OnViewportPropertyChanged() {
    UpdateUIRepresentation();
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) {
    var plotter2d_ = (PlotterBase)plotter;
    plotter2d_.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    plotter.CentralGrid.Children.Remove(element: this);

    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
