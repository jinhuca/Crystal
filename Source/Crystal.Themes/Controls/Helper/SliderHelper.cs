namespace Crystal.Themes.Controls;

public class SliderHelper
{
  public static readonly DependencyProperty ThumbFillBrushProperty = DependencyProperty.RegisterAttached(
    "ThumbFillBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetThumbFillBrush(UIElement element) => (Brush?)element.GetValue(ThumbFillBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetThumbFillBrush(UIElement element, Brush? value) => element.SetValue(ThumbFillBrushProperty, value);

  public static readonly DependencyProperty ThumbFillHoverBrushProperty = DependencyProperty.RegisterAttached(
    "ThumbFillHoverBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetThumbFillHoverBrush(UIElement element) => (Brush?)element.GetValue(ThumbFillHoverBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetThumbFillHoverBrush(UIElement element, Brush? value) => element.SetValue(ThumbFillHoverBrushProperty, value);

  public static readonly DependencyProperty ThumbFillPressedBrushProperty = DependencyProperty.RegisterAttached(
    "ThumbFillPressedBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetThumbFillPressedBrush(UIElement element) => (Brush?)element.GetValue(ThumbFillPressedBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetThumbFillPressedBrush(UIElement element, Brush? value) => element.SetValue(ThumbFillPressedBrushProperty, value);

  public static readonly DependencyProperty ThumbFillDisabledBrushProperty = DependencyProperty.RegisterAttached(
    "ThumbFillDisabledBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetThumbFillDisabledBrush(UIElement element) => (Brush?)element.GetValue(ThumbFillDisabledBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetThumbFillDisabledBrush(UIElement element, Brush? value) => element.SetValue(ThumbFillDisabledBrushProperty, value);

  public static readonly DependencyProperty TrackFillBrushProperty = DependencyProperty.RegisterAttached(
    "TrackFillBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackFillBrush(UIElement element) => (Brush?)element.GetValue(TrackFillBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackFillBrush(UIElement element, Brush? value) => element.SetValue(TrackFillBrushProperty, value);

  public static readonly DependencyProperty TrackFillHoverBrushProperty = DependencyProperty.RegisterAttached(
    "TrackFillHoverBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackFillHoverBrush(UIElement element) => (Brush?)element.GetValue(TrackFillHoverBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackFillHoverBrush(UIElement element, Brush? value) => element.SetValue(TrackFillHoverBrushProperty, value);

  public static readonly DependencyProperty TrackFillPressedBrushProperty = DependencyProperty.RegisterAttached(
    "TrackFillPressedBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackFillPressedBrush(UIElement element) => (Brush?)element.GetValue(TrackFillPressedBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackFillPressedBrush(UIElement element, Brush? value) => element.SetValue(TrackFillPressedBrushProperty, value);

  public static readonly DependencyProperty TrackFillDisabledBrushProperty = DependencyProperty.RegisterAttached(
    "TrackFillDisabledBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackFillDisabledBrush(UIElement element) => (Brush?)element.GetValue(TrackFillDisabledBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackFillDisabledBrush(UIElement element, Brush? value) => element.SetValue(TrackFillDisabledBrushProperty, value);

  public static readonly DependencyProperty TrackValueFillBrushProperty = DependencyProperty.RegisterAttached(
    "TrackValueFillBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackValueFillBrush(UIElement element) => (Brush?)element.GetValue(TrackValueFillBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackValueFillBrush(UIElement element, Brush? value) => element.SetValue(TrackValueFillBrushProperty, value);

  public static readonly DependencyProperty TrackValueFillHoverBrushProperty = DependencyProperty.RegisterAttached(
    "TrackValueFillHoverBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackValueFillHoverBrush(UIElement element) => (Brush?)element.GetValue(TrackValueFillHoverBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackValueFillHoverBrush(UIElement element, Brush? value)
  {
    element.SetValue(TrackValueFillHoverBrushProperty, value);
  }

  public static readonly DependencyProperty TrackValueFillPressedBrushProperty = DependencyProperty.RegisterAttached(
    "TrackValueFillPressedBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackValueFillPressedBrush(UIElement element) => (Brush?)element.GetValue(TrackValueFillPressedBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackValueFillPressedBrush(UIElement element, Brush? value) => element.SetValue(TrackValueFillPressedBrushProperty, value);

  public static readonly DependencyProperty TrackValueFillDisabledBrushProperty = DependencyProperty.RegisterAttached(
    "TrackValueFillDisabledBrush",
    typeof(Brush),
    typeof(SliderHelper),
    new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static Brush? GetTrackValueFillDisabledBrush(UIElement element) => (Brush?)element.GetValue(TrackValueFillDisabledBrushProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetTrackValueFillDisabledBrush(UIElement element, Brush? value) => element.SetValue(TrackValueFillDisabledBrushProperty, value);

  public static readonly DependencyProperty ChangeValueByProperty = DependencyProperty.RegisterAttached(
    "ChangeValueBy",
    typeof(MouseWheelChange),
    typeof(SliderHelper),
    new PropertyMetadata(MouseWheelChange.SmallChange));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static MouseWheelChange GetChangeValueBy(UIElement element) => (MouseWheelChange)element.GetValue(ChangeValueByProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetChangeValueBy(UIElement element, MouseWheelChange value) => element.SetValue(ChangeValueByProperty, value);

  public static readonly DependencyProperty EnableMouseWheelProperty = DependencyProperty.RegisterAttached(
    "EnableMouseWheel",
    typeof(MouseWheelState),
    typeof(SliderHelper),
    new PropertyMetadata(MouseWheelState.None, OnEnableMouseWheelChanged));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static MouseWheelState GetEnableMouseWheel(UIElement element) => (MouseWheelState)element.GetValue(EnableMouseWheelProperty);

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(Slider))]
  [AttachedPropertyBrowsableForType(typeof(RangeSlider))]
  public static void SetEnableMouseWheel(UIElement element, MouseWheelState value) => element.SetValue(EnableMouseWheelProperty, value);

  private static void OnEnableMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (e.NewValue != e.OldValue)
    {
      switch (d)
      {
        case Slider slider:
          {
            slider.PreviewMouseWheel -= OnSliderPreviewMouseWheel;
            if ((MouseWheelState)e.NewValue != MouseWheelState.None)
            {
              slider.PreviewMouseWheel += OnSliderPreviewMouseWheel;
            }

            break;
          }
        case RangeSlider rangeSlider:
          {
            rangeSlider.PreviewMouseWheel -= OnRangeSliderPreviewMouseWheel;
            if ((MouseWheelState)e.NewValue != MouseWheelState.None)
            {
              rangeSlider.PreviewMouseWheel += OnRangeSliderPreviewMouseWheel;
            }

            break;
          }
      }
    }
  }

  internal static object ConstrainToRange(RangeBase rangeBase, double value)
  {
    var minimum = rangeBase.Minimum;
    if (value < minimum)
    {
      return minimum;
    }

    var maximum = rangeBase.Maximum;
    if (value > maximum)
    {
      return maximum;
    }

    return value;
  }

  private static void OnSliderPreviewMouseWheel(object sender, MouseWheelEventArgs e)
  {
    if (sender is Slider slider && (slider.IsFocused || MouseWheelState.MouseHover.Equals(slider.GetValue(EnableMouseWheelProperty))))
    {
      var changeType = (MouseWheelChange)slider.GetValue(ChangeValueByProperty);
      var difference = changeType == MouseWheelChange.LargeChange ? slider.LargeChange : slider.SmallChange;

      var currentValue = slider.Value;
      var newValue = ConstrainToRange(slider, e.Delta > 0 ? currentValue + difference : currentValue - difference);

      slider.SetCurrentValue(RangeBase.ValueProperty, newValue);

      e.Handled = true;
    }
  }

  private static void OnRangeSliderPreviewMouseWheel(object sender, MouseWheelEventArgs e)
  {
    if (sender is RangeSlider rangeSlider && (rangeSlider.IsFocused || MouseWheelState.MouseHover.Equals(rangeSlider.GetValue(EnableMouseWheelProperty))))
    {
      var changeType = (MouseWheelChange)rangeSlider.GetValue(ChangeValueByProperty);
      var difference = changeType == MouseWheelChange.LargeChange ? rangeSlider.LargeChange : rangeSlider.SmallChange;

      if (e.Delta > 0)
      {
        rangeSlider.SetCurrentValue(RangeSlider.UpperValueProperty, RangeSlider.CoerceUpperValue(rangeSlider, rangeSlider.UpperValue + difference));
        rangeSlider.SetCurrentValue(RangeSlider.LowerValueProperty, RangeSlider.CoerceLowerValue(rangeSlider, rangeSlider.LowerValue + difference));
      }
      else
      {
        rangeSlider.SetCurrentValue(RangeSlider.LowerValueProperty, RangeSlider.CoerceLowerValue(rangeSlider, rangeSlider.LowerValue - difference));
        rangeSlider.SetCurrentValue(RangeSlider.UpperValueProperty, RangeSlider.CoerceUpperValue(rangeSlider, rangeSlider.UpperValue - difference));
      }

      e.Handled = true;
    }
  }
}