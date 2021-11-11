using PassingParameters.Views;
using System.Windows;
using Crystal;

namespace PassingParameters
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
