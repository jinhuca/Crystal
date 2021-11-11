using Crystal;
using NavigationJournal.Views;
using System.Windows;

namespace NavigationJournal
{
  public partial class App
  {
    protected override Window CreateShell()
    {
      return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule<ModuleA.ModuleAModule>();
    }
  }
}
