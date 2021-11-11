using Crystal;
using System.Windows;

using UsingEventAggregator.Views;

namespace UsingEventAggregator
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
      moduleCatalog.AddModule<ModuleB.ModuleBModule>();
    }
  }
}
