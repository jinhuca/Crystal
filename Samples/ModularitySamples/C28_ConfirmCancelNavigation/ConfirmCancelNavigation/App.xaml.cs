using ConfirmCancelNavigation.Views;
using System.Windows;
using Crystal;

namespace ConfirmCancelNavigation
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
