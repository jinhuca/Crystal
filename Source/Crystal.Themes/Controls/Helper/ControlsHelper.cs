﻿namespace Crystal.Themes.Controls;

/// <summary>
/// A helper class that provides various controls.
/// </summary>
public static class ControlsHelper
{
  public static readonly DependencyProperty DisabledVisualElementVisibilityProperty = DependencyProperty.RegisterAttached(
    "DisabledVisualElementVisibility",
    typeof(Visibility),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));

  /// <summary>
  /// Gets the value to handle the visibility of the DisabledVisualElement in the template.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
  [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
  [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
  public static Visibility GetDisabledVisualElementVisibility(UIElement element)
  {
    return (Visibility)element.GetValue(DisabledVisualElementVisibilityProperty);
  }

  /// <summary>
  /// Sets the value to handle the visibility of the DisabledVisualElement in the template.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
  [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
  [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
  public static void SetDisabledVisualElementVisibility(UIElement element, Visibility value)
  {
    element.SetValue(DisabledVisualElementVisibilityProperty, value);
  }

  /// <summary>
  /// The DependencyProperty for the CharacterCasing property.
  /// The Controls content will be converted to upper or lower case if it is a string.
  /// </summary>
  public static readonly DependencyProperty ContentCharacterCasingProperty = DependencyProperty.RegisterAttached(
    "ContentCharacterCasing",
    typeof(CharacterCasing),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(CharacterCasing.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure), value => CharacterCasing.Normal <= (CharacterCasing)value && (CharacterCasing)value <= CharacterCasing.Upper);

  /// <summary>
  /// Gets the character casing of the control
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(ContentControl))]
  [AttachedPropertyBrowsableForType(typeof(DropDownButton))]
  [AttachedPropertyBrowsableForType(typeof(SplitButton))]
  [AttachedPropertyBrowsableForType(typeof(WindowCommands))]
  [AttachedPropertyBrowsableForType(typeof(ColorPalette))]
  public static CharacterCasing GetContentCharacterCasing(UIElement element)
  {
    return (CharacterCasing)element.GetValue(ContentCharacterCasingProperty);
  }

  /// <summary>
  /// Sets the character casing of the control
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(ContentControl))]
  [AttachedPropertyBrowsableForType(typeof(DropDownButton))]
  [AttachedPropertyBrowsableForType(typeof(SplitButton))]
  [AttachedPropertyBrowsableForType(typeof(WindowCommands))]
  [AttachedPropertyBrowsableForType(typeof(ColorPalette))]
  public static void SetContentCharacterCasing(UIElement element, CharacterCasing value)
  {
    element.SetValue(ContentCharacterCasingProperty, value);
  }

  public static readonly DependencyProperty RecognizesAccessKeyProperty = DependencyProperty.RegisterAttached(
    "RecognizesAccessKey",
    typeof(bool),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));

  /// <summary> 
  /// Gets the value if the inner ContentPresenter use AccessText in its style.
  /// </summary> 
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(ContentControl))]
  [AttachedPropertyBrowsableForType(typeof(DropDownButton))]
  [AttachedPropertyBrowsableForType(typeof(SplitButton))]
  public static bool GetRecognizesAccessKey(UIElement element)
  {
    return (bool)element.GetValue(RecognizesAccessKeyProperty);
  }

  /// <summary> 
  /// Sets the value if the inner ContentPresenter should use AccessText in its style.
  /// </summary> 
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(ContentControl))]
  [AttachedPropertyBrowsableForType(typeof(DropDownButton))]
  [AttachedPropertyBrowsableForType(typeof(SplitButton))]
  public static void SetRecognizesAccessKey(UIElement element, bool value)
  {
    element.SetValue(RecognizesAccessKeyProperty, BooleanBoxes.Box(value));
  }

  public static readonly DependencyProperty FocusBorderBrushProperty = DependencyProperty.RegisterAttached(
    "FocusBorderBrush",
    typeof(Brush),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  /// <summary>
  /// Gets the brush used to draw the focus border.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBox))]
  [AttachedPropertyBrowsableForType(typeof(DatePicker))]
  [AttachedPropertyBrowsableForType(typeof(ComboBox))]
  [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
  public static Brush GetFocusBorderBrush(DependencyObject obj)
  {
    return (Brush)obj.GetValue(FocusBorderBrushProperty);
  }

  /// <summary>
  /// Sets the brush used to draw the focus border.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBox))]
  [AttachedPropertyBrowsableForType(typeof(DatePicker))]
  [AttachedPropertyBrowsableForType(typeof(ComboBox))]
  [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
  public static void SetFocusBorderBrush(DependencyObject obj, Brush value)
  {
    obj.SetValue(FocusBorderBrushProperty, value);
  }

  public static readonly DependencyProperty FocusBorderThicknessProperty = DependencyProperty.RegisterAttached(
    "FocusBorderThickness",
    typeof(Thickness),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  /// <summary>
  /// Gets the brush used to draw the focus border.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBox))]
  [AttachedPropertyBrowsableForType(typeof(DatePicker))]
  [AttachedPropertyBrowsableForType(typeof(ComboBox))]
  [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
  public static Thickness GetFocusBorderThickness(DependencyObject obj)
  {
    return (Thickness)obj.GetValue(FocusBorderThicknessProperty);
  }

  /// <summary>
  /// Sets the brush used to draw the focus border.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBox))]
  [AttachedPropertyBrowsableForType(typeof(DatePicker))]
  [AttachedPropertyBrowsableForType(typeof(ComboBox))]
  [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
  public static void SetFocusBorderThickness(DependencyObject obj, Thickness value)
  {
    obj.SetValue(FocusBorderThicknessProperty, value);
  }

  public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.RegisterAttached(
    "MouseOverBorderBrush",
    typeof(Brush),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

  /// <summary>
  /// Gets the brush used to draw the mouse over brush.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBox))]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  [AttachedPropertyBrowsableForType(typeof(RadioButton))]
  [AttachedPropertyBrowsableForType(typeof(DatePicker))]
  [AttachedPropertyBrowsableForType(typeof(ComboBox))]
  [AttachedPropertyBrowsableForType(typeof(Tile))]
  public static Brush GetMouseOverBorderBrush(DependencyObject obj)
  {
    return (Brush)obj.GetValue(MouseOverBorderBrushProperty);
  }

  /// <summary>
  /// Sets the brush used to draw the mouse over brush.
  /// </summary>
  [Category(AppName.CrystalThemes)]
  [AttachedPropertyBrowsableForType(typeof(TextBox))]
  [AttachedPropertyBrowsableForType(typeof(CheckBox))]
  [AttachedPropertyBrowsableForType(typeof(RadioButton))]
  [AttachedPropertyBrowsableForType(typeof(DatePicker))]
  [AttachedPropertyBrowsableForType(typeof(ComboBox))]
  [AttachedPropertyBrowsableForType(typeof(Tile))]
  public static void SetMouseOverBorderBrush(DependencyObject obj, Brush value)
  {
    obj.SetValue(MouseOverBorderBrushProperty, value);
  }

  /// <summary>
  /// DependencyProperty for <see cref="CornerRadius"/> property.
  /// </summary>
  public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
    "CornerRadius",
    typeof(CornerRadius),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(new CornerRadius(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary> 
  /// The CornerRadius property allows users to control the roundness of the button corners independently by 
  /// setting a radius value for each corner. Radius values that are too large are scaled so that they
  /// smoothly blend from corner to corner. (Can be used e.g. at CrystalButton style)
  /// Description taken from original Microsoft description :-D
  /// </summary>
  [Category(AppName.CrystalThemes)]
  public static CornerRadius GetCornerRadius(UIElement element)
  {
    return (CornerRadius)element.GetValue(CornerRadiusProperty);
  }

  [Category(AppName.CrystalThemes)]
  public static void SetCornerRadius(UIElement element, CornerRadius value)
  {
    element.SetValue(CornerRadiusProperty, value);
  }

  /// <summary>
  /// Gets or sets a value indicating whether the child contents of the control are not editable.
  /// </summary>
  public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.RegisterAttached(
    "IsReadOnly",
    typeof(bool),
    typeof(ControlsHelper),
    new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets a value indicating whether the child contents of the control are not editable.
  /// </summary>
  public static bool GetIsReadOnly(UIElement element)
  {
    return (bool)element.GetValue(IsReadOnlyProperty);
  }

  /// <summary>
  /// Sets a value indicating whether the child contents of the control are not editable.
  /// </summary>
  public static void SetIsReadOnly(UIElement element, bool value)
  {
    element.SetValue(IsReadOnlyProperty, BooleanBoxes.Box(value));
  }
}