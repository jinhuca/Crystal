namespace Crystal.Themes.Automation.Peers;

public class TabItemExAutomationPeer : TabItemAutomationPeer
{
  public TabItemExAutomationPeer(object owner, TabControlAutomationPeer tabControlAutomationPeer)
    : base(owner, tabControlAutomationPeer)
  {
  }

  protected override List<AutomationPeer> GetChildrenCore()
  {
    // Call the base in case we have children in the header
    var headerChildren = base.GetChildrenCore();

    // Only if the TabItem is selected we need to add its visual children
    if (!(GetWrapper() is TabItem tabItem)
        || tabItem.IsSelected == false)
    {
      return headerChildren;
    }

    if (!(ItemsControlAutomationPeer.Owner is TabControlEx parentTabControl))
    {
      return headerChildren;
    }

    var contentHost = parentTabControl.FindChildContentPresenter(tabItem.Content, tabItem);

    if (contentHost is not null)
    {
      var contentHostPeer = new FrameworkElementAutomationPeer(contentHost);
      var contentChildren = contentHostPeer.GetChildren();

      if (contentChildren is not null)
      {
        if (headerChildren is null)
        {
          headerChildren = contentChildren;
        }
        else
        {
          headerChildren.AddRange(contentChildren);
        }
      }
    }

    return headerChildren;
  }

  private UIElement? GetWrapper()
  {
    var itemsControlAutomationPeer = ItemsControlAutomationPeer;

    var owner = (TabControlEx?)itemsControlAutomationPeer?.Owner;

    if (owner is null)
    {
      return null;
    }

    if (owner.IsItemItsOwnContainer(Item))
    {
      return Item as UIElement;
    }

    return owner.ItemContainerGenerator.ContainerFromItem(Item) as UIElement;
  }
}