using System.Reflection;
using Crystal.Themes.Automation.Peers;
using Crystal.Themes.Internal;

namespace Crystal.Themes.Controls;

/// <summary>
/// The standard WPF TabControl is quite bad in the fact that it only
/// even contains the current TabItem in the VisualTree, so if you
/// have complex views it takes a while to re-create the view each tab
/// selection change.Which makes the standard TabControl very sticky to
/// work with. This class along with its associated ControlTemplate
/// allow all TabItems to remain in the VisualTree without it being Sticky.
/// It does this by keeping all TabItem content in the VisualTree but
/// hides all inactive TabItem content, and only keeps the active TabItem
/// content shown.
/// 
/// Acknowledgement
///     Eric Burke
///         http://eric.burke.name/dotnetmania/2009/04/26/22.09.28
///     Sacha Barber: https://sachabarbs.wordpress.com/about-me/
///         http://stackoverflow.com/a/10210889/920384
///     http://stackoverflow.com/a/7838955/920384
/// </summary>
/// <remarks>
/// We use two attached properties to later recognize the content presenters we generated.
/// We need the OwningItem because the TabItem associated with an item can later change.
///
/// We need the OwningTabItem to reduce the amount of lookups we have to do.
/// </remarks>
[TemplatePart(Name = "PART_HeaderPanel", Type = typeof(Panel))]
[TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
public class TabControlEx : TabControl
{
  private static readonly MethodInfo? updateSelectedContentMethodInfo = typeof(TabControl).GetMethod("UpdateSelectedContent", BindingFlags.NonPublic | BindingFlags.Instance);

  private Panel? itemsHolder;

  public static readonly DependencyProperty ChildContentVisibilityProperty = DependencyProperty.Register(
    nameof(ChildContentVisibility),
    typeof(Visibility),
    typeof(TabControlEx),
    new PropertyMetadata(Visibility.Visible));

  public static readonly DependencyProperty TabPanelVisibilityProperty = DependencyProperty.Register(
    nameof(TabPanelVisibility),
    typeof(Visibility),
    typeof(TabControlEx),
    new PropertyMetadata(Visibility.Visible));

  public static readonly DependencyProperty OwningTabItemProperty = DependencyProperty.RegisterAttached(
    "OwningTabItem",
    typeof(TabItem),
    typeof(TabControlEx),
    new PropertyMetadata(default(TabItem)));

  /// <summary>Helper for getting <see cref="OwningTabItemProperty"/> from <paramref name="element"/>.</summary>
  /// <param name="element"><see cref="DependencyObject"/> to read <see cref="OwningTabItemProperty"/> from.</param>
  /// <returns>OwningTabItem property value.</returns>
  [AttachedPropertyBrowsableForType(typeof(ContentPresenter))]
  public static TabItem? GetOwningTabItem(DependencyObject element) => (TabItem?)element.GetValue(OwningTabItemProperty);

  /// <summary>Helper for setting <see cref="OwningTabItemProperty"/> on <paramref name="element"/>.</summary>
  /// <param name="element"><see cref="DependencyObject"/> to set <see cref="OwningTabItemProperty"/> on.</param>
  /// <param name="value">OwningTabItem property value.</param>
  public static void SetOwningTabItem(DependencyObject element, TabItem? value) => element.SetValue(OwningTabItemProperty, value);

  public static readonly DependencyProperty OwningItemProperty = DependencyProperty.RegisterAttached(
    "OwningItem",
    typeof(object),
    typeof(TabControlEx),
    new PropertyMetadata(default(object)));

  /// <summary>Helper for setting <see cref="OwningItemProperty"/> on <paramref name="element"/>.</summary>
  /// <param name="element"><see cref="DependencyObject"/> to set <see cref="OwningItemProperty"/> on.</param>
  /// <param name="value">OwningItem property value.</param>
  public static void SetOwningItem(DependencyObject element, object? value) => element.SetValue(OwningItemProperty, value);

  /// <summary>Helper for getting <see cref="OwningItemProperty"/> from <paramref name="element"/>.</summary>
  /// <param name="element"><see cref="DependencyObject"/> to read <see cref="OwningItemProperty"/> from.</param>
  /// <returns>OwningItem property value.</returns>
  [AttachedPropertyBrowsableForType(typeof(ContentPresenter))]
  public static object? GetOwningItem(DependencyObject element) => element.GetValue(OwningItemProperty);

  public static readonly DependencyProperty MoveFocusToContentWhenSelectionChangesProperty = DependencyProperty.Register(
    nameof(MoveFocusToContentWhenSelectionChanges),
    typeof(bool),
    typeof(TabControlEx),
    new PropertyMetadata(default(bool)));

  /// <summary>
  /// Gets or sets whether keyboard focus should be moved to the content area when the selected item changes.
  /// </summary>
  public bool MoveFocusToContentWhenSelectionChanges
  {
    get => (bool)GetValue(MoveFocusToContentWhenSelectionChangesProperty);
    set => SetValue(MoveFocusToContentWhenSelectionChangesProperty, value);
  }

  static TabControlEx()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControlEx), new FrameworkPropertyMetadata(typeof(TabControlEx)));
  }

  public TabControlEx()
  {
    Unloaded += HandleTabControlExUnloaded;
  }

  /// <summary>
  /// Defines if the TabPanel (Tab-Header) are visible.
  /// </summary>
  public Visibility TabPanelVisibility
  {
    get => (Visibility)GetValue(TabPanelVisibilityProperty);
    set => SetValue(TabPanelVisibilityProperty, value);
  }

  /// <summary>
  /// Gets or sets the child content visibility.
  /// </summary>
  /// <value>
  /// The child content visibility.
  /// </value>
  public Visibility ChildContentVisibility
  {
    get => (Visibility)GetValue(ChildContentVisibilityProperty);
    set => SetValue(ChildContentVisibilityProperty, value);
  }

  /// <inheritdoc/>
  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    var newItemsHolder = Template.FindName("PART_ItemsHolder", this) as Panel;
    var isDifferentItemsHolder = ReferenceEquals(itemsHolder, newItemsHolder) == false;

    if (isDifferentItemsHolder)
    {
      ClearItemsHolder();
    }

    itemsHolder = newItemsHolder;

    if (isDifferentItemsHolder)
    {
      UpdateSelectedContent();
    }
  }

  /// <inheritdoc/>
  protected override void OnInitialized(EventArgs e)
  {
    base.OnInitialized(e);

    ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
    ItemContainerGenerator.ItemsChanged += OnGeneratorItemsChanged;
  }

  /// <inheritdoc/>
  /// <summary>
  /// When the items change we remove any generated panel children and add any new ones as necessary.
  /// </summary>
  protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
  {
    base.OnItemsChanged(e);

    if (itemsHolder is null)
    {
      return;
    }

    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Reset:
        RefreshItemsHolder();
        break;

      case NotifyCollectionChangedAction.Add:
      case NotifyCollectionChangedAction.Remove:
        if (e.OldItems is not null)
        {
          foreach (var item in e.OldItems)
          {
            var contentPresenter = FindChildContentPresenter(item, null);

            if (contentPresenter is not null)
            {
              itemsHolder.Children.Remove(contentPresenter);
            }
          }
        }

        // don't do anything with new items because we don't want to
        // create visuals that aren't being shown yet
        UpdateSelectedContent();
        break;

      case NotifyCollectionChangedAction.Replace:
        // Replace is not really implemented yet
        RefreshItemsHolder();
        break;
    }
  }

  /// <inheritdoc/>
  protected override void OnSelectionChanged(SelectionChangedEventArgs e)
  {
    // If we don't have an items holder we can safely forward the call to base
    if (itemsHolder is null)
    {
      base.OnSelectionChanged(e);
      return;
    }

    // We must NOT call base.OnSelectionChanged because that would interfere with our ability to update the selected content before the automation events are fired etc.

    if (FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
    {
      RaiseSelectionChangedEvent(e);

      if (IsKeyboardFocusWithin)
      {
        GetSelectedTabItem()?.SetFocus();
      }

      UpdateSelectedContent();
    }
    else
    {
      var keyboardFocusWithin = IsKeyboardFocusWithin;
      UpdateSelectedContent();

      if (keyboardFocusWithin)
      {
        GetSelectedTabItem()?.SetFocus();
      }

      RaiseSelectionChangedEvent(e);
    }

    if (!AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated)
        && !AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected)
        && (!AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection)
            && !AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)))
    {
      return;
    }
    (UIElementAutomationPeer.CreatePeerForElement(this) as TabControlAutomationPeer)?.RaiseSelectionEvents(e);
  }

  private void RaiseSelectionChangedEvent(SelectionChangedEventArgs e) => RaiseEvent(e);

  /// <inheritdoc/>
  protected override void OnKeyDown(KeyEventArgs e)
  {
    // We need this to prevent the base class to always accept CTRL + TAB navigation regardless of which keyboard navigation mode is set for this control
    if (e.Key == Key.Tab && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
    {
      var controlTabNavigation = KeyboardNavigation.GetControlTabNavigation(this);

      if (controlTabNavigation != KeyboardNavigationMode.Cycle && controlTabNavigation != KeyboardNavigationMode.Continue)
      {
        return;
      }
    }

    base.OnKeyDown(e);
  }

  /// <inheritdoc/>
  protected override AutomationPeer OnCreateAutomationPeer()
  {
    // If we don't have an items holder we can safely forward the call to base
    if (itemsHolder is null)
    {
      return base.OnCreateAutomationPeer();
    }

    var automationPeer = new TabControlExAutomationPeer(this);
    return automationPeer;
  }

  /// <summary>
  /// Copied from <see cref="TabControl"/>. wish it were protected in that class instead of private.
  /// </summary>
  public TabItem? GetSelectedTabItem()
  {
    var selectedItem = SelectedItem;
    if (selectedItem is null)
    {
      return null;
    }

    var tabItem = selectedItem as TabItem;
    if (tabItem is null)
    {
      tabItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem;

      if (tabItem is null
          // is this really the container we wanted?
          || ReferenceEquals(selectedItem, ItemContainerGenerator.ItemFromContainer(tabItem)) == false)
      {
        tabItem = ItemContainerGenerator.ContainerFromItem(selectedItem) as TabItem;
      }
    }

    return tabItem;
  }

  private void ClearItemsHolder()
  {
    if (itemsHolder is null)
    {
      return;
    }

    foreach (var itemsHolderChild in itemsHolder.Children)
    {
      var contentPresenter = itemsHolderChild as ContentPresenter;

      contentPresenter?.ClearValue(OwningItemProperty);
      contentPresenter?.ClearValue(OwningTabItemProperty);
    }

    itemsHolder.Children.Clear();
  }

  /// <summary>
  /// Clears all current children by calling <see cref="ClearItemsHolder"/> and calls <see cref="UpdateSelectedContent"/> afterwards.
  /// </summary>
  private void RefreshItemsHolder()
  {
    ClearItemsHolder();
    UpdateSelectedContent();
  }

  private void HandleTabControlExLoaded(object? sender, RoutedEventArgs e)
  {
    Loaded -= HandleTabControlExLoaded;
    UpdateSelectedContent();
  }

  private void HandleTabControlExUnloaded(object? sender, RoutedEventArgs e)
  {
    Loaded -= HandleTabControlExLoaded;
    Loaded += HandleTabControlExLoaded;
    ClearItemsHolder();
  }

  private void OnGeneratorStatusChanged(object? sender, EventArgs e)
  {
    if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
    {
      return;
    }
    UpdateSelectedContent();
  }

  private void OnGeneratorItemsChanged(object? sender, ItemsChangedEventArgs e)
  {
    // We only care about reset.
    // Reset, in case of ItemContainerGenerator, is generated when it's refreshed.
    // It gets refresh when things like ItemContainerStyleSelector etc. change.
    if (e.Action == NotifyCollectionChangedAction.Reset)
    {
      RefreshItemsHolder();
    }
  }

  /// <summary>
  /// Generate a ContentPresenter for the selected item and control the visibility of already created presenters.
  /// </summary>
  private void UpdateSelectedContent()
  {
    if (itemsHolder is null)
    {
      return;
    }

    updateSelectedContentMethodInfo?.Invoke(this, null);

    var selectedTabItem = GetSelectedTabItem();

    if (selectedTabItem is not null)
    {
      // generate a ContentPresenter if necessary
      CreateChildContentPresenterIfRequired(SelectedItem, selectedTabItem);
    }

    // show the right child
    foreach (ContentPresenter? contentPresenter in itemsHolder.Children)
    {
      if (contentPresenter is null)
      {
        continue;
      }

      var tabItem = (TabItem?)ItemContainerGenerator.ContainerFromItem(GetOwningItem(contentPresenter)) ?? GetOwningTabItem(contentPresenter);

      // Hide all non selected items and show the selected item
      if (tabItem?.IsSelected == true)
      {
        contentPresenter.Visibility = Visibility.Visible;
        contentPresenter.HorizontalAlignment = tabItem.HorizontalContentAlignment;
        contentPresenter.VerticalAlignment = tabItem.VerticalContentAlignment;

        if (tabItem.ContentTemplate is null
            && tabItem.ContentTemplateSelector is null
            && tabItem.ContentStringFormat is null)
        {
          contentPresenter.ContentTemplate = ContentTemplate;
          contentPresenter.ContentTemplateSelector = ContentTemplateSelector;
          contentPresenter.ContentStringFormat = ContentStringFormat;
        }
        else
        {
          contentPresenter.ContentTemplate = tabItem.ContentTemplate;
          contentPresenter.ContentTemplateSelector = tabItem.ContentTemplateSelector;
          contentPresenter.ContentStringFormat = tabItem.ContentStringFormat;
        }
      }
      else
      {
        contentPresenter.Visibility = Visibility.Collapsed;
      }

      if (tabItem is not null && MoveFocusToContentWhenSelectionChanges)
      {
        MoveFocusToContent(contentPresenter, tabItem);
      }
    }
  }

  /// <summary>
  /// Create the child ContentPresenter for the given item (could be data or a TabItem) if none exists.
  /// </summary>
  private void CreateChildContentPresenterIfRequired(object? item, TabItem tabItem)
  {
    if (item is null)
    {
      return;
    }

    // Jump out if we already created a ContentPresenter for this item
    if (FindChildContentPresenter(item, tabItem) is not null)
    {
      return;
    }

    // the actual child to be added
    var contentPresenter = new ContentPresenter
    {
      Content = item is TabItem itemAsTabItem ? itemAsTabItem.Content : item,
      Visibility = Visibility.Collapsed
    };

    var owningTabItem = item as TabItem ?? (TabItem)ItemContainerGenerator.ContainerFromItem(item);

    if (owningTabItem is null)
    {
      throw new Exception("No owning TabItem could be found.");
    }

    SetOwningItem(contentPresenter, item);
    SetOwningTabItem(contentPresenter, owningTabItem);

    itemsHolder?.Children.Add(contentPresenter);
  }

  /// <summary>
  /// Find the <see cref="ContentPresenter"/> for the given object. Data could be a TabItem or a piece of data.
  /// </summary>
  public ContentPresenter? FindChildContentPresenter(object? item, TabItem? tabItem)
  {
    if (item is null)
    {
      return null;
    }

    if (itemsHolder is null)
    {
      return null;
    }

    var contentPresenters = itemsHolder.Children.OfType<ContentPresenter>().ToList();

    if (tabItem is not null)
    {
      return contentPresenters.FirstOrDefault(contentPresenter => ReferenceEquals(GetOwningTabItem(contentPresenter), tabItem))
             ?? contentPresenters.FirstOrDefault(contentPresenter => ReferenceEquals(GetOwningItem(contentPresenter), item));
    }

    return contentPresenters.FirstOrDefault(contentPresenter => ReferenceEquals(GetOwningItem(contentPresenter), item))
           ?? contentPresenters.FirstOrDefault(contentPresenter => ReferenceEquals(contentPresenter.Content, item));
  }

  private void MoveFocusToContent(ContentPresenter contentPresenter, TabItem tabItem)
  {
    // Do nothing if the item is not visible or already has keyboard focus
    if (contentPresenter.Visibility != Visibility.Visible || contentPresenter.IsKeyboardFocusWithin)
    {
      return;
    }

    var presenter = contentPresenter;

    Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
    {
      tabItem.BringIntoView();
      presenter.BringIntoView();
      if (presenter.IsKeyboardFocusWithin == false)
      {
        presenter.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
      }
    });
  }
}