using Crystal.Themes.Controls.Helper;

namespace Crystal.Themes.Controls;

[TemplatePart(Name = nameof(PART_PopupListBox), Type = typeof(ListBox))]
[TemplatePart(Name = nameof(PART_Popup), Type = typeof(Popup))]
[TemplatePart(Name = nameof(PART_SelectedItemsPresenter), Type = typeof(ListBox))]
[StyleTypedProperty(Property = nameof(SelectedItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
public class MultiSelectionComboBox : ComboBox
{
  #region Constructors

  static MultiSelectionComboBox()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectionComboBox), new FrameworkPropertyMetadata(typeof(MultiSelectionComboBox)));
    TextProperty.OverrideMetadata(typeof(MultiSelectionComboBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnTextChanged));
    CommandManager.RegisterClassCommandBinding(typeof(MultiSelectionComboBox), new CommandBinding(ClearContentCommand, ExecutedClearContentCommand, CanExecuteClearContentCommand));
    CommandManager.RegisterClassCommandBinding(typeof(MultiSelectionComboBox), new CommandBinding(RemoveItemCommand, RemoveItemCommand_Executed, RemoveItemCommand_CanExecute));
    CommandManager.RegisterClassCommandBinding(typeof(MultiSelectionComboBox), new CommandBinding(SelectAllCommand, OnSelectAll, OnQueryStatusSelectAll));
  }

  public MultiSelectionComboBox()
  {
    var selectedItemsImpl = new ObservableCollection<object>();
    SetValue(SelectedItemsPropertyKey, selectedItemsImpl);

    selectedItemsImpl.CollectionChanged += SelectedItemsImpl_CollectionChanged;
  }

  #endregion

  //-------------------------------------------------------------------
  //
  //  Private Members
  // 
  //-------------------------------------------------------------------

  #region private Members

  private Popup? PART_Popup;
  private ListBox? PART_PopupListBox;
  private TextBox? PART_EditableTextBox;
  private ListBox? PART_SelectedItemsPresenter;

  private bool isUserdefinedTextInputPending;
  private bool isTextChanging; // This flag indicates if the text is changed by us, so we don't want to re-fire the TextChangedEvent.
  private bool shouldDoTextReset; // Defines if the Text should be reset after selecting items from string
  private bool shouldAddItems; // Defines if the MultiSelectionComboBox should add new items from text input. Don't set this to true while input is pending. We cannot know how long the user needs for typing.
  private bool isSyncingSelectedItems; // true if syncing in one or the other direction already running
  private DispatcherTimer? updateSelectedItemsFromTextTimer;
  private static readonly RoutedUICommand SelectAllCommand
    = new RoutedUICommand("Select All",
      nameof(SelectAllCommand),
      typeof(MultiSelectionComboBox),
      new InputGestureCollection() { new KeyGesture(Key.A, ModifierKeys.Control) });

  #endregion

  //-------------------------------------------------------------------
  //
  //  Public Properties
  // 
  //-------------------------------------------------------------------

  #region Public Properties

  /// <summary>Identifies the <see cref="SelectionMode"/> dependency property.</summary>
  public static readonly DependencyProperty SelectionModeProperty
    = DependencyProperty.Register(nameof(SelectionMode),
      typeof(SelectionMode),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(SelectionMode.Single),
      o =>
      {
        var value = (SelectionMode)o;
        return value == SelectionMode.Single
               || value == SelectionMode.Multiple
               || value == SelectionMode.Extended;
      });

  /// <summary>
  ///     Indicates the selection behavior for the ListBox.
  /// </summary>
  public SelectionMode SelectionMode
  {
    get => (SelectionMode)GetValue(SelectionModeProperty);
    set => SetValue(SelectionModeProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItem"/> dependency property.</summary>
  public new static readonly DependencyProperty SelectedItemProperty
    = DependencyProperty.Register(nameof(SelectedItem),
      typeof(object),
      typeof(MultiSelectionComboBox),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

  /// <summary>
  /// Gets or Sets the selectedItem
  /// </summary>
  public new object? SelectedItem
  {
    get => (object?)GetValue(SelectedItemProperty);
    set => SetValue(SelectedItemProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedIndex"/> dependency property.</summary>
  public new static readonly DependencyProperty SelectedIndexProperty
    = DependencyProperty.Register(nameof(SelectedIndex),
      typeof(int),
      typeof(MultiSelectionComboBox),
      new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

  /// <summary>
  /// Gets or Sets the SelectedIndex
  /// </summary>
  public new int SelectedIndex
  {
    get => (int)GetValue(SelectedIndexProperty);
    set => SetValue(SelectedIndexProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedValue"/> dependency property.</summary>
  public new static readonly DependencyProperty SelectedValueProperty
    = DependencyProperty.Register(nameof(SelectedValue),
      typeof(object),
      typeof(MultiSelectionComboBox),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

  /// <summary>
  /// Gets or Sets the SelectedValue
  /// </summary>
  public new object? SelectedValue
  {
    get => (object?)GetValue(SelectedValueProperty);
    set => SetValue(SelectedValueProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItems"/> dependency property.</summary>
  internal static readonly DependencyPropertyKey SelectedItemsPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(SelectedItems),
      typeof(IList),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>Identifies the <see cref="SelectedItems"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

  /// <summary>
  /// The currently selected items.
  /// </summary>
  [Bindable(true), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IList? SelectedItems
  {
    get => (IList?)GetValue(SelectedItemsProperty);
    protected set => SetValue(SelectedItemsPropertyKey, value);
  }

  /// <summary>Identifies the <see cref="DisplaySelectedItems"/> dependency property.</summary>
  internal static readonly DependencyPropertyKey DisplaySelectedItemsPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(DisplaySelectedItems),
      typeof(IEnumerable),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>Identifies the <see cref="DisplaySelectedItems"/> dependency property.</summary>
  public static readonly DependencyProperty DisplaySelectedItemsProperty = DisplaySelectedItemsPropertyKey.DependencyProperty;

  /// <summary>
  /// Gets the <see cref="SelectedItems"/> in the specified order which was set via <see cref="OrderSelectedItemsBy"/>
  /// </summary>
  public IEnumerable? DisplaySelectedItems
  {
    get => (IEnumerable?)GetValue(DisplaySelectedItemsProperty);
    protected set => SetValue(DisplaySelectedItemsPropertyKey, value);
  }

  /// <summary>Identifies the <see cref="OrderSelectedItemsBy"/> dependency property.</summary>
  public static readonly DependencyProperty OrderSelectedItemsByProperty
    = DependencyProperty.Register(nameof(OrderSelectedItemsBy),
      typeof(SelectedItemsOrderType),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(SelectedItemsOrderType.SelectedOrder, OnOrderSelectedItemsByChanged));

  /// <summary>
  /// Gets or sets how the <see cref="SelectedItems"/> should be sorted
  /// </summary>
  public SelectedItemsOrderType OrderSelectedItemsBy
  {
    get => (SelectedItemsOrderType)GetValue(OrderSelectedItemsByProperty);
    set => SetValue(OrderSelectedItemsByProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItemContainerStyle"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemContainerStyleProperty
    = DependencyProperty.Register(nameof(SelectedItemContainerStyle),
      typeof(Style),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="Style"/> for the <see cref="SelectedItems"/>
  /// </summary>
  public Style? SelectedItemContainerStyle
  {
    get => (Style?)GetValue(SelectedItemContainerStyleProperty);
    set => SetValue(SelectedItemContainerStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItemContainerStyleSelector"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemContainerStyleSelectorProperty
    = DependencyProperty.Register(nameof(SelectedItemContainerStyleSelector),
      typeof(StyleSelector),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="StyleSelector"/> for the <see cref="SelectedItemContainerStyle"/>
  /// </summary>
  public StyleSelector? SelectedItemContainerStyleSelector
  {
    get => (StyleSelector?)GetValue(SelectedItemContainerStyleSelectorProperty);
    set => SetValue(SelectedItemContainerStyleSelectorProperty, value);
  }

  /// <summary>Identifies the <see cref="Separator"/> dependency property.</summary>
  public static readonly DependencyProperty SeparatorProperty
    = DependencyProperty.Register(nameof(Separator),
      typeof(string),
      typeof(MultiSelectionComboBox),
      new FrameworkPropertyMetadata(null, UpdateText));

  /// <summary>
  /// Gets or Sets the Separator which will be used if the ComboBox is editable.
  /// </summary>
  public string? Separator
  {
    get => (string?)GetValue(SeparatorProperty);
    set => SetValue(SeparatorProperty, value);
  }

  /// <summary>Identifies the <see cref="HasCustomText"/> dependency property.</summary>
  internal static readonly DependencyPropertyKey HasCustomTextPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(HasCustomText),
      typeof(bool),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>Identifies the <see cref="HasCustomText"/> dependency property.</summary>
  public static readonly DependencyProperty HasCustomTextProperty = HasCustomTextPropertyKey.DependencyProperty;

  /// <summary>
  /// Indicates if the text is user defined
  /// </summary>
  public bool HasCustomText
  {
    get => (bool)GetValue(HasCustomTextProperty);
    protected set => SetValue(HasCustomTextPropertyKey, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="TextWrapping"/> dependency property.</summary>
  public static readonly DependencyProperty TextWrappingProperty
    = TextBlock.TextWrappingProperty.AddOwner(typeof(MultiSelectionComboBox),
      new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure));

  /// <summary>
  /// The TextWrapping property controls whether or not text wraps
  /// when it reaches the flow edge of its containing block box.
  /// </summary>
  public TextWrapping TextWrapping
  {
    get => (TextWrapping)GetValue(TextWrappingProperty);
    set => SetValue(TextWrappingProperty, value);
  }

  /// <summary>Identifies the <see cref="AcceptsReturn"/> dependency property.</summary>
  public static readonly DependencyProperty AcceptsReturnProperty
    = TextBoxBase.AcceptsReturnProperty.AddOwner(typeof(MultiSelectionComboBox));

  /// <summary>
  /// The TextWrapping property controls whether or not text wraps
  /// when it reaches the flow edge of its containing block box.
  /// </summary>
  public bool AcceptsReturn
  {
    get => (bool)GetValue(AcceptsReturnProperty);
    set => SetValue(AcceptsReturnProperty, value);
  }

  /// <summary>Identifies the <see cref="ObjectToStringComparer"/> dependency property.</summary>
  public static readonly DependencyProperty ObjectToStringComparerProperty
    = DependencyProperty.Register(nameof(ObjectToStringComparer),
      typeof(ICompareObjectToString),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets a function that is used to check if the entered Text is an object that should be selected.
  /// </summary>
  public ICompareObjectToString? ObjectToStringComparer
  {
    get => (ICompareObjectToString?)GetValue(ObjectToStringComparerProperty);
    set => SetValue(ObjectToStringComparerProperty, value);
  }

  /// <summary>Identifies the <see cref="EditableTextStringComparision"/> dependency property.</summary>
  public static readonly DependencyProperty EditableTextStringComparisionProperty
    = DependencyProperty.Register(nameof(EditableTextStringComparision),
      typeof(StringComparison),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(StringComparison.Ordinal));

  /// <summary>
  ///  Gets or Sets the <see cref="StringComparison"/> that is used to check if the entered <see cref="ComboBox.Text"/> matches to the <see cref="SelectedItems"/>
  /// </summary>
  public StringComparison EditableTextStringComparision
  {
    get => (StringComparison)GetValue(EditableTextStringComparisionProperty);
    set => SetValue(EditableTextStringComparisionProperty, value);
  }

  /// <summary>Identifies the <see cref="StringToObjectParser"/> dependency property.</summary>
  public static readonly DependencyProperty StringToObjectParserProperty
    = DependencyProperty.Register(nameof(StringToObjectParser),
      typeof(IParseStringToObject),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets a parser-class that implements <see cref="IParseStringToObject"/> 
  /// </summary>
  public IParseStringToObject? StringToObjectParser
  {
    get => (IParseStringToObject?)GetValue(StringToObjectParserProperty);
    set => SetValue(StringToObjectParserProperty, value);
  }

  /// <summary>Identifies the <see cref="DisabledPopupOverlayContent"/> dependency property.</summary>
  public static readonly DependencyProperty DisabledPopupOverlayContentProperty
    = DependencyProperty.Register(nameof(DisabledPopupOverlayContent),
      typeof(object),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the DisabledPopupOverlayContent
  /// </summary>
  public object? DisabledPopupOverlayContent
  {
    get => (object?)GetValue(DisabledPopupOverlayContentProperty);
    set => SetValue(DisabledPopupOverlayContentProperty, value);
  }

  /// <summary>Identifies the <see cref="DisabledPopupOverlayContentTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty DisabledPopupOverlayContentTemplateProperty
    = DependencyProperty.Register(nameof(DisabledPopupOverlayContentTemplate),
      typeof(DataTemplate),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the DisabledPopupOverlayContentTemplate
  /// </summary>
  public DataTemplate? DisabledPopupOverlayContentTemplate
  {
    get => (DataTemplate?)GetValue(DisabledPopupOverlayContentTemplateProperty);
    set => SetValue(DisabledPopupOverlayContentTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItemTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemTemplateProperty
    = DependencyProperty.Register(nameof(SelectedItemTemplate),
      typeof(DataTemplate),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the SelectedItemTemplate
  /// </summary>
  public DataTemplate? SelectedItemTemplate
  {
    get => (DataTemplate?)GetValue(SelectedItemTemplateProperty);
    set => SetValue(SelectedItemTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItemTemplateSelector"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemTemplateSelectorProperty
    = DependencyProperty.Register(nameof(SelectedItemTemplateSelector),
      typeof(DataTemplateSelector),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the SelectedItemTemplateSelector
  /// </summary>
  public DataTemplateSelector? SelectedItemTemplateSelector
  {
    get => (DataTemplateSelector?)GetValue(SelectedItemTemplateSelectorProperty);
    set => SetValue(SelectedItemTemplateSelectorProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItemStringFormat"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemStringFormatProperty
    = DependencyProperty.Register(nameof(SelectedItemStringFormat),
      typeof(string),
      typeof(MultiSelectionComboBox),
      new FrameworkPropertyMetadata(null, UpdateText));

  /// <summary>
  /// Gets or Sets the string format for the selected items
  /// </summary>
  public string? SelectedItemStringFormat
  {
    get => (string?)GetValue(SelectedItemStringFormatProperty);
    set => SetValue(SelectedItemStringFormatProperty, value);
  }

  /// <summary>Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property.</summary>
  public static readonly DependencyProperty VerticalScrollBarVisibilityProperty
    = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
      typeof(ScrollBarVisibility),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(ScrollBarVisibility.Auto));

  /// <summary>
  /// Gets or Sets if the vertical scrollbar is visible
  /// </summary>
  public ScrollBarVisibility VerticalScrollBarVisibility
  {
    get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
    set => SetValue(VerticalScrollBarVisibilityProperty, value);
  }

  /// <summary>Identifies the <see cref="HorizontalScrollBarVisibility"/> dependency property.</summary>
  public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty
    = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility),
      typeof(ScrollBarVisibility),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(ScrollBarVisibility.Auto));

  /// <summary>
  /// Gets or Sets if the horizontal scrollbar is visible
  /// </summary>
  public ScrollBarVisibility HorizontalScrollBarVisibility
  {
    get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
    set => SetValue(HorizontalScrollBarVisibilityProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectedItemsPanelTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedItemsPanelTemplateProperty
    = DependencyProperty.Register(nameof(SelectedItemsPanelTemplate),
      typeof(ItemsPanelTemplate),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ItemsPanelTemplate"/> for the selected items.
  /// </summary>
  public ItemsPanelTemplate? SelectedItemsPanelTemplate
  {
    get => (ItemsPanelTemplate?)GetValue(SelectedItemsPanelTemplateProperty);
    set => SetValue(SelectedItemsPanelTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="SelectItemsFromTextInputDelay"/> dependency property.</summary>
  public static readonly DependencyProperty SelectItemsFromTextInputDelayProperty
    = DependencyProperty.Register(nameof(SelectItemsFromTextInputDelay),
      typeof(int),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(-1));

  /// <summary>
  /// Gets or Sets the delay in milliseconds to wait before the selection is updated during text input.
  /// If this value is -1 the selection will not be updated during text input. 
  /// Note: You also need to set <see cref="ObjectToStringComparer"/> to get this to work. 
  /// </summary>
  public int SelectItemsFromTextInputDelay
  {
    get => (int)GetValue(SelectItemsFromTextInputDelayProperty);
    set => SetValue(SelectItemsFromTextInputDelayProperty, value);
  }

  /// <summary>Identifies the <see cref="InterceptKeyboardSelection"/> dependency property.</summary>
  public static readonly DependencyProperty InterceptKeyboardSelectionProperty
    = DependencyProperty.Register(nameof(InterceptKeyboardSelection),
      typeof(bool),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(BooleanBoxes.TrueBox));

  /// <summary>
  /// Gets or Sets if the user can select items from the keyboard, e.g. with the ▲ ▼ Keys. 
  /// This property is only applied when the <see cref="System.Windows.Controls.SelectionMode"/> is <see cref="System.Windows.Controls.SelectionMode.Single"/>
  /// </summary>
  public bool InterceptKeyboardSelection
  {
    get => (bool)GetValue(InterceptKeyboardSelectionProperty);
    set => SetValue(InterceptKeyboardSelectionProperty, value);
  }

  /// <summary>Identifies the <see cref="InterceptMouseWheelSelection"/> dependency property.</summary>
  public static readonly DependencyProperty InterceptMouseWheelSelectionProperty
    = DependencyProperty.Register(nameof(InterceptMouseWheelSelection),
      typeof(bool),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(BooleanBoxes.TrueBox));

  /// <summary>
  /// Gets or Sets if the user can select items by mouse wheel. 
  /// This property is only applied when the <see cref="System.Windows.Controls.SelectionMode"/> is <see cref="System.Windows.Controls.SelectionMode.Single"/>
  /// </summary>
  public bool InterceptMouseWheelSelection
  {
    get => (bool)GetValue(InterceptMouseWheelSelectionProperty);
    set => SetValue(InterceptMouseWheelSelectionProperty, value);
  }

  /// <summary>
  /// Resets the custom Text to the selected Items text 
  /// </summary>
  public void ResetEditableText(bool forceUpdate = false)
  {
    if (PART_EditableTextBox is not null)
    {
      var oldSelectionStart = PART_EditableTextBox.SelectionStart;
      var oldSelectionLength = PART_EditableTextBox.SelectionLength;

      SetValue(HasCustomTextPropertyKey, false);
      UpdateEditableText(forceUpdate);

      PART_EditableTextBox.SelectionStart = oldSelectionStart;
      PART_EditableTextBox.SelectionLength = oldSelectionLength;
    }
  }

  /// <summary>
  /// Identifies the <see cref="IsDropDownHeaderVisible"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty IsDropDownHeaderVisibleProperty =
    DependencyProperty.Register(nameof(IsDropDownHeaderVisible),
      typeof(bool),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets or Sets if the Header in the DropDown is visible
  /// </summary>
  public bool IsDropDownHeaderVisible
  {
    get => (bool)GetValue(IsDropDownHeaderVisibleProperty);
    set => SetValue(IsDropDownHeaderVisibleProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownHeaderContent"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownHeaderContentProperty =
    DependencyProperty.Register(nameof(DropDownHeaderContent),
      typeof(object),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content of the DropDown-Header
  /// </summary>
  public object? DropDownHeaderContent
  {
    get => (object?)GetValue(DropDownHeaderContentProperty);
    set => SetValue(DropDownHeaderContentProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownHeaderContentTemplate"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownHeaderContentTemplateProperty =
    DependencyProperty.Register(nameof(DropDownHeaderContentTemplate),
      typeof(DataTemplate),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content template of the DropDown-Header
  /// </summary>
  public DataTemplate? DropDownHeaderContentTemplate
  {
    get => (DataTemplate?)GetValue(DropDownHeaderContentTemplateProperty);
    set => SetValue(DropDownHeaderContentTemplateProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownHeaderContentTemplateSelector"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownHeaderContentTemplateSelectorProperty =
    DependencyProperty.Register(nameof(DropDownHeaderContentTemplateSelector),
      typeof(DataTemplateSelector),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content template selector of the DropDown-Header
  /// </summary>
  public DataTemplateSelector? DropDownHeaderContentTemplateSelector
  {
    get => (DataTemplateSelector?)GetValue(DropDownHeaderContentTemplateSelectorProperty);
    set => SetValue(DropDownHeaderContentTemplateSelectorProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownHeaderContentStringFormat"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownHeaderContentStringFormatProperty =
    DependencyProperty.Register(nameof(DropDownHeaderContentStringFormat),
      typeof(string),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content string format of the DropDown-Header
  /// </summary>
  public string? DropDownHeaderContentStringFormat
  {
    get => (string?)GetValue(DropDownHeaderContentStringFormatProperty);
    set => SetValue(DropDownHeaderContentStringFormatProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="IsDropDownFooterVisible"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty IsDropDownFooterVisibleProperty =
    DependencyProperty.Register(nameof(IsDropDownFooterVisible),
      typeof(bool),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets or Sets if the Footer in the DropDown is visible
  /// </summary>
  public bool IsDropDownFooterVisible
  {
    get => (bool)GetValue(IsDropDownFooterVisibleProperty);
    set => SetValue(IsDropDownFooterVisibleProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownFooterContent"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownFooterContentProperty =
    DependencyProperty.Register(nameof(DropDownFooterContent),
      typeof(object),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content of the DropDown-Footer
  /// </summary>
  public object? DropDownFooterContent
  {
    get => (object?)GetValue(DropDownFooterContentProperty);
    set => SetValue(DropDownFooterContentProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownFooterContentTemplate"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownFooterContentTemplateProperty =
    DependencyProperty.Register(nameof(DropDownFooterContentTemplate),
      typeof(DataTemplate),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content template of the DropDown-Footer
  /// </summary>
  public DataTemplate? DropDownFooterContentTemplate
  {
    get => (DataTemplate?)GetValue(DropDownFooterContentTemplateProperty);
    set => SetValue(DropDownFooterContentTemplateProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownFooterContentTemplateSelector"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownFooterContentTemplateSelectorProperty =
    DependencyProperty.Register(nameof(DropDownFooterContentTemplateSelector),
      typeof(DataTemplateSelector),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content template selector of the DropDown-Footer
  /// </summary>
  public DataTemplateSelector? DropDownFooterContentTemplateSelector
  {
    get => (DataTemplateSelector?)GetValue(DropDownFooterContentTemplateSelectorProperty);
    set => SetValue(DropDownFooterContentTemplateSelectorProperty, value);
  }

  /// <summary>
  /// Identifies the <see cref="DropDownFooterContentStringFormat"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty DropDownFooterContentStringFormatProperty =
    DependencyProperty.Register(nameof(DropDownFooterContentStringFormat),
      typeof(string),
      typeof(MultiSelectionComboBox),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the content string format of the DropDown-Footer
  /// </summary>
  public string? DropDownFooterContentStringFormat
  {
    get => (string?)GetValue(DropDownFooterContentStringFormatProperty);
    set => SetValue(DropDownFooterContentStringFormatProperty, value);
  }

  #endregion

  #region Methods

  /// <summary>
  /// Updates the Text of the editable TextBox.
  /// Sets the custom Text if any otherwise the concatenated string.
  /// </summary> 
  private void UpdateEditableText(bool forceUpdate = false)
  {
    if (PART_EditableTextBox is null || (PART_EditableTextBox.IsKeyboardFocused && !forceUpdate))
    {
      return;
    }

    isTextChanging = true;

    var oldSelectionStart = PART_EditableTextBox.SelectionStart;
    var oldSelectionLength = PART_EditableTextBox.SelectionLength;
    var oldTextLength = PART_EditableTextBox.Text.Length;

    var selectedItemsText = GetSelectedItemsText();

    if (!HasCustomText)
    {
      SetCurrentValue(TextProperty, selectedItemsText);
    }

    UpdateHasCustomText(selectedItemsText);

    if (oldSelectionLength == oldTextLength) // We had all Text selected, so we select all again
    {
      PART_EditableTextBox.SelectionStart = 0;
      PART_EditableTextBox.SelectionLength = PART_EditableTextBox.Text.Length;
    }
    else if (oldSelectionStart == oldTextLength) // we had the cursor at the last position, so we move the cursor to the end again
    {
      PART_EditableTextBox.SelectionStart = PART_EditableTextBox.Text.Length;
    }
    else // we restore the old selection
    {
      PART_EditableTextBox.SelectionStart = oldSelectionStart;
      PART_EditableTextBox.SelectionLength = oldSelectionLength;
    }

    isTextChanging = false;
  }

  private void UpdateDisplaySelectedItems()
  {
    UpdateDisplaySelectedItems(OrderSelectedItemsBy);
  }

  public string? GetSelectedItemsText()
  {
    switch (SelectionMode)
    {
      case SelectionMode.Single:
        if (ReadLocalValue(DisplayMemberPathProperty) != DependencyProperty.UnsetValue
            || ReadLocalValue(SelectedItemStringFormatProperty) != DependencyProperty.UnsetValue)
        {
          return BindingHelper.Eval(SelectedItem, DisplayMemberPath ?? string.Empty, SelectedItemStringFormat)?.ToString();
        }
        else
        {
          return SelectedItem?.ToString();
        }

      case SelectionMode.Multiple:
      case SelectionMode.Extended:
        IEnumerable<object>? values;

        if (ReadLocalValue(DisplayMemberPathProperty) != DependencyProperty.UnsetValue
            || ReadLocalValue(SelectedItemStringFormatProperty) != DependencyProperty.UnsetValue)
        {
          values = DisplaySelectedItems?.OfType<object>().Select(o => BindingHelper.Eval(o, DisplayMemberPath ?? string.Empty, SelectedItemStringFormat)) as IEnumerable<object>;
        }
        else
        {
          values = DisplaySelectedItems as IEnumerable<object>;
        }

        return values is null ? null : string.Join(Separator ?? string.Empty, values);

      default:
        return null;
    }
  }

  private void UpdateHasCustomText(string? selectedItemsText)
  {
    // if the parameter was null lets get the text on our own.
    selectedItemsText ??= GetSelectedItemsText();

    HasCustomText = !((string.IsNullOrEmpty(selectedItemsText) && string.IsNullOrEmpty(Text))
                      || string.Equals(Text, selectedItemsText, EditableTextStringComparision));
  }

  private void UpdateDisplaySelectedItems(SelectedItemsOrderType selectedItemsOrderType)
  {
    var displaySelectedItems = selectedItemsOrderType switch
    {
      SelectedItemsOrderType.SelectedOrder => SelectedItems,
      SelectedItemsOrderType.ItemsSourceOrder => (SelectedItems as IEnumerable<object>)?.OrderBy(o => Items.IndexOf(o)),
      _ => DisplaySelectedItems
    };

    SetValue(DisplaySelectedItemsPropertyKey, displaySelectedItems);
  }

  private void SelectItemsFromText(int millisecondsToWait)
  {
    if (!isUserdefinedTextInputPending || isTextChanging)
    {
      return;
    }

    // We want to do a text reset or add items only if we don't need to wait for more input. 
    shouldDoTextReset = millisecondsToWait == 0;
    shouldAddItems = millisecondsToWait == 0;

    if (updateSelectedItemsFromTextTimer is null)
    {
      updateSelectedItemsFromTextTimer = new DispatcherTimer(DispatcherPriority.Background);
      updateSelectedItemsFromTextTimer.Tick += UpdateSelectedItemsFromTextTimer_Tick;
    }

    if (updateSelectedItemsFromTextTimer.IsEnabled)
    {
      updateSelectedItemsFromTextTimer.Stop();
    }

    if (ObjectToStringComparer is not null && (!string.IsNullOrEmpty(Separator) || SelectionMode == SelectionMode.Single))
    {
      updateSelectedItemsFromTextTimer.Interval = TimeSpan.FromMilliseconds(millisecondsToWait > 0 ? millisecondsToWait : 0);
      updateSelectedItemsFromTextTimer.Start();
    }
  }

#if NET5_0_OR_GREATER || NETCOREAPP
  private void UpdateSelectedItemsFromTextTimer_Tick(object? sender, EventArgs e)
#else
        private void UpdateSelectedItemsFromTextTimer_Tick(object sender, EventArgs e)
#endif
  {
    updateSelectedItemsFromTextTimer?.Stop();

    // We clear the selection if there is no text available. 
    if (string.IsNullOrEmpty(Text))
    {
      switch (SelectionMode)
      {
        case SelectionMode.Single:
          SetCurrentValue(SelectedItemProperty, null);
          break;
        case SelectionMode.Multiple:
        case SelectionMode.Extended:
          SelectedItems?.Clear();
          break;
        default:
          throw new NotSupportedException("Unknown SelectionMode");
      }

      return;
    }

    bool foundItem;

    switch (SelectionMode)
    {
      case SelectionMode.Single:
        SetCurrentValue(SelectedItemProperty, null);

        foundItem = false;
        if (ObjectToStringComparer is not null)
        {
          foreach (var item in Items)
          {
            if (ObjectToStringComparer.CheckIfStringMatchesObject(Text, item, EditableTextStringComparision, SelectedItemStringFormat))
            {
              SetCurrentValue(SelectedItemProperty, item);
              foundItem = true;
              break;
            }
          }
        }

        if (!foundItem)
        {
          // We try to add a new item. If we were able to do so we need to update the text as it may differ. 
          if (shouldAddItems && TryAddObjectFromString(Text, out var result))
          {
            SetCurrentValue(SelectedItemProperty, result);
          }
          else
          {
            shouldDoTextReset = false; // We did not find the needed item so we should not do the text reset.
          }
        }

        break;

      case SelectionMode.Multiple:
      case SelectionMode.Extended:
        if (SelectedItems is null)
        {
          break; // Exit here if we have no SelectedItems yet
        }

        var strings = !string.IsNullOrEmpty(Separator) ? Text?.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries) : null;

        int position = 0;

        if (strings is not null)
        {
          foreach (var stringObject in strings)
          {
            foundItem = false;
            if (ObjectToStringComparer is not null)
            {
              foreach (var item in Items)
              {
                if (ObjectToStringComparer.CheckIfStringMatchesObject(stringObject, item, EditableTextStringComparision, SelectedItemStringFormat))
                {
                  var oldPosition = SelectedItems.IndexOf(item);

                  if (oldPosition < 0) // if old index is <0 the item is not in list yet. let's add it.
                  {
                    SelectedItems.Insert(position, item);
                    foundItem = true;
                    position++;
                  }
                  else if (oldPosition > position) // if the item has a wrong position in list we need to swap it accordingly.
                  {
                    SelectedItems.RemoveAt(oldPosition);
                    SelectedItems.Insert(position, item);

                    foundItem = true;
                    position++;
                  }
                  else if (oldPosition == position)
                  {
                    foundItem = true;
                    position++;
                  }
                }
              }
            }

            if (!foundItem)
            {
              if (shouldAddItems && TryAddObjectFromString(stringObject, out var result))
              {
                SelectedItems.Insert(position, result);
                position++;
              }
              else
              {
                shouldDoTextReset = false;
              }
            }
          }
        }

        // Remove Items if needed.
        while (SelectedItems.Count > position)
        {
          SelectedItems.RemoveAt(position);
        }

        break;

      default:
        throw new NotSupportedException("Unknown SelectionMode");
    }

    // First we need to check if the string matches completely to the selected items. Therefore we need to display the items in the selected order first
    UpdateDisplaySelectedItems(SelectedItemsOrderType.SelectedOrder);
    UpdateHasCustomText(null);

    // If the items should be ordered differently than above we need to reorder them accordingly.
    if (OrderSelectedItemsBy != SelectedItemsOrderType.SelectedOrder)
    {
      UpdateDisplaySelectedItems();
    }

    if (PART_EditableTextBox is not null)
    {
      // We do a text reset if all items were successfully found and we don't have to wait for more input.
      if (shouldDoTextReset)
      {
        var oldCaretPos = PART_EditableTextBox.CaretIndex;
        ResetEditableText();
        PART_EditableTextBox.CaretIndex = oldCaretPos;
      }

      // If we have the KeyboardFocus we need to update the text later in order to not interrupt the user.
      // Therefore we connect this flag to the KeyboardFocus of the TextBox.
      isUserdefinedTextInputPending = PART_EditableTextBox.IsKeyboardFocused;
    }
  }

  private bool TryAddObjectFromString(string? input, out object? result)
  {
    try
    {
      if (StringToObjectParser is null)
      {
        result = null;
        return false;
      }

      var elementType = DefaultStringToObjectParser.Instance.GetElementType(ItemsSource);

      var foundItem = StringToObjectParser.TryCreateObjectFromString(input, out result, Language.GetEquivalentCulture(), SelectedItemStringFormat, elementType);

      var addingItemEventArgs = new AddingItemEventArgs(AddingItemEvent,
        this,
        input,
        result,
        foundItem,
        ItemsSource as IList,
        elementType,
        SelectedItemStringFormat,
        Language.GetEquivalentCulture(),
        StringToObjectParser);

      RaiseEvent(addingItemEventArgs);

      if (addingItemEventArgs.Handled)
      {
        addingItemEventArgs.Accepted = false;
      }

      // If the adding event was not handled and the item is marked as accepted and we are allowed to modify the items list we can add the pared item
      if (addingItemEventArgs.Accepted && (!addingItemEventArgs.TargetList?.IsReadOnly ?? false))
      {
        addingItemEventArgs.TargetList?.Add(addingItemEventArgs.ParsedObject);

        RaiseEvent(new AddedItemEventArgs(AddedItemEvent, this, addingItemEventArgs.ParsedObject, addingItemEventArgs.TargetList));
      }

      result = addingItemEventArgs.ParsedObject;
      return addingItemEventArgs.Accepted;
    }
    catch (Exception e)
    {
      Trace.WriteLine(e.Message);
      result = null;
      return false;
    }
  }

  #endregion

  #region Commands

  // Clear Text Command
  public static RoutedUICommand ClearContentCommand { get; } = new RoutedUICommand("ClearContent", nameof(ClearContentCommand), typeof(MultiSelectionComboBox));

  private static void ExecutedClearContentCommand(object sender, ExecutedRoutedEventArgs e)
  {
    if (sender is MultiSelectionComboBox multiSelectionCombo)
    {
      if (multiSelectionCombo.HasCustomText)
      {
        multiSelectionCombo.ResetEditableText(true);
      }
      else
      {
        switch (multiSelectionCombo.SelectionMode)
        {
          case SelectionMode.Single:
            multiSelectionCombo.SetCurrentValue(SelectedItemProperty, null);
            break;
          case SelectionMode.Multiple:
          case SelectionMode.Extended:
            multiSelectionCombo.SelectedItems?.Clear();
            break;
          default:
            throw new NotSupportedException("Unknown SelectionMode");
        }
      }

      multiSelectionCombo.ResetEditableText(true);
    }
  }

  private static void CanExecuteClearContentCommand(object sender, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = false;
    if (sender is MultiSelectionComboBox multiSelectionComboBox)
    {
      e.CanExecute = !string.IsNullOrEmpty(multiSelectionComboBox.Text) || multiSelectionComboBox.SelectedItems?.Count > 0;
    }
  }

  public static RoutedUICommand RemoveItemCommand { get; } = new RoutedUICommand("Remove item", nameof(RemoveItemCommand), typeof(MultiSelectionComboBox));

  private static void RemoveItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
  {
    if (sender is MultiSelectionComboBox multiSelectionCombo)
    {
      if (multiSelectionCombo.SelectionMode == SelectionMode.Single)
      {
        multiSelectionCombo.SetCurrentValue(SelectedItemProperty, null);
        return;
      }

      if (multiSelectionCombo.SelectedItems is not null && multiSelectionCombo.SelectedItems.Contains(e.Parameter))
      {
        multiSelectionCombo.SelectedItems.Remove(e.Parameter);
      }
    }
  }

  private static void RemoveItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = false;
    if (sender is MultiSelectionComboBox)
    {
      e.CanExecute = e.Parameter != null;
    }
  }

  #endregion

  #region Overrides

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    // Init SelectedItemsPresenter
    var selectedItemsPresenterName = nameof(PART_SelectedItemsPresenter);
    PART_SelectedItemsPresenter = GetTemplateChild(selectedItemsPresenterName) as ListBox ?? throw new MissingRequiredTemplatePartException(this, selectedItemsPresenterName);

    PART_SelectedItemsPresenter.MouseLeftButtonUp -= PART_SelectedItemsPresenter_MouseLeftButtonUp;
    PART_SelectedItemsPresenter.MouseLeftButtonUp += PART_SelectedItemsPresenter_MouseLeftButtonUp;
    PART_SelectedItemsPresenter.SelectionChanged -= PART_SelectedItemsPresenter_SelectionChanged;
    PART_SelectedItemsPresenter.SelectionChanged += PART_SelectedItemsPresenter_SelectionChanged;

    // Init EditableTextBox
    PART_EditableTextBox = GetTemplateChild(nameof(PART_EditableTextBox)) as TextBox ?? throw new MissingRequiredTemplatePartException(this, nameof(PART_EditableTextBox));

    PART_EditableTextBox.LostFocus -= PART_EditableTextBox_LostFocus;
    PART_EditableTextBox.LostFocus += PART_EditableTextBox_LostFocus;

    // Init Popup
    PART_Popup = GetTemplateChild(nameof(PART_Popup)) as Popup ?? throw new MissingRequiredTemplatePartException(this, nameof(PART_Popup));
    PART_PopupListBox = GetTemplateChild(nameof(PART_PopupListBox)) as ListBox ?? throw new MissingRequiredTemplatePartException(this, nameof(PART_PopupListBox));

    if (PART_PopupListBox.SelectedItems is INotifyCollectionChanged selectedItemsCollection)
    {
      selectedItemsCollection.CollectionChanged -= PART_PopupListBox_SelectedItems_CollectionChanged;
      selectedItemsCollection.CollectionChanged += PART_PopupListBox_SelectedItems_CollectionChanged;
    }

    SyncSelectedItems(SelectedItems, PART_PopupListBox.SelectedItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    // Do update the text and selection
    UpdateDisplaySelectedItems();
    UpdateEditableText(true);
  }

#if NET5_0_OR_GREATER
  private void PART_PopupListBox_SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
#else
        private void PART_PopupListBox_SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
#endif
  {
    SyncSelectedItems(PART_PopupListBox?.SelectedItems, SelectedItems, e);
  }

  protected override void OnSelectionChanged(SelectionChangedEventArgs e)
  {
    base.OnSelectionChanged(e);
    UpdateDisplaySelectedItems();
    UpdateEditableText();
  }

  private void MultiSelectionComboBox_Loaded(object sender, EventArgs e)
  {
    Loaded -= MultiSelectionComboBox_Loaded;

    if (PART_PopupListBox is not null)
    {
      // If we have the ItemsSource set, we need to exit here. 
      if (((PART_PopupListBox.Items as IList)?.IsReadOnly ?? false) || BindingOperations.IsDataBound(PART_PopupListBox, ItemsSourceProperty))
      {
        return;
      }

      PART_PopupListBox.Items.Clear();
      foreach (var item in Items)
      {
        PART_PopupListBox.Items.Add(item);
      }
    }
  }

  protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
  {
    base.OnItemsChanged(e);

    if (!IsLoaded)
    {
      Loaded += MultiSelectionComboBox_Loaded;
      return;
    }

    // If we have the ItemsSource set, we need to exit here. 
    if (((PART_PopupListBox?.Items as IList)?.IsReadOnly ?? false) || BindingOperations.IsDataBound(this, ItemsSourceProperty))
    {
      return;
    }

    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        if (e.NewItems is not null)
        {
          foreach (var item in e.NewItems)
          {
            PART_PopupListBox?.Items?.Add(item);
          }
        }

        break;

      case NotifyCollectionChangedAction.Remove:
        if (e.OldItems is not null)
        {
          foreach (var item in e.OldItems)
          {
            PART_PopupListBox?.Items?.Remove(item);
          }
        }

        break;

      case NotifyCollectionChangedAction.Replace:
      case NotifyCollectionChangedAction.Move:
      case NotifyCollectionChangedAction.Reset:
        PART_PopupListBox?.Items.Clear();
        foreach (var item in Items)
        {
          PART_PopupListBox?.Items?.Add(item);
        }

        break;
      default:
        throw new NotSupportedException("Unsupported NotifyCollectionChangedAction");
    }
  }

  protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
  {
    base.OnRenderSizeChanged(sizeInfo);

    // For now we only want to update our position if the height changed. Else we will get a flickering in SharedGridColumns
    if (IsDropDownOpen && sizeInfo.HeightChanged)
    {
      this.BeginInvoke(multiSelectionComboBox =>
      {
        if (multiSelectionComboBox.PART_Popup is not null)
        {
          multiSelectionComboBox.PART_Popup.HorizontalOffset++;
          multiSelectionComboBox.PART_Popup.HorizontalOffset--;
        }
      });
    }
  }

  protected override void OnDropDownOpened(EventArgs e)
  {
    base.OnDropDownOpened(e);

    if (PART_PopupListBox is not null)
    {
      PART_PopupListBox.Focus();

      if (PART_PopupListBox.Items.Count == 0)
      {
        return;
      }
    }

    MoveFocusToDropDown();

    SelectItemsFromText(0);
  }

  /// <summary>
  /// Sets the Keyboard focus to the dropdown
  /// </summary>
  private void MoveFocusToDropDown()
  {
    if (PART_PopupListBox is null || PART_Popup is null)
    {
      return;
    }

    var index = PART_PopupListBox.SelectedIndex;
    if (index < 0 && PART_PopupListBox.Items.Count > 0)
    {
      index = 0;
    }

    this.BeginInvoke(() =>
      {
        ListBoxItem? item = null;
        if (index >= 0)
        {
          PART_PopupListBox.ScrollIntoView(PART_PopupListBox.Items[index]);
          item = PART_PopupListBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        if (item is not null)
        {
          item.Focus();
          KeyboardNavigationEx.Focus(item);
          PART_PopupListBox.ScrollIntoView(item);
        }
        else
        {
          PART_Popup.Focus();
        }
      },
      DispatcherPriority.Send);
  }

  /// <summary>
  /// Return true if the item is (or is eligible to be) its own ItemUI
  /// </summary>
  protected override bool IsItemItsOwnContainerOverride(object item)
  {
    return (item is ListBoxItem);
  }

  /// <summary> Create or identify the element used to display the given item. </summary>
  protected override DependencyObject GetContainerForItemOverride()
  {
    return new ListBoxItem();
  }

  protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
  {
    if (PART_PopupListBox is null || PART_EditableTextBox is null)
    {
      return;
    }

    if (IsEditable && !IsDropDownOpen && !InterceptKeyboardSelection)
    {
      if (HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled && ScrollViewerHelper.GetIsHorizontalScrollWheelEnabled(this))
      {
        if (e.Delta > 0)
        {
          PART_EditableTextBox.LineLeft();
        }
        else
        {
          PART_EditableTextBox.LineRight();
        }
      }
      else
      {
        if (e.Delta > 0)
        {
          PART_EditableTextBox.LineUp();
        }
        else
        {
          PART_EditableTextBox.LineDown();
        }
      }
    }
    else if (!IsEditable && !IsDropDownOpen && !InterceptMouseWheelSelection)
    {
      var scrollViewer = PART_SelectedItemsPresenter.FindChild<ScrollViewer>();
      if (scrollViewer?.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled && ScrollViewerHelper.GetIsHorizontalScrollWheelEnabled(this))
      {
        if (e.Delta > 0)
        {
          scrollViewer?.LineLeft();
        }
        else
        {
          scrollViewer?.LineRight();
        }
      }
      else
      {
        if (e.Delta > 0)
        {
          scrollViewer?.LineUp();
        }
        else
        {
          scrollViewer?.LineDown();
        }
      }
    }
    // ListBox eats the selection so we need to handle this event here if we want to select the next item.
    else if (!IsDropDownOpen && InterceptMouseWheelSelection && SelectionMode == SelectionMode.Single)
    {
      if (e.Delta > 0 && PART_PopupListBox.SelectedIndex > 0)
      {
        SelectPrev();
      }
      else if (e.Delta < 0 && PART_PopupListBox.SelectedIndex < PART_PopupListBox.Items.Count - 1)
      {
        SelectNext();
      }
    }

    // The event is handled if the drop down is not open. 
    e.Handled = !IsDropDownOpen;
    base.OnPreviewMouseWheel(e);
  }

  /// <summary>
  ///     An event reporting a key was pressed
  /// </summary>
  protected override void OnPreviewKeyDown(KeyEventArgs e)
  {
    // Only process preview key events if they going to our editable text box
    if (IsEditable && PART_EditableTextBox is not null && ReferenceEquals(e.OriginalSource, PART_EditableTextBox))
    {
      KeyDownHandler(e);
    }
  }

  /// <summary>
  ///     An event reporting a key was pressed
  /// </summary>
  protected override void OnKeyDown(KeyEventArgs e)
  {
    KeyDownHandler(e);
  }

  private void KeyDownHandler(KeyEventArgs e)
  {
    var handled = false;
    var key = e.Key;

    // We want to handle Alt key. Get the real key if it is Key.System.
    if (key == Key.System)
    {
      key = e.SystemKey;
    }

    // In Right to Left mode we switch Right and Left keys
    var isRightToLeft = (FlowDirection == FlowDirection.RightToLeft);

    switch (key)
    {
      case Key.Up:
        handled = true;
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
          IsDropDownOpen = !IsDropDownOpen;
        }
        else
        {
          // When the drop down isn't open then focus is on the ComboBox
          // and we can't use KeyboardNavigation.
          if (IsDropDownOpen)
          {
            MoveFocusToDropDown();
          }
          else if (!IsDropDownOpen && InterceptKeyboardSelection && SelectionMode == SelectionMode.Single)
          {
            SelectPrev();
          }
        }

        break;

      case Key.Down:
        handled = true;
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
          IsDropDownOpen = !IsDropDownOpen;
        }
        else
        {
          // When the drop down isn't open then focus is on the ComboBox
          // and we can't use KeyboardNavigation.
          if (IsDropDownOpen)
          {
            MoveFocusToDropDown();
          }
          else if (!IsDropDownOpen && InterceptKeyboardSelection && SelectionMode == SelectionMode.Single)
          {
            SelectNext();
          }
        }

        break;

      case Key.F4:
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
        {
          IsDropDownOpen = !IsDropDownOpen;
          handled = true;
        }

        break;

      case Key.Escape:
        base.OnKeyDown(e);
        break;

      case Key.Enter:
        if (IsDropDownOpen)
        {
          base.OnKeyDown(e);
        }

        break;

      case Key.Home:
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
        {
          if (!IsDropDownOpen && InterceptKeyboardSelection && SelectionMode == SelectionMode.Single)
          {
            SelectFirst();
          }

          handled = true;
        }

        break;

      case Key.End:
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
        {
          if (!IsDropDownOpen && InterceptKeyboardSelection && SelectionMode == SelectionMode.Single)
          {
            SelectLast();
          }

          handled = true;
        }

        break;

      case Key.Right:
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
        {
          if (IsDropDownOpen)
          {
            MoveFocusToDropDown();
          }
          else
          {
            if (!isRightToLeft)
            {
              SelectNext();
            }
            else if (!IsDropDownOpen && InterceptKeyboardSelection && SelectionMode == SelectionMode.Single)
            {
              // If it's RTL then Right should go backwards
              SelectPrev();
            }
          }

          handled = true;
        }

        break;

      case Key.Left:
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt && !IsEditable)
        {
          if (IsDropDownOpen)
          {
            MoveFocusToDropDown();
          }
          else if (!IsDropDownOpen && InterceptKeyboardSelection && SelectionMode == SelectionMode.Single)
          {
            if (!isRightToLeft)
            {
              SelectPrev();
            }
            else
            {
              // If it's RTL then Left should go the other direction
              SelectNext();
            }
          }

          handled = true;
        }

        break;

      case Key.PageUp:
        if (IsDropDownOpen)
        {
          // At the moment this feature is not implemented for this control.
          handled = true;
        }

        break;

      case Key.PageDown:
        if (IsDropDownOpen)
        {
          // At the moment this feature is not implemented for this control.
          handled = true;
        }

        break;

      case Key.Oem5:
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
          // At the moment this feature is not implemented for this control.
          handled = true;
        }

        break;

      default:
        handled = false;
        break;
    }

    if (handled)
    {
      e.Handled = true;
    }
  }

  // adopted from original ComoBox
  private void SelectPrev()
  {
    if (!Items.IsEmpty)
    {
      // Search backwards from SelectedIndex - 1 but don't start before the beginning.
      // If SelectedIndex is less than 0, there is nothing to select before this item.
      if (SelectedIndex > 0)
      {
        SelectItemHelper(SelectedIndex - 1, -1, -1);
      }
    }
  }

  // adopted from original ComoBox
  private void SelectNext()
  {
    var count = Items.Count;
    if (count > 0)
    {
      // Search forwards from SelectedIndex + 1 but don't start past the end.
      // If SelectedIndex is before the last item then there is potentially
      // something afterwards that we could select.
      if (SelectedIndex < count - 1)
      {
        SelectItemHelper(SelectedIndex + 1, +1, count);
      }
    }
  }

  // adopted from original ComoBox
  private void SelectFirst()
  {
    SelectItemHelper(0, +1, Items.Count);
  }

  // adopted from original ComoBox
  private void SelectLast()
  {
    SelectItemHelper(Items.Count - 1, -1, -1);
  }

  // adopted from original ComoBox
  // Walk in the specified direction until we get to a selectable
  // item or to the stopIndex.
  // NOTE: stopIndex is not inclusive (it should be one past the end of the range)
  private void SelectItemHelper(int startIndex, int increment, int stopIndex)
  {
    Debug.Assert((increment > 0 && startIndex <= stopIndex) || (increment < 0 && startIndex >= stopIndex), "Infinite loop detected");

    for (var i = startIndex; i != stopIndex; i += increment)
    {
      // If the item is selectable and the wrapper is selectable, select it.
      // Need to check both because the user could set any combination of
      // IsSelectable and IsEnabled on the item and wrapper.
      var item = Items[i];
      var container = ItemContainerGenerator.ContainerFromIndex(i);
      if (IsSelectableHelper(item) && IsSelectableHelper(container))
      {
        SelectedIndex = i;
        UpdateEditableText(true); // We force the update of the text
        isUserdefinedTextInputPending = false;
        break;
      }
    }
  }

  // adopted from original ComoBox
  private static bool IsSelectableHelper(object o)
  {
    var d = o as DependencyObject;
    // If o is not a DependencyObject, it is just a plain
    // object and must be selectable and enabled.
    if (d == null)
    {
      return true;
    }

    // It's selectable if IsSelectable is true and IsEnabled is true.
    return (bool)d.GetValue(IsEnabledProperty);
  }

  /// <summary>
  ///     Select all the items
  /// </summary>
  public void SelectAll()
  {
    PART_PopupListBox?.SelectAll();
  }

  private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
  {
    if (target is MultiSelectionComboBox comboBox)
    {
      comboBox.SelectAll();
    }
  }

  private static void OnQueryStatusSelectAll(object target, CanExecuteRoutedEventArgs args)
  {
    if (target is MultiSelectionComboBox comboBox)
    {
      args.CanExecute
        = comboBox.SelectionMode == SelectionMode.Extended
          && comboBox.IsDropDownOpen
          && !(comboBox.PART_EditableTextBox?.IsKeyboardFocused ?? false);
    }
  }

  /// <summary>
  ///     Clears all of the selected items.
  /// </summary>
  [PublicAPI]
  public void UnselectAll()
  {
    PART_PopupListBox?.UnselectAll();
  }

  #endregion

  #region Events

#if NET5_0_OR_GREATER
  private void SelectedItemsImpl_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
#else
        private void SelectedItemsImpl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
#endif
  {
    if (PART_PopupListBox is null)
    {
      return;
    }

    SyncSelectedItems(sender as IList, PART_PopupListBox.SelectedItems, e);
  }

  private void SyncSelectedItems(IList? sourceCollection, IList? targetCollection, NotifyCollectionChangedEventArgs e)
  {
    if (isSyncingSelectedItems || sourceCollection is null || targetCollection is null || !IsInitialized)
    {
      return;
    }

    isSyncingSelectedItems = true;

    try
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          if (e.NewItems is not null)
          {
            foreach (var item in e.NewItems)
            {
              targetCollection.Add(item);
            }
          }

          break;
        case NotifyCollectionChangedAction.Remove:
          if (e.OldItems is not null)
          {
            foreach (var item in e.OldItems)
            {
              targetCollection.Remove(item);
            }
          }

          break;
        case NotifyCollectionChangedAction.Replace:
          if (e.NewItems is not null)
          {
            foreach (var item in e.NewItems)
            {
              targetCollection.Add(item);
            }
          }

          if (e.OldItems is not null)
          {
            foreach (var item in e.OldItems)
            {
              targetCollection.Remove(item);
            }
          }

          break;
        case NotifyCollectionChangedAction.Move:
          if (e.OldItems is not null)
          {
            var itemCount = e.OldItems.Count;

            // for the number of items being removed, remove the item from the Old Starting Index
            // (this will cause following items to be shifted down to fill the hole).
            for (var i = 0; i < itemCount; i++)
            {
              targetCollection.RemoveAt(e.OldStartingIndex);
            }
          }

          if (e.NewItems is not null)
          {
            var itemCount = e.NewItems.Count;

            for (var i = 0; i < itemCount; i++)
            {
              var insertionPoint = e.NewStartingIndex + i;

              if (insertionPoint > targetCollection.Count)
              {
                targetCollection.Add(e.NewItems[i]);
              }
              else
              {
                targetCollection.Insert(insertionPoint, e.NewItems[i]);
              }
            }
          }

          break;
        case NotifyCollectionChangedAction.Reset:
          targetCollection.Clear();

          foreach (var item in sourceCollection)
          {
            targetCollection.Add(item);
          }

          break;
      }

      UpdateDisplaySelectedItems();
      UpdateEditableText();
      UpdateHasCustomText(null);
    }
    finally
    {
      isSyncingSelectedItems = false;
    }
  }

  private void PART_EditableTextBox_LostFocus(object sender, RoutedEventArgs e)
  {
    SelectItemsFromText(0);
  }

  private void PART_SelectedItemsPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    // If we have a ScrollViewer (ListBox has) we need to handle this event here as it will not be forwarded to the ToggleButton
    SetCurrentValue(IsDropDownOpenProperty, BooleanBoxes.Box(!IsDropDownOpen));
  }

  private void PART_SelectedItemsPresenter_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    // We don't want the SelectedItems to be selectable. So anytime the selection will be changed we will reset it. 
    PART_SelectedItemsPresenter?.SetCurrentValue(SelectedItemProperty, null);
  }

  private static void UpdateText(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is MultiSelectionComboBox multiSelectionComboBox)
    {
      multiSelectionComboBox.UpdateEditableText();
    }
  }

  private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is MultiSelectionComboBox multiSelectionComboBox && !multiSelectionComboBox.isTextChanging)
    {
      multiSelectionComboBox.UpdateHasCustomText(null);
      multiSelectionComboBox.isUserdefinedTextInputPending = true;

      // Select the items during typing if enabled
      if (multiSelectionComboBox.SelectItemsFromTextInputDelay >= 0)
      {
        multiSelectionComboBox.SelectItemsFromText(multiSelectionComboBox.SelectItemsFromTextInputDelay);
      }
    }
  }

  private static void OnOrderSelectedItemsByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is MultiSelectionComboBox multiSelectionComboBox && !multiSelectionComboBox.HasCustomText)
    {
      multiSelectionComboBox.UpdateDisplaySelectedItems();
      multiSelectionComboBox.UpdateEditableText();
    }
  }

  /// <summary>Identifies the <see cref="AddingItem"/> routed event.</summary>
  public static readonly RoutedEvent AddingItemEvent = EventManager.RegisterRoutedEvent(
    nameof(AddingItem), RoutingStrategy.Bubble, typeof(AddingItemEventArgsHandler), typeof(MultiSelectionComboBox));

  /// <summary>
  ///     Occurs before a new object is added to the Items-List
  /// </summary>
  public event AddingItemEventArgsHandler AddingItem
  {
    add => AddHandler(AddingItemEvent, value);
    remove => RemoveHandler(AddingItemEvent, value);
  }

  /// <summary>Identifies the <see cref="AddedItem"/> routed event.</summary>
  public static readonly RoutedEvent AddedItemEvent = EventManager.RegisterRoutedEvent(
    nameof(AddedItem), RoutingStrategy.Bubble, typeof(AddedItemEventArgsHandler), typeof(MultiSelectionComboBox));

  /// <summary>
  ///     Occurs before a new object is added to the Items-List
  /// </summary>
  public event AddedItemEventArgsHandler AddedItem
  {
    add => AddHandler(AddedItemEvent, value);
    remove => RemoveHandler(AddedItemEvent, value);
  }

  #endregion
}