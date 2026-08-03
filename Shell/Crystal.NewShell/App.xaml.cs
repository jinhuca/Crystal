using System.Windows;

namespace Crystal.NewShell;

/// <summary>
/// Interaction logic for App.xaml. A Prism (Unity) application: it creates the shell
/// window and populates the module catalog.
/// </summary>
public partial class App : PrismApplication {
  protected override Window CreateShell() => Container.Resolve<MainWindow>();

  protected override void RegisterTypes(IContainerRegistry containerRegistry) {
  }

  protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
    moduleCatalog.AddModule<CpuModule.CpuModule>();
  }
}
