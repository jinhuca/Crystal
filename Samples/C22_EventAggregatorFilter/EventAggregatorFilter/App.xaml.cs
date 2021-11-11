using Crystal;
using EventAggregatorFilter.Views;
using ModuleA;
using ModuleB;
using System.Windows;

namespace EventAggregatorFilter
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule<ModuleAModule>();
      moduleCatalog.AddModule<ModuleBModule>();
    }
  }
}
