namespace Crystal.Themes.Controls;

public partial class HamburgerMenu
{
  public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent(
    nameof(ItemClick),
    RoutingStrategy.Direct,
    typeof(ItemClickRoutedEventHandler),
    typeof(HamburgerMenu));

  public event ItemClickRoutedEventHandler ItemClick
  {
    add => AddHandler(ItemClickEvent, value);
    remove => RemoveHandler(ItemClickEvent, value);
  }

  public static readonly RoutedEvent OptionsItemClickEvent = EventManager.RegisterRoutedEvent(
    nameof(OptionsItemClick),
    RoutingStrategy.Direct,
    typeof(ItemClickRoutedEventHandler),
    typeof(HamburgerMenu));

  public event ItemClickRoutedEventHandler OptionsItemClick
  {
    add => AddHandler(OptionsItemClickEvent, value);
    remove => RemoveHandler(OptionsItemClickEvent, value);
  }

  public static readonly RoutedEvent ItemInvokedEvent = EventManager.RegisterRoutedEvent(
    nameof(ItemInvoked),
    RoutingStrategy.Direct,
    typeof(HamburgerMenuItemInvokedRoutedEventHandler),
    typeof(HamburgerMenu));

  public event HamburgerMenuItemInvokedRoutedEventHandler ItemInvoked
  {
    add => AddHandler(ItemInvokedEvent, value);
    remove => RemoveHandler(ItemInvokedEvent, value);
  }

  public static readonly RoutedEvent HamburgerButtonClickEvent = EventManager.RegisterRoutedEvent(
    nameof(HamburgerButtonClick),
    RoutingStrategy.Direct,
    typeof(RoutedEventHandler),
    typeof(HamburgerMenu));

  public event RoutedEventHandler HamburgerButtonClick
  {
    add => AddHandler(HamburgerButtonClickEvent, value);
    remove => RemoveHandler(HamburgerButtonClickEvent, value);
  }

  private void OnHamburgerButtonClick(object sender, RoutedEventArgs e)
  {
    var args = new RoutedEventArgs(HamburgerButtonClickEvent, sender);
    RaiseEvent(args);

    if (!args.Handled)
    {
      IsPaneOpen = !IsPaneOpen;
    }
  }

  private bool OnItemClick()
  {
    var selectedItem = buttonsListView?.SelectedItem;

    if (!CanRaiseItemEvents(selectedItem))
    {
      return false;
    }

    (selectedItem as HamburgerMenuItem)?.RaiseCommand();
    RaiseItemCommand();

    var raiseItemEvents = RaiseItemEvents(selectedItem);
    if (raiseItemEvents && optionsListView != null)
    {
      optionsListView.SelectedIndex = -1;
    }

    return raiseItemEvents;
  }

  private bool CanRaiseItemEvents(object? selectedItem)
  {
    if (selectedItem is null)
    {
      return false;
    }

    if (selectedItem is IHamburgerMenuHeaderItem || selectedItem is IHamburgerMenuSeparatorItem)
    {
      if (buttonsListView != null)
      {
        buttonsListView.SelectedIndex = -1;
      }

      return false;
    }

    return true;
  }

  private bool RaiseItemEvents(object? selectedItem)
  {
    if (selectedItem is null)
    {
      return false;
    }

    var itemClickEventArgs = new ItemClickEventArgs(ItemClickEvent, this) { ClickedItem = selectedItem };
    RaiseEvent(itemClickEventArgs);

    var hamburgerMenuItemInvokedEventArgs = new HamburgerMenuItemInvokedEventArgs(ItemInvokedEvent, this) { InvokedItem = selectedItem, IsItemOptions = false };
    RaiseEvent(hamburgerMenuItemInvokedEventArgs);

    return !itemClickEventArgs.Handled && !hamburgerMenuItemInvokedEventArgs.Handled;
  }

  private bool OnOptionsItemClick()
  {
    var selectedItem = optionsListView?.SelectedItem;

    if (!CanRaiseOptionsItemEvents(selectedItem))
    {
      return false;
    }

    (selectedItem as HamburgerMenuItem)?.RaiseCommand();
    RaiseOptionsItemCommand();

    var raiseOptionsItemEvents = RaiseOptionsItemEvents(selectedItem);
    if (raiseOptionsItemEvents && buttonsListView != null)
    {
      buttonsListView.SelectedIndex = -1;
    }

    return raiseOptionsItemEvents;
  }

  private bool CanRaiseOptionsItemEvents(object? selectedItem)
  {
    if (selectedItem is null)
    {
      return false;
    }

    if (selectedItem is IHamburgerMenuHeaderItem || selectedItem is IHamburgerMenuSeparatorItem)
    {
      if (optionsListView != null)
      {
        optionsListView.SelectedIndex = -1;
      }

      return false;
    }

    return true;
  }

  private bool RaiseOptionsItemEvents(object? selectedItem)
  {
    if (selectedItem is null)
    {
      return false;
    }

    if (selectedItem is IHamburgerMenuHeaderItem || selectedItem is IHamburgerMenuSeparatorItem)
    {
      return false;
    }

    var itemClickEventArgs = new ItemClickEventArgs(OptionsItemClickEvent, this) { ClickedItem = selectedItem };
    RaiseEvent(itemClickEventArgs);

    var hamburgerMenuItemInvokedEventArgs = new HamburgerMenuItemInvokedEventArgs(ItemInvokedEvent, this) { InvokedItem = selectedItem, IsItemOptions = true };
    RaiseEvent(hamburgerMenuItemInvokedEventArgs);

    return !itemClickEventArgs.Handled && !hamburgerMenuItemInvokedEventArgs.Handled;
  }

  private void ButtonsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (sender is not ListBox listBox)
    {
      return;
    }

    listBox.SelectionChanged -= ButtonsListView_SelectionChanged;

    if (e.AddedItems != null && e.AddedItems.Count > 0)
    {
      var canItemClick = OnItemClick();

      if (!canItemClick)
      {
        // The following lines will fire another SelectionChanged event.
        if (e.RemovedItems.Count > 0)
        {
          listBox.SelectedItem = e.RemovedItems[0];
        }
        else
        {
          listBox.SelectedIndex = -1;
        }
      }
    }

    listBox.SelectionChanged += ButtonsListView_SelectionChanged;
  }

  private void OptionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (sender is not ListBox listBox)
    {
      return;
    }

    listBox.SelectionChanged -= OptionsListView_SelectionChanged;

    if (e.AddedItems != null && e.AddedItems.Count > 0)
    {
      var canItemClick = OnOptionsItemClick();

      if (!canItemClick)
      {
        // The following lines will fire another SelectionChanged event.
        if (e.RemovedItems.Count > 0)
        {
          listBox.SelectedItem = e.RemovedItems[0];
        }
        else
        {
          listBox.SelectedIndex = -1;
        }
      }
    }

    listBox.SelectionChanged += OptionsListView_SelectionChanged;
  }
}