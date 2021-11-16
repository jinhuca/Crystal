namespace Crystal.Themes.Controls
{
  /// <summary>
  /// The HamburgerMenu is based on a <see cref="SplitView"/> control. By default it contains a HamburgerButton and a ListView to display menu items.
  /// </summary>
  [TemplatePart(Name = "HamburgerButton", Type = typeof(Button))]
  [TemplatePart(Name = "ButtonsListView", Type = typeof(ListBox))]
  [TemplatePart(Name = "OptionsListView", Type = typeof(ListBox))]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.TemplatePart", "WPF0132:Use PART prefix.", Justification = "<Pending>")]
  public partial class HamburgerMenu : ContentControl
  {
    private Button? hamburgerButton;
    private ListBox? buttonsListView;
    private ListBox? optionsListView;
    private readonly PropertyChangeNotifier actualWidthPropertyChangeNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="HamburgerMenu"/> class.
    /// </summary>
    public HamburgerMenu()
    {
      DefaultStyleKey = typeof(HamburgerMenu);

      actualWidthPropertyChangeNotifier = new PropertyChangeNotifier(this, ActualWidthProperty);
      actualWidthPropertyChangeNotifier.ValueChanged += (s, e) => CoerceValue(OpenPaneLengthProperty);
    }

    /// <summary>
    /// Override default OnApplyTemplate to capture children controls
    /// </summary>
    public override void OnApplyTemplate()
    {
      if (hamburgerButton != null)
      {
        hamburgerButton.Click -= OnHamburgerButtonClick;
      }

      if (buttonsListView != null)
      {
        buttonsListView.SelectionChanged -= ButtonsListView_SelectionChanged;
      }

      if (optionsListView != null)
      {
        optionsListView.SelectionChanged -= OptionsListView_SelectionChanged;
      }

      hamburgerButton = GetTemplateChild("HamburgerButton") as Button;
      buttonsListView = GetTemplateChild("ButtonsListView") as ListBox;
      optionsListView = GetTemplateChild("OptionsListView") as ListBox;

      if (hamburgerButton != null)
      {
        hamburgerButton.Click += OnHamburgerButtonClick;
      }

      if (buttonsListView != null)
      {
        buttonsListView.SelectionChanged += ButtonsListView_SelectionChanged;
      }

      if (optionsListView != null)
      {
        optionsListView.SelectionChanged += OptionsListView_SelectionChanged;
      }

      ChangeItemFocusVisualStyle();

      Loaded -= HamburgerMenu_Loaded;
      Loaded += HamburgerMenu_Loaded;

      base.OnApplyTemplate();
    }

    private void HamburgerMenu_Loaded(object sender, RoutedEventArgs e)
    {
      if (GetValue(ContentProperty) != null)
      {
        return;
      }

      var item = buttonsListView?.SelectedItem;
      var canRaiseItemEvents = CanRaiseItemEvents(item);
      if (canRaiseItemEvents && RaiseItemEvents(item))
      {
        return;
      }

      var optionItem = optionsListView?.SelectedItem;
      var canRaiseOptionsItemEvents = CanRaiseOptionsItemEvents(optionItem);
      if (canRaiseOptionsItemEvents && RaiseOptionsItemEvents(optionItem))
      {
        return;
      }

      if (canRaiseItemEvents || canRaiseOptionsItemEvents)
      {
        var selectedItem = item ?? optionItem;
        if (selectedItem != null)
        {
          SetCurrentValue(ContentProperty, selectedItem);
        }
      }
    }
  }
}