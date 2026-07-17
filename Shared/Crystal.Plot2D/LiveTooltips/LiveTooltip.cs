using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.LiveTooltips;

public sealed class LiveToolTip : ContentControl {
  private static int _nameCounter;

  static LiveToolTip() {
    var thisType = typeof(LiveToolTip);

    DefaultStyleKeyProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: thisType));
    FocusableProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));
    IsHitTestVisibleProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));
    BackgroundProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.White));
    OpacityProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0));
    BorderBrushProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.DarkGray));
    BorderThicknessProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Thickness(uniformLength: 1.0)));
  }

  public LiveToolTip() {
    Name = "Plotter2D_LiveToolTip_" + _nameCounter;
    _nameCounter++;
  }

  #region Properties

  public FrameworkElement Owner {
    get => (FrameworkElement)GetValue(dp: OwnerProperty);
    set => SetValue(dp: OwnerProperty, value: value);
  }

  public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(
    name: nameof(Owner),
    propertyType: typeof(FrameworkElement),
    ownerType: typeof(LiveToolTip),
    typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #endregion // end of Properties
}
