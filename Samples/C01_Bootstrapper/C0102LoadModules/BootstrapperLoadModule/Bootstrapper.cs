using C0102Module;
using Crystal;
using System.Windows;

namespace C0102BootstrapperModule
{
  public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<Module>();
		}
	}
}
