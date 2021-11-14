using Crystal;
using ModuleA;
using NavigateToViews.Views;
using System.Windows;

namespace NavigateToViews
{
  public partial class App : CrystalApplication
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<ModuleAModule>();
  }
}
