using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystal.Plot2D.LiveTooltips;

internal sealed class LiveToolTipAdorner : Adorner {
  private readonly Canvas canvas = new() { IsHitTestVisible = false };
  private readonly VisualCollection visualChildren;

  public LiveToolTipAdorner(UIElement adornedElement, LiveToolTip tooltip)
    : base(adornedElement: adornedElement) {
    visualChildren = new VisualCollection(parent: this);

    adornedElement.MouseLeave += adornedElement_MouseLeave;
    adornedElement.MouseEnter += adornedElement_MouseEnter;
    adornedElement.PreviewMouseMove += adornedElement_MouseMove;
    //FrameworkElement frAdornedElement = (FrameworkElement)adornedElement;
    //frAdornedElement.SizeChanged += frAdornedElement_SizeChanged;

    liveTooltip = tooltip;

    tooltip.Visibility = Visibility.Hidden;

    canvas.Children.Add(element: liveTooltip);
    AddLogicalChild(child: canvas);
    visualChildren.Add(visual: canvas);

    Unloaded += LiveTooltipAdorner_Unloaded;
  }

  //void frAdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
  //{
  //    grid.Width = e.NewSize.Width;
  //    grid.Height = e.NewSize.Height;

  //    InvalidateMeasure();
  //}

  private void LiveTooltipAdorner_Unloaded(object sender, RoutedEventArgs e) {
    canvas.Children.Remove(element: liveTooltip);
  }

  private void adornedElement_MouseLeave(object sender, MouseEventArgs e) {
    liveTooltip.Visibility = Visibility.Hidden;
  }

  private void adornedElement_MouseEnter(object sender, MouseEventArgs e) {
    liveTooltip.Visibility = Visibility.Visible;
  }

  private Point mousePosition;
  private void adornedElement_MouseMove(object sender, MouseEventArgs e) {
    liveTooltip.Visibility = Visibility.Visible;
    mousePosition = e.GetPosition(relativeTo: AdornedElement);
    InvalidateMeasure();
  }

  private void ArrangeTooltip() {
    var tooltipSize = liveTooltip.DesiredSize;

    var location = mousePosition;
    location.Offset(offsetX: -tooltipSize.Width / 2, offsetY: -tooltipSize.Height - 1);

    liveTooltip.Arrange(finalRect: new Rect(location: location, size: tooltipSize));
  }

  private readonly LiveToolTip liveTooltip;
  public LiveToolTip LiveTooltip => liveTooltip;

  #region Overrides

  protected override Visual GetVisualChild(int index) {
    return visualChildren[index: index];
  }

  protected override int VisualChildrenCount => visualChildren.Count;

  protected override Size MeasureOverride(Size constraint) {
    foreach(UIElement item in visualChildren) {
      item.Measure(availableSize: constraint);
    }

    liveTooltip.Measure(availableSize: constraint);

    return base.MeasureOverride(constraint: constraint);
  }

  protected override Size ArrangeOverride(Size finalSize) {
    foreach(UIElement item in visualChildren) {
      item.Arrange(finalRect: new Rect(size: item.DesiredSize));
    }

    ArrangeTooltip();

    return finalSize;
  }

  #endregion // end of overrides
}
