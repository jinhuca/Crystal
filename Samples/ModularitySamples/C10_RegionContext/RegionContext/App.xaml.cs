using Crystal;
using ModuleA;
using RegionContext.Views;
using System.Windows;

namespace RegionContext
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<ModuleAModule>();
  }
}
