using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Crystal.Themes.Behaviors;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  internal enum SpellingResourceKeyId
  {
    SuggestionMenuItemStyle,
    IgnoreAllMenuItemStyle,
    NoSuggestionsMenuItemStyle,
    SeparatorStyle,
  }

  public static class Spelling
  {
    public static ResourceKey SuggestionMenuItemStyleKey { get; } = new ComponentResourceKey(typeof(Spelling), SpellingResourceKeyId.SuggestionMenuItemStyle);
    public static ResourceKey IgnoreAllMenuItemStyleKey { get; } = new ComponentResourceKey(typeof(Spelling), SpellingResourceKeyId.IgnoreAllMenuItemStyle);
    public static ResourceKey NoSuggestionsMenuItemStyleKey { get; } = new ComponentResourceKey(typeof(Spelling), SpellingResourceKeyId.NoSuggestionsMenuItemStyle);
    public static ResourceKey SeparatorStyleKey { get; } = new ComponentResourceKey(typeof(Spelling), SpellingResourceKeyId.SeparatorStyle);
  }

  /// <summary>
  /// A helper class that provides various attached properties for the TextBox control.
  /// </summary>
  /// <remarks>
  /// Password watermarking code from: http://prabu-guru.blogspot.com/2010/06/how-to-add-watermark-text-to-textbox.html
  /// </remarks>
  public class TextBoxHelper
  {
    public static readonly DependencyProperty IsMonitoringProperty = DependencyProperty.RegisterAttached(
      "IsMonitoring",
      typeof(bool),
      typeof(TextBoxHelper),
      new UIPropertyMetadata(BooleanBoxes.FalseBox, OnIsMonitoringChanged));

    [Category(AppName.CrystalThemes)]
    public static bool GetIsMonitoring(UIElement element) => (bool)element.GetValue(IsMonitoringProperty);

    [Category(AppName.CrystalThemes)]
    public static void SetIsMonitoring(DependencyObject obj, bool value) => obj.SetValue(IsMonitoringProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
      "Watermark",
      typeof(string),
      typeof(TextBoxHelper),
      new UIPropertyMetadata(string.Empty));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

    public static readonly DependencyProperty WatermarkAlignmentProperty = DependencyProperty.RegisterAttached(
      "WatermarkAlignment",
      typeof(TextAlignment),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static TextAlignment GetWatermarkAlignment(DependencyObject obj) => (TextAlignment)obj.GetValue(WatermarkAlignmentProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static void SetWatermarkAlignment(DependencyObject obj, TextAlignment value) => obj.SetValue(WatermarkAlignmentProperty, value);

    public static readonly DependencyProperty WatermarkTrimmingProperty = DependencyProperty.RegisterAttached(
      "WatermarkTrimming",
      typeof(TextTrimming),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(TextTrimming.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static TextTrimming GetWatermarkTrimming(DependencyObject obj) => (TextTrimming)obj.GetValue(WatermarkTrimmingProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static void SetWatermarkTrimming(DependencyObject obj, TextTrimming value) => obj.SetValue(WatermarkTrimmingProperty, value);

    public static readonly DependencyProperty WatermarkWrappingProperty = DependencyProperty.RegisterAttached(
      "WatermarkWrapping",
      typeof(TextWrapping),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    public static TextWrapping GetWatermarkWrapping(DependencyObject obj) => (TextWrapping)obj.GetValue(WatermarkWrappingProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    public static void SetWatermarkWrapping(DependencyObject obj, TextWrapping value) => obj.SetValue(WatermarkWrappingProperty, value);

    public static readonly DependencyProperty UseFloatingWatermarkProperty = DependencyProperty.RegisterAttached(
      "UseFloatingWatermark",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, ButtonCommandOrClearTextChanged));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static bool GetUseFloatingWatermark(DependencyObject obj) => (bool)obj.GetValue(UseFloatingWatermarkProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static void SetUseFloatingWatermark(DependencyObject obj, bool value) => obj.SetValue(UseFloatingWatermarkProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty AutoWatermarkProperty = DependencyProperty.RegisterAttached(
      "AutoWatermark",
      typeof(bool),
      typeof(TextBoxHelper),
      new PropertyMetadata(BooleanBoxes.FalseBox, OnAutoWatermarkChanged));

    private static void OnAutoWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is FrameworkElement element && e.OldValue != e.NewValue)
      {
        if ((e.NewValue as bool?).GetValueOrDefault())
        {
          if (element.IsLoaded)
          {
            OnControlWithAutoWatermarkSupportLoaded(element, new RoutedEventArgs());
          }
          else
          {
            element.Loaded += OnControlWithAutoWatermarkSupportLoaded;
          }
        }
        else
        {
          element.Loaded -= OnControlWithAutoWatermarkSupportLoaded;
        }
      }
    }

    private static void OnControlWithAutoWatermarkSupportLoaded(object o, RoutedEventArgs routedEventArgs)
    {
      if (o is FrameworkElement fe)
      {
        fe.Loaded -= OnControlWithAutoWatermarkSupportLoaded;

        if (!AutoWatermarkPropertyMapping.TryGetValue(fe.GetType(), out var dependencyProperty))
        {
          throw new NotSupportedException($"{nameof(AutoWatermarkProperty)} is not supported for {fe.GetType()}");
        }

        var resolvedProperty = ResolvePropertyFromBindingExpression(fe.GetBindingExpression(dependencyProperty));
        if (resolvedProperty != null)
        {
          var attribute = resolvedProperty.GetCustomAttribute<DisplayAttribute>();
          if (attribute != null)
          {
            fe.SetCurrentValue(WatermarkProperty, attribute.GetPrompt());
          }
        }
      }
    }

    private static PropertyInfo? ResolvePropertyFromBindingExpression(BindingExpression? bindingExpression)
    {
      if (bindingExpression != null && bindingExpression.Status != BindingStatus.PathError)
      {
        var propertyName = bindingExpression.ResolvedSourcePropertyName;
        if (!string.IsNullOrEmpty(propertyName))
        {
          var resolvedType = bindingExpression.ResolvedSource?.GetType();
          if (resolvedType != null)
          {
            return resolvedType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
          }
        }
      }

      return null;
    }

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static bool GetAutoWatermark(DependencyObject element) => (bool)element.GetValue(AutoWatermarkProperty);

    /// <summary>
    /// Indicates whether if the watermark is automatically retrieved by using the <see cref="DisplayAttribute"/> of the bound property.
    /// </summary>
    /// <remarks>
    /// This attached property uses reflection; thus it might reduce the performance of the application.
    /// The auto-watermark does work for the following controls:
    /// In the following case no custom watermark is shown
    /// <list type="bullet">
    /// <item>There is no binding</item>
    /// <item>Binding path errors</item>
    /// <item>Binding to a element of a collection without using a property of that element <c>Binding Path=Collection[0]</c> use: <c>Binding Path=Collection[0].SubProperty</c></item>
    /// <item>The bound property does not have a <see cref="DisplayAttribute"/></item>
    /// </list>
    /// </remarks>
    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(TimePickerBase))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    [AttachedPropertyBrowsableForType(typeof(HotKeyBox))]
    [AttachedPropertyBrowsableForType(typeof(ColorPicker))]
    public static void SetAutoWatermark(DependencyObject element, bool value) => element.SetValue(AutoWatermarkProperty, BooleanBoxes.Box(value));

    private static readonly Dictionary<Type, DependencyProperty> AutoWatermarkPropertyMapping = new()
    {
      { typeof(TextBox), TextBox.TextProperty },
      { typeof(ComboBox), Selector.SelectedItemProperty },
      { typeof(NumericUpDown), NumericUpDown.ValueProperty },
      { typeof(HotKeyBox), HotKeyBox.HotKeyProperty },
      { typeof(DatePicker), DatePicker.SelectedDateProperty },
      { typeof(TimePicker), TimePickerBase.SelectedDateTimeProperty },
      { typeof(DateTimePicker), TimePickerBase.SelectedDateTimeProperty },
      { typeof(ColorPicker), ColorPickerBase.SelectedColorProperty },
      { typeof(ColorCanvas), ColorPickerBase.SelectedColorProperty },
      { typeof(MultiSelectionComboBox), MultiSelectionComboBox.SelectedItemProperty }
    };

    internal static readonly DependencyPropertyKey TextLengthPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "TextLength",
      typeof(int),
      typeof(TextBoxHelper),
      new PropertyMetadata(0));

    public static readonly DependencyProperty TextLengthProperty = TextLengthPropertyKey.DependencyProperty;

    [Category(AppName.CrystalThemes)]
    public static int GetTextLength(UIElement element) => (int)element.GetValue(TextLengthProperty);

    public static readonly DependencyProperty HasTextProperty = DependencyProperty.RegisterAttached(
      "HasText",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    public static bool GetHasText(DependencyObject obj) => (bool)obj.GetValue(HasTextProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    [AttachedPropertyBrowsableForType(typeof(ComboBox))]
    [AttachedPropertyBrowsableForType(typeof(MultiSelectionComboBox))]
    [AttachedPropertyBrowsableForType(typeof(DatePicker))]
    [AttachedPropertyBrowsableForType(typeof(NumericUpDown))]
    public static void SetHasText(DependencyObject obj, bool value) => obj.SetValue(HasTextProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty ClearTextButtonProperty = DependencyProperty.RegisterAttached(
      "ClearTextButton",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, ButtonCommandOrClearTextChanged));

    [Category(AppName.CrystalThemes)]
    public static bool GetClearTextButton(DependencyObject d) => (bool)d.GetValue(ClearTextButtonProperty);

    public static void SetClearTextButton(DependencyObject obj, bool value) => obj.SetValue(ClearTextButtonProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty TextButtonProperty = DependencyProperty.RegisterAttached(
      "TextButton",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, ButtonCommandOrClearTextChanged));

    [Category(AppName.CrystalThemes)]
    public static bool GetTextButton(DependencyObject d) => (bool)d.GetValue(TextButtonProperty);

    public static void SetTextButton(DependencyObject obj, bool value) => obj.SetValue(TextButtonProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty ButtonsAlignmentProperty = DependencyProperty.RegisterAttached(
      "ButtonsAlignment",
      typeof(ButtonsAlignment),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(ButtonsAlignment.Right, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    [Category(AppName.CrystalThemes)]
    public static ButtonsAlignment GetButtonsAlignment(DependencyObject d) => (ButtonsAlignment)d.GetValue(ButtonsAlignmentProperty);

    public static void SetButtonsAlignment(DependencyObject obj, ButtonsAlignment value) => obj.SetValue(ButtonsAlignmentProperty, value);

    public static readonly DependencyProperty IsClearTextButtonBehaviorEnabledProperty = DependencyProperty.RegisterAttached(
      "IsClearTextButtonBehaviorEnabled",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, IsClearTextButtonBehaviorEnabledChanged));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
    public static bool GetIsClearTextButtonBehaviorEnabled(Button d) => (bool)d.GetValue(IsClearTextButtonBehaviorEnabledProperty);

    [AttachedPropertyBrowsableForType(typeof(ButtonBase))]
    public static void SetIsClearTextButtonBehaviorEnabled(Button obj, bool value) => obj.SetValue(IsClearTextButtonBehaviorEnabledProperty, BooleanBoxes.Box(value));

    /// <summary>
    /// This property can be used to set the button width (PART_ClearText) of TextBox, PasswordBox, ComboBox, NumericUpDown
    /// </summary>
    public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.RegisterAttached(
      "ButtonWidth",
      typeof(double),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(22d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    public static double GetButtonWidth(DependencyObject obj) => (double)obj.GetValue(ButtonWidthProperty);

    public static void SetButtonWidth(DependencyObject obj, double value) => obj.SetValue(ButtonWidthProperty, value);

    public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.RegisterAttached(
      "ButtonCommand",
      typeof(ICommand),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(null, ButtonCommandOrClearTextChanged));

    [Category(AppName.CrystalThemes)]
    public static ICommand? GetButtonCommand(DependencyObject d) => (ICommand?)d.GetValue(ButtonCommandProperty);

    public static void SetButtonCommand(DependencyObject obj, ICommand? value) => obj.SetValue(ButtonCommandProperty, value);

    public static readonly DependencyProperty ButtonCommandParameterProperty = DependencyProperty.RegisterAttached(
      "ButtonCommandParameter",
      typeof(object),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(null));

    [Category(AppName.CrystalThemes)]
    public static object? GetButtonCommandParameter(DependencyObject d) => d.GetValue(ButtonCommandParameterProperty);

    public static void SetButtonCommandParameter(DependencyObject obj, object? value) => obj.SetValue(ButtonCommandParameterProperty, value);

    public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.RegisterAttached(
      "ButtonContent",
      typeof(object),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata("r"));

    [Category(AppName.CrystalThemes)]
    public static object GetButtonContent(DependencyObject d) => d.GetValue(ButtonContentProperty);

    public static void SetButtonContent(DependencyObject obj, object value) => obj.SetValue(ButtonContentProperty, value);

    public static readonly DependencyProperty ButtonContentTemplateProperty = DependencyProperty.RegisterAttached(
      "ButtonContentTemplate",
      typeof(DataTemplate),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(null));


    [Category(AppName.CrystalThemes)]
    public static DataTemplate? GetButtonContentTemplate(DependencyObject d) => (DataTemplate?)d.GetValue(ButtonContentTemplateProperty);

    public static void SetButtonContentTemplate(DependencyObject obj, DataTemplate? value) => obj.SetValue(ButtonContentTemplateProperty, value);

    public static readonly DependencyProperty ButtonTemplateProperty = DependencyProperty.RegisterAttached(
      "ButtonTemplate",
      typeof(ControlTemplate),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(null));

    [Category(AppName.CrystalThemes)]
    public static ControlTemplate? GetButtonTemplate(DependencyObject d) => (ControlTemplate?)d.GetValue(ButtonTemplateProperty);

    public static void SetButtonTemplate(DependencyObject obj, ControlTemplate? value) => obj.SetValue(ButtonTemplateProperty, value);

    public static readonly DependencyProperty ButtonFontFamilyProperty = DependencyProperty.RegisterAttached(
      "ButtonFontFamily",
      typeof(FontFamily),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(new FontFamilyConverter().ConvertFromString("Marlett")));

    [Category(AppName.CrystalThemes)]
    public static FontFamily GetButtonFontFamily(DependencyObject d) => (FontFamily)d.GetValue(ButtonFontFamilyProperty);

    public static void SetButtonFontFamily(DependencyObject obj, FontFamily value) => obj.SetValue(ButtonFontFamilyProperty, value);

    public static readonly DependencyProperty ButtonFontSizeProperty = DependencyProperty.RegisterAttached(
      "ButtonFontSize",
      typeof(double),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(SystemFonts.MessageFontSize));

    [Category(AppName.CrystalThemes)]
    public static double GetButtonFontSize(DependencyObject d) => (double)d.GetValue(ButtonFontSizeProperty);

    public static void SetButtonFontSize(DependencyObject obj, double value) => obj.SetValue(ButtonFontSizeProperty, value);

    public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached(
      "SelectAllOnFocus",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

    public static bool GetSelectAllOnFocus(DependencyObject obj) => (bool)obj.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(DependencyObject obj, bool value) => obj.SetValue(SelectAllOnFocusProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty IsWaitingForDataProperty = DependencyProperty.RegisterAttached(
      "IsWaitingForData",
      typeof(bool),
      typeof(TextBoxHelper),
      new UIPropertyMetadata(BooleanBoxes.FalseBox));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    public static bool GetIsWaitingForData(DependencyObject obj) => (bool)obj.GetValue(IsWaitingForDataProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
    public static void SetIsWaitingForData(DependencyObject obj, bool value) => obj.SetValue(IsWaitingForDataProperty, BooleanBoxes.Box(value));

    public static readonly DependencyProperty IsSpellCheckContextMenuEnabledProperty = DependencyProperty.RegisterAttached(
      "IsSpellCheckContextMenuEnabled",
      typeof(bool),
      typeof(TextBoxHelper),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, IsSpellCheckContextMenuEnabledChanged));

    /// <summary>
    /// Indicates if a TextBox or RichTextBox should use SpellCheck context menu
    /// </summary>
    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    public static bool GetIsSpellCheckContextMenuEnabled(UIElement element) => (bool)element.GetValue(IsSpellCheckContextMenuEnabledProperty);

    [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
    public static void SetIsSpellCheckContextMenuEnabled(UIElement element, bool value) => element.SetValue(IsSpellCheckContextMenuEnabledProperty, BooleanBoxes.Box(value));

    private static void IsSpellCheckContextMenuEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is TextBoxBase textBox))
      {
        throw new InvalidOperationException("The property 'IsSpellCheckContextMenuEnabled' may only be set on TextBoxBase elements.");
      }

      if (e.OldValue != e.NewValue)
      {
        textBox.SetCurrentValue(SpellCheck.IsEnabledProperty, BooleanBoxes.Box((bool)e.NewValue));
        if ((bool)e.NewValue)
        {
          textBox.ContextMenuOpening += TextBoxBaseContextMenuOpening;
          textBox.LostFocus += TextBoxBaseLostFocus;
          textBox.ContextMenuClosing += TextBoxBaseContextMenuClosing;
        }
        else
        {
          textBox.ContextMenuOpening -= TextBoxBaseContextMenuOpening;
          textBox.LostFocus -= TextBoxBaseLostFocus;
          textBox.ContextMenuClosing -= TextBoxBaseContextMenuClosing;
        }
      }
    }

    private static void TextBoxBaseLostFocus(object sender, RoutedEventArgs e)
    {
      RemoveSpellCheckMenuItems((sender as FrameworkElement)?.ContextMenu);
    }

    private static void TextBoxBaseContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
      RemoveSpellCheckMenuItems((sender as FrameworkElement)?.ContextMenu);
    }

    private static void TextBoxBaseContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
      var tbBase = (TextBoxBase)sender;
      var contextMenu = tbBase.ContextMenu;
      if (contextMenu is null)
      {
        return;
      }

      RemoveSpellCheckMenuItems(contextMenu);

      if (!SpellCheck.GetIsEnabled(tbBase))
      {
        return;
      }

      var spellingError = tbBase switch
      {
        TextBox textBox => textBox.GetSpellingError(textBox.CaretIndex),
        RichTextBox richTextBox => richTextBox.GetSpellingError(richTextBox.CaretPosition),
        _ => null
      };

      if (spellingError != null)
      {
        var itemIndex = 0;

        var spellingSuggestionStyle = contextMenu.TryFindResource(Spelling.SuggestionMenuItemStyleKey) as Style;
        var suggestions = spellingError.Suggestions.ToList();
        if (suggestions.Any())
        {
          foreach (var suggestion in suggestions)
          {
            contextMenu.Items.Insert(itemIndex++, new MenuItem
            {
              Command = EditingCommands.CorrectSpellingError,
              CommandParameter = suggestion,
              CommandTarget = tbBase,
              Style = spellingSuggestionStyle,
              Tag = typeof(Spelling)
            });
          }
        }
        else
        {
          contextMenu.Items.Insert(itemIndex++, new MenuItem
          {
            Style = contextMenu.TryFindResource(Spelling.NoSuggestionsMenuItemStyleKey) as Style,
            Tag = typeof(Spelling)
          });
        }

        // add a separator
        contextMenu.Items.Insert(itemIndex++, new Separator
        {
          Style = contextMenu.TryFindResource(Spelling.SeparatorStyleKey) as Style,
          Tag = typeof(Spelling)
        });

        // ignore all
        contextMenu.Items.Insert(itemIndex++, new MenuItem
        {
          Command = EditingCommands.IgnoreSpellingError,
          CommandTarget = tbBase,
          Style = contextMenu.TryFindResource(Spelling.IgnoreAllMenuItemStyleKey) as Style,
          Tag = typeof(Spelling)
        });

        // add another separator
        contextMenu.Items.Insert(itemIndex, new Separator
        {
          Style = contextMenu.TryFindResource(Spelling.SeparatorStyleKey) as Style,
          Tag = typeof(Spelling)
        });
      }
    }

    private static void RemoveSpellCheckMenuItems(ContextMenu? contextMenu)
    {
      if (contextMenu != null)
      {
        var spellCheckItems = contextMenu.Items.OfType<FrameworkElement>().Where(item => ReferenceEquals(item.Tag, typeof(Spelling))).ToList();
        foreach (var item in spellCheckItems)
        {
          contextMenu.Items.Remove(item);
        }
      }
    }

    private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is RichTextBox richTextBox)
      {
        if ((bool)e.NewValue)
        {
          // Fixes #1343 and #2514: also triggers the show of the floating watermark if necessary
          richTextBox.BeginInvoke(() => RichTextBox_TextChanged(richTextBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None)));

          richTextBox.TextChanged += RichTextBox_TextChanged;
          richTextBox.GotFocus += TextBoxGotFocus;
          richTextBox.PreviewMouseLeftButtonDown += UIElementPreviewMouseLeftButtonDown;
        }
        else
        {
          richTextBox.TextChanged -= RichTextBox_TextChanged;
          richTextBox.GotFocus -= TextBoxGotFocus;
          richTextBox.PreviewMouseLeftButtonDown -= UIElementPreviewMouseLeftButtonDown;
        }
      }
      else if (d is TextBox textBox)
      {
        if ((bool)e.NewValue)
        {
          // Fixes #1343 and #2514: also triggers the show of the floating watermark if necessary
          textBox.BeginInvoke(() => TextChanged(textBox, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None)));

          textBox.TextChanged += TextChanged;
          textBox.GotFocus += TextBoxGotFocus;
          textBox.PreviewMouseLeftButtonDown += UIElementPreviewMouseLeftButtonDown;
        }
        else
        {
          textBox.TextChanged -= TextChanged;
          textBox.GotFocus -= TextBoxGotFocus;
          textBox.PreviewMouseLeftButtonDown -= UIElementPreviewMouseLeftButtonDown;
        }
      }
      else if (d is PasswordBox passBox)
      {
        if ((bool)e.NewValue)
        {
          // Fixes #1343 and #2514: also triggers the show of the floating watermark if necessary
          passBox.BeginInvoke(() => PasswordChanged(passBox, new RoutedEventArgs(PasswordBox.PasswordChangedEvent, passBox)));

          passBox.PasswordChanged += PasswordChanged;
          passBox.GotFocus += PasswordGotFocus;
          passBox.PreviewMouseLeftButtonDown += UIElementPreviewMouseLeftButtonDown;
        }
        else
        {
          passBox.PasswordChanged -= PasswordChanged;
          passBox.GotFocus -= PasswordGotFocus;
          passBox.PreviewMouseLeftButtonDown -= UIElementPreviewMouseLeftButtonDown;
        }
      }
      else if (d is NumericUpDown numericUpDown)
      {
        if ((bool)e.NewValue)
        {
          // Fixes #1343 and #2514: also triggers the show of the floating watermark if necessary
          numericUpDown.BeginInvoke(() => OnNumericUpDownValueChanged(numericUpDown, new RoutedEventArgs(NumericUpDown.ValueChangedEvent, numericUpDown)));

          numericUpDown.ValueChanged += OnNumericUpDownValueChanged;
        }
        else
        {
          numericUpDown.ValueChanged -= OnNumericUpDownValueChanged;
        }
      }
      else if (d is TimePickerBase timePicker)
      {
        if ((bool)e.NewValue)
        {
          timePicker.SelectedDateTimeChanged += OnTimePickerBaseSelectedDateTimeChanged;
        }
        else
        {
          timePicker.SelectedDateTimeChanged -= OnTimePickerBaseSelectedDateTimeChanged;
        }
      }
      else if (d is DatePicker datePicker)
      {
        if ((bool)e.NewValue)
        {
          datePicker.SelectedDateChanged += OnDatePickerBaseSelectedDateChanged;
        }
        else
        {
          datePicker.SelectedDateChanged -= OnDatePickerBaseSelectedDateChanged;
        }
      }
    }

    private static void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (sender is RichTextBox richTextBox)
      {
        SetRichTextBoxTextLength(richTextBox);
      }
    }

    private static void SetTextLength<TDependencyObject>(TDependencyObject? sender, Func<TDependencyObject, int> funcTextLength)
        where TDependencyObject : DependencyObject
    {
      if (sender != null)
      {
        var value = funcTextLength(sender);
        sender.SetValue(TextLengthPropertyKey, value);
        sender.SetCurrentValue(HasTextProperty, BooleanBoxes.Box(value > 0));
      }
    }

    private static void TextChanged(object sender, RoutedEventArgs e)
      => SetTextLength(sender as TextBox, textBox => textBox.Text.Length);

    private static void OnNumericUpDownValueChanged(object sender, RoutedEventArgs e)
      => SetTextLength(sender as NumericUpDown, numericUpDown => numericUpDown.Value.HasValue ? 1 : 0);

    private static void PasswordChanged(object sender, RoutedEventArgs e)
      => SetTextLength(sender as PasswordBox, passwordBox => passwordBox.Password.Length);

    private static void OnDatePickerBaseSelectedDateChanged(object? sender, RoutedEventArgs e)
      => SetTextLength(sender as DatePicker, timePickerBase => timePickerBase.SelectedDate.HasValue ? 1 : 0);

    private static void OnTimePickerBaseSelectedDateTimeChanged(object? sender, RoutedEventArgs e)
      => SetTextLength(sender as TimePickerBase, timePickerBase => timePickerBase.SelectedDateTime.HasValue ? 1 : 0);

    private static void TextBoxGotFocus(object? sender, RoutedEventArgs e)
      => ControlGotFocus(sender as TextBoxBase, textBox => textBox.SelectAll());

    private static void UIElementPreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
      if (sender is UIElement uiElement && !uiElement.IsKeyboardFocusWithin && GetSelectAllOnFocus(uiElement))
      {
        // always SelectAllOnFocus
        uiElement.Focus();
        e.Handled = true;
      }
    }

    private static void PasswordGotFocus(object sender, RoutedEventArgs e)
      => ControlGotFocus(sender as PasswordBox, passwordBox => passwordBox?.SelectAll());

    private static void ControlGotFocus<TDependencyObject>(TDependencyObject? sender, Action<TDependencyObject> action)
        where TDependencyObject : DependencyObject
    {
      if (sender != null)
      {
        if (GetSelectAllOnFocus(sender))
        {
          sender.Dispatcher.BeginInvoke(action, sender);
        }
      }
    }

    private static void IsClearTextButtonBehaviorEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && d is Button button)
      {
        button.Click -= ButtonClicked;
        if ((bool)e.NewValue)
        {
          button.Click += ButtonClicked;
        }
      }
    }

    public static void ButtonClicked(object sender, RoutedEventArgs e)
    {
      var button = (Button)sender;

      var parent = button.GetAncestors().FirstOrDefault(a => a is RichTextBox || a is TextBox || a is PasswordBox || a is ComboBox || a is ColorPickerBase || a is MultiSelectionComboBox);
      if (parent is null)
      {
        return;
      }

      var command = GetButtonCommand(parent);
      var commandParameter = GetButtonCommandParameter(parent) ?? parent;
      if (command != null && command.CanExecute(commandParameter))
      {
        if (parent is TextBox textBox)
        {
          textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        command.Execute(commandParameter);
      }

      if (GetClearTextButton(parent))
      {
        if (parent is RichTextBox richTextBox)
        {
          richTextBox.Document?.Blocks?.Clear();
          richTextBox.Selection?.Select(richTextBox.CaretPosition, richTextBox.CaretPosition);
        }
        else if (parent is TextBox textBox)
        {
          textBox.Clear();
          textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
        else if (parent is PasswordBox passwordBox)
        {
          passwordBox.Clear();
          passwordBox.GetBindingExpression(PasswordBoxBindingBehavior.PasswordProperty)?.UpdateSource();
        }
        else if (parent is MultiSelectionComboBox multiSelectionComboBox)
        {
          if (multiSelectionComboBox.HasCustomText)
          {
            multiSelectionComboBox.ResetEditableText(true);
          }
          else
          {
            switch (multiSelectionComboBox.SelectionMode)
            {
              case SelectionMode.Single:
                multiSelectionComboBox.SetCurrentValue(MultiSelectionComboBox.SelectedItemProperty, null);
                break;
              case SelectionMode.Multiple:
              case SelectionMode.Extended:
                multiSelectionComboBox.SelectedItems?.Clear();
                break;
              default:
                throw new NotSupportedException("Unknown SelectionMode");
            }
            multiSelectionComboBox.ResetEditableText(true);
          }
        }
        else if (parent is ComboBox comboBox)
        {
          if (comboBox.IsEditable)
          {
            comboBox.SetCurrentValue(ComboBox.TextProperty, string.Empty);
            comboBox.GetBindingExpression(ComboBox.TextProperty)?.UpdateSource();
          }

          comboBox.SetCurrentValue(ComboBox.SelectedItemProperty, null);
          comboBox.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateSource();
        }
        else if (parent is ColorPickerBase colorPicker)
        {
          colorPicker.SetCurrentValue(ColorPickerBase.SelectedColorProperty, null);
        }
      }
    }

    private static void ButtonCommandOrClearTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is RichTextBox richTextBox)
      {
        richTextBox.Loaded -= RichTextBoxLoaded;
        richTextBox.Loaded += RichTextBoxLoaded;
        if (richTextBox.IsLoaded)
        {
          RichTextBoxLoaded(richTextBox, new RoutedEventArgs());
        }
      }
      else if (d is TextBox textBox)
      {
        // only one loaded event
        textBox.Loaded -= TextChanged;
        textBox.Loaded += TextChanged;
        if (textBox.IsLoaded)
        {
          TextChanged(textBox, new RoutedEventArgs());
        }
      }
      else if (d is PasswordBox passwordBox)
      {
        // only one loaded event
        passwordBox.Loaded -= PasswordChanged;
        passwordBox.Loaded += PasswordChanged;
        if (passwordBox.IsLoaded)
        {
          PasswordChanged(passwordBox, new RoutedEventArgs());
        }
      }
      else if (d is ComboBox comboBox)
      {
        // only one loaded event
        comboBox.Loaded -= ComboBoxLoaded;
        comboBox.Loaded += ComboBoxLoaded;
        if (comboBox.IsLoaded)
        {
          ComboBoxLoaded(comboBox, new RoutedEventArgs());
        }
      }
    }

    private static void RichTextBoxLoaded(object sender, RoutedEventArgs e)
    {
      if (sender is RichTextBox richTextBox)
      {
        SetRichTextBoxTextLength(richTextBox);
      }
    }

    private static void SetRichTextBoxTextLength(RichTextBox richTextBox)
    {
      SetTextLength(richTextBox, rtb =>
          {
            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            var text = textRange.Text;
            var lastIndexOfNewLine = text.LastIndexOf(Environment.NewLine, StringComparison.InvariantCulture);
            if (lastIndexOfNewLine >= 0)
            {
              text = text.Remove(lastIndexOfNewLine);
            }

            return text.Length;
          });
    }

    private static void ComboBoxLoaded(object sender, RoutedEventArgs e)
    {
      if (sender is ComboBox comboBox)
      {
        comboBox.SetCurrentValue(HasTextProperty, BooleanBoxes.Box(!string.IsNullOrWhiteSpace(comboBox.Text) || comboBox.SelectedItem != null));
      }
    }
  }
}