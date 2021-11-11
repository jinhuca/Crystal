using Crystal;
using ModuleA.ViewModels;
using ModuleA.Views;

namespace ModuleA
{
  public class ModuleAModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      var regionManager = containerProvider.Resolve<IRegionManager>();
      IRegion region = regionManager.Regions["ContentRegion"];

      var tabA = containerProvider.Resolve<TabView>();
      SetTitle(tabA, "Tab A");
      tabA.Background = System.Windows.Media.Brushes.LightBlue;
      region.Add(tabA);

      var tabB = containerProvider.Resolve<TabView>();
      SetTitle(tabB, "Tab B");
      tabB.Background = System.Windows.Media.Brushes.LightCyan;
      region.Add(tabB);

      var tabC = containerProvider.Resolve<TabView>();
      SetTitle(tabC, "Tab C");
      tabC.Background = System.Windows.Media.Brushes.LightGreen;
      region.Add(tabC);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    void SetTitle(TabView tab, string title)
    {
      var temp = tab.DataContext as TabViewModel;
      if(temp != null) temp.Title = title;
    }
  }
}
