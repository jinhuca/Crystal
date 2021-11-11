using ActiveAwareCommands.Views;
using CompositeCommandShared;
using Crystal;
using ModuleA;
using System.Windows;

namespace ActiveAwareCommands
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<ModuleAModule>();
    protected override void RegisterTypes(IContainerRegistry containerRegistry) => containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();

  }
}
