using System.Windows;
using BootstrapperShell.Views;
using Crystal;
using Crystal.Desktop;

namespace BootstrapperShell
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell() => Container.Resolve<MainWindow>();
	}
}
