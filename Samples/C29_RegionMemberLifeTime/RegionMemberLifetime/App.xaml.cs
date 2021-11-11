using Crystal;
using RegionMemberLifetime.Views;
using System.Windows;

namespace RegionMemberLifetime
{
  public partial class App : CrystalApplication
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
