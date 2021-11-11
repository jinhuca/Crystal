using NavigationParticipation.Views;
using Crystal;
using System.Windows;

namespace NavigationParticipation
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<ModuleA.ModuleAModule>();
  }
}
