using Crystal;
using ModuleA;
using RegionNavigation.Views;
using System.Windows;

namespace RegionNavigation
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<ModuleAModule>();

  }
}
