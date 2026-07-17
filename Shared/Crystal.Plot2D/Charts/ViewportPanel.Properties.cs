using Crystal.Plot2D.Common;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public partial class ViewportPanel {
  #region Properties

  #region ViewportBounds

  [AttachedPropertyBrowsableForChildren]
  public static DataRect GetViewportBounds(DependencyObject obj) => (DataRect)obj.GetValue(dp: ViewportBoundsProperty);

  public static void SetViewportBounds(DependencyObject obj, DataRect value) => obj.SetValue(dp: ViewportBoundsProperty, value: value);

  public static readonly DependencyProperty ViewportBoundsProperty = DependencyProperty.RegisterAttached(
    name: "ViewportBounds",
    propertyType: typeof(DataRect),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: DataRect.Empty, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region X

  [AttachedPropertyBrowsableForChildren]
  public static double GetX(DependencyObject obj) => (double)obj.GetValue(dp: XProperty);

  public static void SetX(DependencyObject obj, double value) => obj.SetValue(dp: XProperty, value: value);

  public static readonly DependencyProperty XProperty = DependencyProperty.RegisterAttached(
    name: "X",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region Y

  [AttachedPropertyBrowsableForChildren]
  public static double GetY(DependencyObject obj) => (double)obj.GetValue(dp: YProperty);

  public static void SetY(DependencyObject obj, double value) => obj.SetValue(dp: YProperty, value: value);

  public static readonly DependencyProperty YProperty = DependencyProperty.RegisterAttached(
    name: "Y",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region ViewportWidth

  [AttachedPropertyBrowsableForChildren]
  public static double GetViewportWidth(DependencyObject obj) => (double)obj.GetValue(dp: ViewportWidthProperty);

  public static void SetViewportWidth(DependencyObject obj, double value) => obj.SetValue(dp: ViewportWidthProperty, value: value);

  public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.RegisterAttached(
    name: "ViewportWidth",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region ViewportHeight

  [AttachedPropertyBrowsableForChildren]
  public static double GetViewportHeight(DependencyObject obj) => (double)obj.GetValue(dp: ViewportHeightProperty);

  public static void SetViewportHeight(DependencyObject obj, double value) => obj.SetValue(dp: ViewportHeightProperty, value: value);

  public static readonly DependencyProperty ViewportHeightProperty = DependencyProperty.RegisterAttached(
    name: "ViewportHeight",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region ScreenOffsetX

  [AttachedPropertyBrowsableForChildren]
  public static double GetScreenOffsetX(DependencyObject obj) => (double)obj.GetValue(dp: ScreenOffsetXProperty);

  public static void SetScreenOffsetX(DependencyObject obj, double value) => obj.SetValue(dp: ScreenOffsetXProperty, value: value);

  public static readonly DependencyProperty ScreenOffsetXProperty = DependencyProperty.RegisterAttached(
    name: "ScreenOffsetX",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region ScreenOffsetY

  [AttachedPropertyBrowsableForChildren]
  public static double GetScreenOffsetY(DependencyObject obj) => (double)obj.GetValue(dp: ScreenOffsetYProperty);

  public static void SetScreenOffsetY(DependencyObject obj, double value) => obj.SetValue(dp: ScreenOffsetYProperty, value: value);

  public static readonly DependencyProperty ScreenOffsetYProperty = DependencyProperty.RegisterAttached(
    name: "ScreenOffsetY",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region HorizontalAlignment

  [AttachedPropertyBrowsableForChildren]
  public static HorizontalAlignment GetViewportHorizontalAlignment(DependencyObject obj)
    => (HorizontalAlignment)obj.GetValue(dp: ViewportHorizontalAlignmentProperty);

  public static void SetViewportHorizontalAlignment(DependencyObject obj, HorizontalAlignment value)
    => obj.SetValue(dp: ViewportHorizontalAlignmentProperty, value: value);

  public static readonly DependencyProperty ViewportHorizontalAlignmentProperty = DependencyProperty.RegisterAttached(
    name: "ViewportHorizontalAlignment",
    propertyType: typeof(HorizontalAlignment),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: HorizontalAlignment.Center, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region VerticalAlignment

  [AttachedPropertyBrowsableForChildren]
  public static VerticalAlignment GetViewportVerticalAlignment(DependencyObject obj)
    => (VerticalAlignment)obj.GetValue(dp: ViewportVerticalAlignmentProperty);

  public static void SetViewportVerticalAlignment(DependencyObject obj, VerticalAlignment value)
    => obj.SetValue(dp: ViewportVerticalAlignmentProperty, value: value);

  public static readonly DependencyProperty ViewportVerticalAlignmentProperty = DependencyProperty.RegisterAttached(
    name: "ViewportVerticalAlignment",
    propertyType: typeof(VerticalAlignment),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: VerticalAlignment.Center, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion

  #region ActualViewportBounds

  public static DataRect GetActualViewportBounds(DependencyObject obj)
    => (DataRect)obj.GetValue(dp: ActualViewportBoundsProperty);

  public static void SetActualViewportBounds(DependencyObject obj, DataRect value)
    => obj.SetValue(dp: ActualViewportBoundsProperty, value: value);

  public static readonly DependencyProperty ActualViewportBoundsProperty = DependencyProperty.RegisterAttached(
    name: "ActualViewportBounds",
    propertyType: typeof(DataRect),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: DataRect.Empty));

  #endregion

  #region PrevActualViewportBounds

  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public static DataRect GetPrevActualViewportBounds(DependencyObject obj)
    => (DataRect)obj.GetValue(dp: PrevActualViewportBoundsProperty);

  public static void SetPrevActualViewportBounds(DependencyObject obj, DataRect value)
    => obj.SetValue(dp: PrevActualViewportBoundsProperty, value: value);

  public static readonly DependencyProperty PrevActualViewportBoundsProperty = DependencyProperty.RegisterAttached(
    name: "PrevActualViewportBounds",
    propertyType: typeof(DataRect),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: DataRect.Empty));

  #endregion

  #region MinScreenWidth

  public static double GetMinScreenWidth(DependencyObject obj)
    => (double)obj.GetValue(dp: MinScreenWidthProperty);

  public static void SetMinScreenWidth(DependencyObject obj, double value)
    => obj.SetValue(dp: MinScreenWidthProperty, value: value);

  public static readonly DependencyProperty MinScreenWidthProperty = DependencyProperty.RegisterAttached(
    name: "MinScreenWidth",
    propertyType: typeof(double),
    ownerType: typeof(ViewportPanel),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, propertyChangedCallback: OnLayoutPropertyChanged));

  #endregion // end of MinScreenWidth

  protected static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if(d is FrameworkElement uiElement_) {
      if(VisualTreeHelper.GetParent(reference: uiElement_) is ViewportPanel panel_) {
        // invalidating not self arrange, but calling Arrange method of only that uiElement which has changed position
        panel_.InvalidatePosition(child: uiElement_);
      }
    }
  }

  #region IsMarkersHost

  public bool IsMarkersHost {
    get => (bool)GetValue(dp: IsMarkersHostProperty);
    set => SetValue(dp: IsMarkersHostProperty, value: value);
  }

  public static readonly DependencyProperty IsMarkersHostProperty = DependencyProperty.Register(
    name: nameof(IsMarkersHost),
    propertyType: typeof(bool),
    ownerType: typeof(ViewportPanel),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion // end of IsMarkersHost

  #endregion
}
