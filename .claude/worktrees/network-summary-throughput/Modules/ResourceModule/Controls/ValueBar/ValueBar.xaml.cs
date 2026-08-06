using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ResourceModule.Controls.Indicator; 

public partial class ValueBar : UserControl {
  #region constants

  private const double DefaultFontSize = 14.0;

  #endregion constants

  public ValueBar() {
    InitializeComponent();
  }

  #region Title Dependency Properties

  #region Text

  public string Title {
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
    nameof(Title),
    typeof(string),
    typeof(ValueBar),
    new PropertyMetadata(string.Empty));

  #endregion Text

  #region FontSize

  [TypeConverter(typeof(FontSizeConverter))]
  [Localizability(LocalizationCategory.None)]
  public double TitleFontSize {
    get => (double)GetValue(TitleFontSizeProperty);
    set => SetValue(TitleFontSizeProperty, value);
  }

  public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
    nameof(TitleFontSize),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(DefaultFontSize));

  #endregion FontSize

  #region FontWeight

  public FontWeight TitleFontWeight {
    get => (FontWeight)GetValue(TitleFontWeightProperty);
    set => SetValue(TitleFontWeightProperty, value);
  }

  public static readonly DependencyProperty TitleFontWeightProperty = DependencyProperty.Register(
    nameof(TitleFontWeight),
    typeof(FontWeight),
    typeof(ValueBar),
    new PropertyMetadata(FontWeights.Regular));

  #endregion FontWeight

  #region Foreground

  public Brush TitleForeground {
    get => (Brush)GetValue(TitleForegroundProperty);
    set => SetValue(TitleForegroundProperty, value);
  }

  public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(
    nameof(TitleForeground),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Black));

  #endregion Foreground

  #endregion Title Dependency Properties
  
  #region Indicator Bar Properties

  #region Height

  [TypeConverter(typeof(LengthConverter))]
  [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
  public double IndicatorHeight {
    get => (double)GetValue(IndicatorHeightProperty);
    set => SetValue(IndicatorHeightProperty, value);
  }

  public static readonly DependencyProperty IndicatorHeightProperty = DependencyProperty.Register(
    nameof(IndicatorHeight),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(10.0));

  #endregion Height

  #region Weight

  [TypeConverter(typeof(LengthConverter))]
  [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
  public double IndicatorWidth {
    get => (double)GetValue(IndicatorWidthProperty);
    set => SetValue(IndicatorWidthProperty, value);
  }

  public static readonly DependencyProperty IndicatorWidthProperty = DependencyProperty.Register(
    nameof(IndicatorWidth),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(120.0));

  #endregion Weight

  #region Brush

  public Brush IndicatorBrush {
    get => (Brush)GetValue(IndicatorBrushProperty);
    set => SetValue(IndicatorBrushProperty, value);
  }

  public static readonly DependencyProperty IndicatorBrushProperty = DependencyProperty.Register(
    nameof(IndicatorBrush),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Green));

  #endregion Brush

  #endregion Indicator Bar Properties

  #region Value DependencyProperties

  #region Value

  private const double DefaultMinValue = 0, DefaultMaxValue = 100;

  public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
    nameof(Value),
    typeof(double),
    typeof(ValueBar),
    new FrameworkPropertyMetadata(
      DefaultMinValue,
      new PropertyChangedCallback(OnValueChanged)));

  public double Value {
    get => (double)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
  }

  private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
    var control = (ValueBar)obj;
    RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
        (double)args.OldValue, (double)args.NewValue, ValueChangedEvent);
    control.OnValueChanged(e);
  }

  public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
    nameof(ValueChanged),
    RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<double>),
    typeof(ValueBar));

  public event RoutedPropertyChangedEventHandler<double> ValueChanged {
    add { AddHandler(ValueChangedEvent, value); }
    remove { RemoveHandler(ValueChangedEvent, value); }
  }

  protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<double> args) {
    RaiseEvent(args);
  }

  #endregion Value Text

  #region FontSize

  [TypeConverter(typeof(FontSizeConverter))]
  [Localizability(LocalizationCategory.None)]
  public double ValueFontSize {
    get => (double)GetValue(ValueFontSizeProperty);
    set => SetValue(ValueFontSizeProperty, value);
  }

  public static readonly DependencyProperty ValueFontSizeProperty = DependencyProperty.Register(
    nameof(ValueFontSize),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(DefaultFontSize));

  #endregion FontSize

  #region FontFamily

  [Bindable(true)]
  [Category("Appearance")]
  [Localizability(LocalizationCategory.Font)]
  public FontFamily ValueFontFamily {
    get => (FontFamily)GetValue(ValueFontFamilyProperty);
    set => SetValue(ValueFontFamilyProperty, value);
  }

  public static readonly DependencyProperty ValueFontFamilyProperty = DependencyProperty.Register(
    nameof(ValueFontFamily),
    typeof(FontFamily),
    typeof(ValueBar),
    new PropertyMetadata(new FontFamily("Segoe UI")));

  #endregion FontFamily

  #region FontWeight

  public FontWeight ValueFontWeight {
    get => (FontWeight)GetValue(ValueFontWeightProperty);
    set => SetValue(ValueFontWeightProperty, value);
  }

  public static readonly DependencyProperty ValueFontWeightProperty = DependencyProperty.Register(
    nameof(ValueFontWeight),
    typeof(FontWeight),
    typeof(ValueBar),
    new PropertyMetadata(FontWeights.Regular));

  #endregion FontWeight

  #region Foreground

  public Brush ValueForeground {
    get => (Brush)GetValue(ValueForegroundProperty);
    set => SetValue(ValueForegroundProperty, value);
  }

  public static readonly DependencyProperty ValueForegroundProperty = DependencyProperty.Register(
    nameof(ValueForeground),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Black));

  #endregion Foreground

  #endregion Value Text
  
  #region Minimum DependencyProperty

  [Bindable(true)]
  [Category("Behavior")]
  public double Minimum {
    get => (double)GetValue(MinimumProperty);
    set => SetValue(MinimumProperty, value);
  }

  public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
    nameof(Minimum),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(0.0));

  #endregion Minimum DependencyProperty

  #region Maximum DependencyProperty

  [Bindable(true)]
  [Category("Behavior")]
  public double Maximum {
    get => (double)GetValue(MaximumProperty);
    set => SetValue(MaximumProperty, value);
  }

  public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
    nameof(Maximum),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(0.0));

  #endregion Maximum DependencyProperty

  #region Separator

  #region Width

  [TypeConverter(typeof(LengthConverter))]
  [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
  public double SeparatorWidth {
    get => (double)GetValue(SeparatorWidthProperty);
    set => SetValue(SeparatorWidthProperty, value);
  }

  public static readonly DependencyProperty SeparatorWidthProperty = DependencyProperty.Register(
    nameof(SeparatorWidth),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(1.0));

  #endregion Width

  #region Margin

  public Thickness SeparatorMargin {
    get => (Thickness)GetValue(SeparatorMarginProperty);
    set => SetValue(SeparatorMarginProperty, value);
  }

  public static readonly DependencyProperty SeparatorMarginProperty = DependencyProperty.Register(
    nameof(SeparatorMargin),
    typeof(Thickness),
    typeof(ValueBar),
    new PropertyMetadata(new Thickness(0)));

  #endregion Margin

  #region Brush

  public Brush SeparatorBrush {
    get => (Brush)GetValue(SeparatorBrushProperty);
    set => SetValue(SeparatorBrushProperty, value);
  }

  public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(
    nameof(SeparatorBrush),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.LightGray));

  #endregion Brush

  #endregion Separator

  #region Min Dependency Properties

  #region Title FontSize

  [TypeConverter(typeof(FontSizeConverter))]
  [Localizability(LocalizationCategory.None)]
  public double MinTitleFontSize {
    get => (double)GetValue(MinTitleFontSizeProperty);
    set => SetValue(MinTitleFontSizeProperty, value);
  }

  public static readonly DependencyProperty MinTitleFontSizeProperty = DependencyProperty.Register(
    nameof(MinTitleFontSize),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(DefaultFontSize));

  #endregion Title FontSize

  #region Title FontWeight

  public FontWeight MinTitleFontWeight {
    get => (FontWeight)GetValue(MinTitleFontWeightProperty);
    set => SetValue(MinTitleFontWeightProperty, value);
  }

  public static readonly DependencyProperty MinTitleFontWeightProperty = DependencyProperty.Register(
    nameof(MinTitleFontWeight),
    typeof(FontWeight),
    typeof(ValueBar),
    new PropertyMetadata(FontWeights.Regular));

  #endregion Title FontWeight

  #region Title Brush

  public Brush MinTitleBrush {
    get => (Brush)GetValue(MinTitleBrushProperty);
    set => SetValue(MinTitleBrushProperty, value);
  }

  public static readonly DependencyProperty MinTitleBrushProperty = DependencyProperty.Register(
    nameof(MinTitleBrush),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Black));

  #endregion Title Brush

  [TypeConverter(typeof(FontSizeConverter))]
  [Localizability(LocalizationCategory.None)]
  public double MinValueFontSize {
    get => (double)GetValue(MinValueFontSizeProperty);
    set => SetValue(MinValueFontSizeProperty, value);
  }

  public static readonly DependencyProperty MinValueFontSizeProperty = DependencyProperty.Register(
    nameof(MinValueFontSize),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(DefaultFontSize));

  public Brush MinValueBrush {
    get => (Brush)GetValue(MinValueBrushProperty);
    set => SetValue(MinValueBrushProperty, value);
  }

  public static readonly DependencyProperty MinValueBrushProperty = DependencyProperty.Register(
    nameof(MinValueBrush),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Black));

  public FontWeight MinValueFontWeight {
    get => (FontWeight)GetValue(MinValueFontWeightProperty);
    set => SetValue(MinValueFontWeightProperty, value);
  }

  public static readonly DependencyProperty MinValueFontWeightProperty = DependencyProperty.Register(
    nameof(MinValueFontWeight),
    typeof(FontWeight),
    typeof(ValueBar),
    new PropertyMetadata(FontWeights.Regular));

  #endregion Min Dependency Properties

  #region Max Dependency Properties

  #region Title FontSize

  [TypeConverter(typeof(FontSizeConverter))]
  [Localizability(LocalizationCategory.None)]
  public double MaxTitleFontSize {
    get => (double)GetValue(MaxTitleFontSizeProperty);
    set => SetValue(MaxTitleFontSizeProperty, value);
  }

  public static readonly DependencyProperty MaxTitleFontSizeProperty = DependencyProperty.Register(
    nameof(MaxTitleFontSize),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(DefaultFontSize));

  #endregion Title FontSize

  #region Title FontWeight

  public FontWeight MaxTitleFontWeight {
    get => (FontWeight)GetValue(MaxTitleFontWeightProperty);
    set => SetValue(MaxTitleFontWeightProperty, value);
  }

  public static readonly DependencyProperty MaxTitleFontWeightProperty = DependencyProperty.Register(
    nameof(MaxTitleFontWeight),
    typeof(FontWeight),
    typeof(ValueBar),
    new PropertyMetadata(FontWeights.Regular));

  #endregion Title FontWeight

  #region Title Brush

  public Brush MaxTitleBrush {
    get => (Brush)GetValue(MaxTitleBrushProperty);
    set => SetValue(MaxTitleBrushProperty, value);
  }

  public static readonly DependencyProperty MaxTitleBrushProperty = DependencyProperty.Register(
    nameof(MaxTitleBrush),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Black));

  #endregion Title Brush

  [TypeConverter(typeof(FontSizeConverter))]
  [Localizability(LocalizationCategory.None)]
  public double MaxValueFontSize {
    get => (double)GetValue(MaxValueFontSizeProperty);
    set => SetValue(MaxValueFontSizeProperty, value);
  }

  public static readonly DependencyProperty MaxValueFontSizeProperty = DependencyProperty.Register(
    nameof(MaxValueFontSize),
    typeof(double),
    typeof(ValueBar),
    new PropertyMetadata(DefaultFontSize));

  public Brush MaxValueBrush {
    get => (Brush)GetValue(MaxValueBrushProperty);
    set => SetValue(MaxValueBrushProperty, value);
  }

  public static readonly DependencyProperty MaxValueBrushProperty = DependencyProperty.Register(
    nameof(MaxValueBrush),
    typeof(Brush),
    typeof(ValueBar),
    new PropertyMetadata(Brushes.Black));

  public FontWeight MaxValueFontWeight {
    get => (FontWeight)GetValue(MaxValueFontWeightProperty);
    set => SetValue(MaxValueFontWeightProperty, value);
  }

  public static readonly DependencyProperty MaxValueFontWeightProperty = DependencyProperty.Register(
    nameof(MaxValueFontWeight),
    typeof(FontWeight),
    typeof(ValueBar),
    new PropertyMetadata(FontWeights.Regular));

  #endregion Max Dependency Properties
}
