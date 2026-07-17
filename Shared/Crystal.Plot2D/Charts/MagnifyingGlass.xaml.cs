using Crystal.Plot2D.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public partial class MagnifyingGlass : IPlotterElement {
  public MagnifyingGlass() {
    InitializeComponent();
    Loaded += MagnifyingGlass_Loaded;

    whiteEllipse.Visibility = Visibility.Collapsed;
    magnifierEllipse.Visibility = Visibility.Collapsed;
  }

  private void MagnifyingGlass_Loaded(object sender, RoutedEventArgs e) {
    UpdateViewBox();
  }

  protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
    Magnification += e.Delta / Mouse.MouseWheelDeltaForOneLine * 0.2;
    e.Handled = false;
  }

  private void plotter_PreviewMouseMove(object sender, MouseEventArgs e) {
    var b_ = (VisualBrush)magnifierEllipse.Fill;
    var pos_ = e.GetPosition(relativeTo: plotter.ParallelCanvas);

    var plotterPos_ = e.GetPosition(relativeTo: plotter);

    var viewBox_ = b_.Viewbox;
    var xOffset_ = viewBox_.Width / 2.0;
    var yOffset_ = viewBox_.Height / 2.0;
    viewBox_.X = plotterPos_.X - xOffset_;
    viewBox_.Y = plotterPos_.Y - yOffset_;
    b_.Viewbox = viewBox_;
    Canvas.SetLeft(element: this, length: pos_.X - Width / 2);
    Canvas.SetTop(element: this, length: pos_.Y - Height / 2);
  }

  private double magnification = 2.0;

  public double Magnification {
    get => magnification;
    set {
      magnification = value;

      UpdateViewBox();
    }
  }

  private void UpdateViewBox() {
    if(!IsLoaded) {
      return;
    }

    var b_ = (VisualBrush)magnifierEllipse.Fill;
    var viewBox_ = b_.Viewbox;
    viewBox_.Width = Width / magnification;
    viewBox_.Height = Height / magnification;
    b_.Viewbox = viewBox_;
  }

  protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
    base.OnPropertyChanged(e: e);

    if(e.Property == WidthProperty || e.Property == HeightProperty) {
      UpdateViewBox();
    }
  }

  #region IPlotterElement Members

  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter) {
    this.plotter = plotter;
    plotter.ParallelCanvas.Children.Add(element: this);
    plotter.PreviewMouseMove += plotter_PreviewMouseMove;
    plotter.MouseEnter += plotter_MouseEnter;
    plotter.MouseLeave += plotter_MouseLeave;

    var b_ = (VisualBrush)magnifierEllipse.Fill;
    b_.Visual = plotter.MainGrid;
  }

  private void plotter_MouseLeave(object sender, MouseEventArgs e) {
    whiteEllipse.Visibility = Visibility.Collapsed;
    magnifierEllipse.Visibility = Visibility.Collapsed;
  }

  private void plotter_MouseEnter(object sender, MouseEventArgs e) {
    whiteEllipse.Visibility = Visibility.Visible;
    magnifierEllipse.Visibility = Visibility.Visible;
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    plotter.MouseEnter -= plotter_MouseEnter;
    plotter.MouseLeave -= plotter_MouseLeave;

    plotter.PreviewMouseMove -= plotter_PreviewMouseMove;
    plotter.ParallelCanvas.Children.Remove(element: this);
    this.plotter = null;

    var b_ = (VisualBrush)magnifierEllipse.Fill;
    b_.Visual = null;
  }

  public PlotterBase Plotter => plotter;

  #endregion
}
