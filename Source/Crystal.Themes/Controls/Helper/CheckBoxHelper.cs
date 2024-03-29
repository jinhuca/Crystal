﻿namespace Crystal.Themes.Controls;

public static class CheckBoxHelper
{
  public static readonly DependencyProperty CheckSizeProperty = DependencyProperty.RegisterAttached(
    "CheckSize",
    typeof(double),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(18.0));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static double GetCheckSize(DependencyObject obj)
  {
    return (double)obj.GetValue(CheckSizeProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckSize(DependencyObject obj, double value)
  {
    obj.SetValue(CheckSizeProperty, value);
  }

  public static readonly DependencyProperty CheckCornerRadiusProperty = DependencyProperty.RegisterAttached(
    "CheckCornerRadius",
    typeof(CornerRadius),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(new CornerRadius(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static CornerRadius GetCheckCornerRadius(UIElement element)
  {
    return (CornerRadius)element.GetValue(CheckCornerRadiusProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckCornerRadius(UIElement element, CornerRadius value)
  {
    element.SetValue(CheckCornerRadiusProperty, value);
  }

  public static readonly DependencyProperty CheckStrokeThicknessProperty = DependencyProperty.RegisterAttached(
    "CheckStrokeThickness",
    typeof(double),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(1d));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static double GetCheckStrokeThickness(DependencyObject obj)
  {
    return (double)obj.GetValue(CheckStrokeThicknessProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckStrokeThickness(DependencyObject obj, double value)
  {
    obj.SetValue(CheckStrokeThicknessProperty, value);
  }

  #region Unchecked

  public static readonly DependencyProperty CheckGlyphUncheckedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphUnchecked",
    typeof(object),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(null));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static object? GetCheckGlyphUnchecked(DependencyObject obj)
  {
    return obj.GetValue(CheckGlyphUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphUnchecked(DependencyObject obj, object? value)
  {
    obj.SetValue(CheckGlyphUncheckedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphUncheckedTemplateProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphUncheckedTemplate",
    typeof(DataTemplate),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(DataTemplate)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static DataTemplate? GetCheckGlyphUncheckedTemplate(DependencyObject obj)
  {
    return (DataTemplate?)obj.GetValue(CheckGlyphUncheckedTemplateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphUncheckedTemplate(DependencyObject obj, DataTemplate? value)
  {
    obj.SetValue(CheckGlyphUncheckedTemplateProperty, value);
  }

  public static readonly DependencyProperty ForegroundUncheckedProperty = DependencyProperty.RegisterAttached(
    "ForegroundUnchecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundUnchecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundUnchecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundUncheckedProperty, value);
  }

  public static readonly DependencyProperty BackgroundUncheckedProperty = DependencyProperty.RegisterAttached(
    "BackgroundUnchecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundUnchecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundUnchecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundUncheckedProperty, value);
  }

  public static readonly DependencyProperty BorderBrushUncheckedProperty = DependencyProperty.RegisterAttached(
    "BorderBrushUnchecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushUnchecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushUnchecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushUncheckedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillUncheckedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillUnchecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillUnchecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillUnchecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillUncheckedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeUncheckedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeUnchecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeUnchecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeUnchecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeUncheckedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundUncheckedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundUnchecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundUnchecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundUncheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundUnchecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundUncheckedProperty, value);
  }

  public static readonly DependencyProperty ForegroundUncheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "ForegroundUncheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundUncheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundUncheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundUncheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundUncheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty BackgroundUncheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "BackgroundUncheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundUncheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundUncheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundUncheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundUncheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty BorderBrushUncheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "BorderBrushUncheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushUncheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushUncheckedMouseOverProperty);
  }

  /// <summary>
  /// Sets the BorderBrush for IsChecked = false, IsMouseOver = true.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushUncheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushUncheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillUncheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillUncheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillUncheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillUncheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillUncheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillUncheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeUncheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeUncheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeUncheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeUncheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeUncheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeUncheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundUncheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundUncheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundUncheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundUncheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundUncheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundUncheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty ForegroundUncheckedPressedProperty = DependencyProperty.RegisterAttached(
    "ForegroundUncheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundUncheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundUncheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundUncheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundUncheckedPressedProperty, value);
  }

  public static readonly DependencyProperty BackgroundUncheckedPressedProperty = DependencyProperty.RegisterAttached(
    "BackgroundUncheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundUncheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundUncheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundUncheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundUncheckedPressedProperty, value);
  }

  public static readonly DependencyProperty BorderBrushUncheckedPressedProperty = DependencyProperty.RegisterAttached(
    "BorderBrushUncheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushUncheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushUncheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushUncheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushUncheckedPressedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillUncheckedPressedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillUncheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillUncheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillUncheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillUncheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillUncheckedPressedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeUncheckedPressedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeUncheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeUncheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeUncheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeUncheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeUncheckedPressedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundUncheckedPressedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundUncheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundUncheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundUncheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundUncheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundUncheckedPressedProperty, value);
  }

  public static readonly DependencyProperty ForegroundUncheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "ForegroundUncheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundUncheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundUncheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundUncheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundUncheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty BackgroundUncheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "BackgroundUncheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundUncheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundUncheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundUncheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundUncheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty BorderBrushUncheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "BorderBrushUncheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushUncheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushUncheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushUncheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushUncheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillUncheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillUncheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillUncheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillUncheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillUncheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillUncheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeUncheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeUncheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeUncheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeUncheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeUncheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeUncheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundUncheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundUncheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundUncheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundUncheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundUncheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundUncheckedDisabledProperty, value);
  }

  #endregion

  #region Checked

  public static readonly DependencyProperty CheckGlyphCheckedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphChecked",
    typeof(object),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(null));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static object? GetCheckGlyphChecked(DependencyObject obj)
  {
    return obj.GetValue(CheckGlyphCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphChecked(DependencyObject obj, object? value)
  {
    obj.SetValue(CheckGlyphCheckedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphCheckedTemplateProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphCheckedTemplate",
    typeof(DataTemplate),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(DataTemplate)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static DataTemplate? GetCheckGlyphCheckedTemplate(DependencyObject obj)
  {
    return (DataTemplate?)obj.GetValue(CheckGlyphCheckedTemplateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphCheckedTemplate(DependencyObject obj, DataTemplate? value)
  {
    obj.SetValue(CheckGlyphCheckedTemplateProperty, value);
  }

  public static readonly DependencyProperty ForegroundCheckedProperty = DependencyProperty.RegisterAttached(
    "ForegroundChecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundChecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundChecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundCheckedProperty, value);
  }

  public static readonly DependencyProperty BackgroundCheckedProperty = DependencyProperty.RegisterAttached(
    "BackgroundChecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundChecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundChecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundCheckedProperty, value);
  }

  public static readonly DependencyProperty BorderBrushCheckedProperty = DependencyProperty.RegisterAttached(
    "BorderBrushChecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushChecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushChecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushCheckedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillCheckedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillChecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillChecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillChecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillCheckedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeCheckedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeChecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeChecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeChecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeCheckedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundCheckedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundChecked",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundChecked(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundCheckedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundChecked(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundCheckedProperty, value);
  }

  public static readonly DependencyProperty ForegroundCheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "ForegroundCheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundCheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundCheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundCheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundCheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty BackgroundCheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "BackgroundCheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundCheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundCheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundCheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundCheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty BorderBrushCheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "BorderBrushCheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushCheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushCheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushCheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushCheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillCheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillCheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillCheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillCheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillCheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillCheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeCheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeCheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeCheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeCheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeCheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeCheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundCheckedMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundCheckedMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundCheckedMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundCheckedMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundCheckedMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundCheckedMouseOverProperty, value);
  }

  public static readonly DependencyProperty ForegroundCheckedPressedProperty = DependencyProperty.RegisterAttached(
    "ForegroundCheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundCheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundCheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundCheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundCheckedPressedProperty, value);
  }

  public static readonly DependencyProperty BackgroundCheckedPressedProperty = DependencyProperty.RegisterAttached(
    "BackgroundCheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundCheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundCheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundCheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundCheckedPressedProperty, value);
  }

  public static readonly DependencyProperty BorderBrushCheckedPressedProperty = DependencyProperty.RegisterAttached(
    "BorderBrushCheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushCheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushCheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushCheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushCheckedPressedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillCheckedPressedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillCheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillCheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillCheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillCheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillCheckedPressedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeCheckedPressedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeCheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeCheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeCheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeCheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeCheckedPressedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundCheckedPressedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundCheckedPressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundCheckedPressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundCheckedPressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundCheckedPressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundCheckedPressedProperty, value);
  }

  public static readonly DependencyProperty ForegroundCheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "ForegroundCheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundCheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundCheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundCheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundCheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty BackgroundCheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "BackgroundCheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundCheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundCheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundCheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundCheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty BorderBrushCheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "BorderBrushCheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushCheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushCheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushCheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushCheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillCheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillCheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillCheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillCheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillCheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillCheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeCheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeCheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeCheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeCheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeCheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeCheckedDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundCheckedDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundCheckedDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundCheckedDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundCheckedDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundCheckedDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundCheckedDisabledProperty, value);
  }

  #endregion

  #region Indeterminate

  public static readonly DependencyProperty CheckGlyphIndeterminateProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphIndeterminate",
    typeof(object),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(null));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static object? GetCheckGlyphIndeterminate(DependencyObject obj)
  {
    return obj.GetValue(CheckGlyphIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphIndeterminate(DependencyObject obj, object? value)
  {
    obj.SetValue(CheckGlyphIndeterminateProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphIndeterminateTemplateProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphIndeterminateTemplate",
    typeof(DataTemplate),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(DataTemplate)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static DataTemplate? GetCheckGlyphIndeterminateTemplate(DependencyObject obj)
  {
    return (DataTemplate?)obj.GetValue(CheckGlyphIndeterminateTemplateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphIndeterminateTemplate(DependencyObject obj, DataTemplate? value)
  {
    obj.SetValue(CheckGlyphIndeterminateTemplateProperty, value);
  }

  public static readonly DependencyProperty ForegroundIndeterminateProperty = DependencyProperty.RegisterAttached(
    "ForegroundIndeterminate",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundIndeterminate(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundIndeterminate(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundIndeterminateProperty, value);
  }

  public static readonly DependencyProperty BackgroundIndeterminateProperty = DependencyProperty.RegisterAttached(
    "BackgroundIndeterminate",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundIndeterminate(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundIndeterminate(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundIndeterminateProperty, value);
  }

  public static readonly DependencyProperty BorderBrushIndeterminateProperty = DependencyProperty.RegisterAttached(
    "BorderBrushIndeterminate",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushIndeterminate(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushIndeterminate(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushIndeterminateProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillIndeterminateProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillIndeterminate",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillIndeterminate(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillIndeterminate(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillIndeterminateProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeIndeterminateProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeIndeterminate",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeIndeterminate(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeIndeterminate(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeIndeterminateProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundIndeterminateProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundIndeterminate",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundIndeterminate(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundIndeterminateProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundIndeterminate(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundIndeterminateProperty, value);
  }

  public static readonly DependencyProperty ForegroundIndeterminateMouseOverProperty = DependencyProperty.RegisterAttached(
    "ForegroundIndeterminateMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundIndeterminateMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundIndeterminateMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundIndeterminateMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundIndeterminateMouseOverProperty, value);
  }

  public static readonly DependencyProperty BackgroundIndeterminateMouseOverProperty = DependencyProperty.RegisterAttached(
    "BackgroundIndeterminateMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundIndeterminateMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundIndeterminateMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundIndeterminateMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundIndeterminateMouseOverProperty, value);
  }

  public static readonly DependencyProperty BorderBrushIndeterminateMouseOverProperty = DependencyProperty.RegisterAttached(
    "BorderBrushIndeterminateMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushIndeterminateMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushIndeterminateMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushIndeterminateMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushIndeterminateMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillIndeterminateMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillIndeterminateMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillIndeterminateMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillIndeterminateMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillIndeterminateMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillIndeterminateMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeIndeterminateMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeIndeterminateMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeIndeterminateMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeIndeterminateMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeIndeterminateMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeIndeterminateMouseOverProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundIndeterminateMouseOverProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundIndeterminateMouseOver",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundIndeterminateMouseOver(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundIndeterminateMouseOverProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundIndeterminateMouseOver(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundIndeterminateMouseOverProperty, value);
  }

  public static readonly DependencyProperty ForegroundIndeterminatePressedProperty = DependencyProperty.RegisterAttached(
    "ForegroundIndeterminatePressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundIndeterminatePressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundIndeterminatePressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundIndeterminatePressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundIndeterminatePressedProperty, value);
  }

  public static readonly DependencyProperty BackgroundIndeterminatePressedProperty = DependencyProperty.RegisterAttached(
    "BackgroundIndeterminatePressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundIndeterminatePressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundIndeterminatePressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundIndeterminatePressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundIndeterminatePressedProperty, value);
  }

  public static readonly DependencyProperty BorderBrushIndeterminatePressedProperty = DependencyProperty.RegisterAttached(
    "BorderBrushIndeterminatePressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushIndeterminatePressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushIndeterminatePressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushIndeterminatePressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushIndeterminatePressedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillIndeterminatePressedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillIndeterminatePressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillIndeterminatePressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillIndeterminatePressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillIndeterminatePressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillIndeterminatePressedProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeIndeterminatePressedProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeIndeterminatePressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeIndeterminatePressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeIndeterminatePressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeIndeterminatePressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeIndeterminatePressedProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundIndeterminatePressedProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundIndeterminatePressed",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundIndeterminatePressed(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundIndeterminatePressedProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundIndeterminatePressed(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundIndeterminatePressedProperty, value);
  }

  public static readonly DependencyProperty ForegroundIndeterminateDisabledProperty = DependencyProperty.RegisterAttached(
    "ForegroundIndeterminateDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetForegroundIndeterminateDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(ForegroundIndeterminateDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetForegroundIndeterminateDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(ForegroundIndeterminateDisabledProperty, value);
  }

  public static readonly DependencyProperty BackgroundIndeterminateDisabledProperty = DependencyProperty.RegisterAttached(
    "BackgroundIndeterminateDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBackgroundIndeterminateDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BackgroundIndeterminateDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBackgroundIndeterminateDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BackgroundIndeterminateDisabledProperty, value);
  }

  public static readonly DependencyProperty BorderBrushIndeterminateDisabledProperty = DependencyProperty.RegisterAttached(
    "BorderBrushIndeterminateDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetBorderBrushIndeterminateDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(BorderBrushIndeterminateDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetBorderBrushIndeterminateDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(BorderBrushIndeterminateDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundFillIndeterminateDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundFillIndeterminateDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundFillIndeterminateDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundFillIndeterminateDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundFillIndeterminateDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundFillIndeterminateDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckBackgroundStrokeIndeterminateDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckBackgroundStrokeIndeterminateDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckBackgroundStrokeIndeterminateDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckBackgroundStrokeIndeterminateDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckBackgroundStrokeIndeterminateDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckBackgroundStrokeIndeterminateDisabledProperty, value);
  }

  public static readonly DependencyProperty CheckGlyphForegroundIndeterminateDisabledProperty = DependencyProperty.RegisterAttached(
    "CheckGlyphForegroundIndeterminateDisabled",
    typeof(Brush),
    typeof(CheckBoxHelper),
    new FrameworkPropertyMetadata(default(Brush)));

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static Brush? GetCheckGlyphForegroundIndeterminateDisabled(DependencyObject obj)
  {
    return (Brush?)obj.GetValue(CheckGlyphForegroundIndeterminateDisabledProperty);
  }

  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  public static void SetCheckGlyphForegroundIndeterminateDisabled(DependencyObject obj, Brush? value)
  {
    obj.SetValue(CheckGlyphForegroundIndeterminateDisabledProperty, value);
  }

  #endregion
}