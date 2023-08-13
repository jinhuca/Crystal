namespace Crystal.Themes.Controls;

public class Tile : Button
{
  /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
  public static readonly DependencyProperty TitleProperty
    = DependencyProperty.Register(nameof(Title),
      typeof(string),
      typeof(Tile),
      new PropertyMetadata(default(string)));

  /// <summary>
  /// Gets or sets the title of the <see cref="Tile"/>.
  /// </summary>
  public string? Title
  {
    get => (string?)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  /// <summary>Identifies the <see cref="HorizontalTitleAlignment"/> dependency property.</summary>
  public static readonly DependencyProperty HorizontalTitleAlignmentProperty =
    DependencyProperty.Register(nameof(HorizontalTitleAlignment),
      typeof(HorizontalAlignment),
      typeof(Tile),
      new FrameworkPropertyMetadata(HorizontalAlignment.Left));

  /// <summary> 
  /// Gets or sets the horizontal alignment of the <see cref="Title"/>.
  /// </summary> 
  [Bindable(true), Category("Layout")]
  public HorizontalAlignment HorizontalTitleAlignment
  {
    get => (HorizontalAlignment)GetValue(HorizontalTitleAlignmentProperty);
    set => SetValue(HorizontalTitleAlignmentProperty, value);
  }

  /// <summary>Identifies the <see cref="VerticalTitleAlignment"/> dependency property.</summary> 
  public static readonly DependencyProperty VerticalTitleAlignmentProperty =
    DependencyProperty.Register(nameof(VerticalTitleAlignment),
      typeof(VerticalAlignment),
      typeof(Tile),
      new FrameworkPropertyMetadata(VerticalAlignment.Bottom));

  /// <summary>
  /// Gets or sets the vertical alignment of the <see cref="Title"/>.
  /// </summary>
  [Bindable(true), Category("Layout")]
  public VerticalAlignment VerticalTitleAlignment
  {
    get => (VerticalAlignment)GetValue(VerticalTitleAlignmentProperty);
    set => SetValue(VerticalTitleAlignmentProperty, value);
  }

  /// <summary>Identifies the <see cref="Count"/> dependency property.</summary>
  public static readonly DependencyProperty CountProperty
    = DependencyProperty.Register(nameof(Count),
      typeof(string),
      typeof(Tile),
      new PropertyMetadata(default(string)));

  /// <summary>
  /// Gets or sets a Count text.
  /// </summary>
  public string? Count
  {
    get => (string?)GetValue(CountProperty);
    set => SetValue(CountProperty, value);
  }

  /// <summary>Identifies the <see cref="TitleFontSize"/> dependency property.</summary>
  public static readonly DependencyProperty TitleFontSizeProperty
    = DependencyProperty.Register(nameof(TitleFontSize),
      typeof(double),
      typeof(Tile),
      new PropertyMetadata(16d));

  /// <summary>
  /// Gets or sets the font size of the <see cref="Title"/>.
  /// </summary>
  public double TitleFontSize
  {
    get => (double)GetValue(TitleFontSizeProperty);
    set => SetValue(TitleFontSizeProperty, value);
  }

  /// <summary>Identifies the <see cref="CountFontSize"/> dependency property.</summary>
  public static readonly DependencyProperty CountFontSizeProperty
    = DependencyProperty.Register(nameof(CountFontSize),
      typeof(double),
      typeof(Tile),
      new PropertyMetadata(28d));

  /// Gets or sets the font size of the <see cref="Count"/>.
  public double CountFontSize
  {
    get => (double)GetValue(CountFontSizeProperty);
    set => SetValue(CountFontSizeProperty, value);
  }

  static Tile()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));
  }
}