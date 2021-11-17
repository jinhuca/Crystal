namespace Crystal.Themes.Controls
{
  /// <summary>
  /// A base class for every CrystalTabControl (Pivot).
  /// </summary>
  public abstract class CrystalTabControlBase : TabControlEx
  {
    static CrystalTabControlBase()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(CrystalTabControlBase), new FrameworkPropertyMetadata(typeof(CrystalTabControlBase)));
    }

    public static readonly DependencyProperty TabStripMarginProperty = DependencyProperty.Register(
      nameof(TabStripMargin),
      typeof(Thickness),
      typeof(CrystalTabControlBase),
      new PropertyMetadata(new Thickness(0)));

    public Thickness TabStripMargin
    {
      get => (Thickness)GetValue(TabStripMarginProperty);
      set => SetValue(TabStripMarginProperty, value);
    }

    public static readonly DependencyProperty CloseTabCommandProperty = DependencyProperty.Register(
      nameof(CloseTabCommand),
      typeof(ICommand),
      typeof(CrystalTabControlBase),
      new PropertyMetadata(null));

    public ICommand? CloseTabCommand
    {
      get => (ICommand?)GetValue(CloseTabCommandProperty);
      set => SetValue(CloseTabCommandProperty, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
      return item is TabItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
      return new CrystalTabItem(); //Overrides the TabControl's default behavior and returns a CrystalTabItem instead of a regular one.
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      if (element != item)
      {
        element.SetValue(DataContextProperty, item); //dont want to set the datacontext to itself.
      }

      base.PrepareContainerForItemOverride(element, item);
    }

    public delegate void TabItemClosingEventHandler(object sender, TabItemClosingEventArgs e);

    public event TabItemClosingEventHandler? TabItemClosingEvent;

    internal bool RaiseTabItemClosingEvent(CrystalTabItem closingItem)
    {
      var tabItemClosingEvent = TabItemClosingEvent;
      if (tabItemClosingEvent != null)
      {
        foreach (TabItemClosingEventHandler subHandler in tabItemClosingEvent.GetInvocationList().OfType<TabItemClosingEventHandler>())
        {
          var args = new TabItemClosingEventArgs(closingItem);
          subHandler.Invoke(this, args);
          if (args.Cancel)
          {
            return true;
          }
        }
      }

      return false;
    }

    public class TabItemClosingEventArgs : CancelEventArgs
    {
      internal TabItemClosingEventArgs(CrystalTabItem item)
      {
        ClosingTabItem = item;
      }

      public CrystalTabItem ClosingTabItem { get; private set; }
    }

    internal void CloseThisTabItem([NotNull] CrystalTabItem tabItem)
    {
      if (tabItem is null)
      {
        throw new ArgumentNullException(nameof(tabItem));
      }

      if (CloseTabCommand != null)
      {
        var closeTabCommandParameter = tabItem.CloseTabCommandParameter ?? tabItem;
        if (CloseTabCommand.CanExecute(closeTabCommandParameter))
        {
          CloseTabCommand.Execute(closeTabCommandParameter);
        }
      }
      else
      {
        // KIDS: don't try this at home
        // this is not good MVVM habits and I'm only doing it
        // because I want the demos to be absolutely bitching

        // the control is allowed to cancel this event
        if (RaiseTabItemClosingEvent(tabItem))
        {
          return;
        }

        if (ItemsSource is null)
        {
          // if the list is hard-coded (i.e. has no ItemsSource)
          // then we remove the item from the collection
          tabItem.ClearStyle();
          Items.Remove(tabItem);
        }
        else
        {
          // if ItemsSource is something we cannot work with, bail out
          var collection = ItemsSource as IList;
          if (collection is null)
          {
            return;
          }

          // find the item and kill it (I mean, remove it)
          var item2Remove = collection.OfType<object>().FirstOrDefault(item => tabItem == item || tabItem.DataContext == item);
          if (item2Remove != null)
          {
            tabItem.ClearStyle();
            collection.Remove(item2Remove);
          }
        }
      }
    }
  }
}