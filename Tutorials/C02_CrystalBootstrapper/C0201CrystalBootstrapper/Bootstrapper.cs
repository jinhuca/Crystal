using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0201CrystalBootstrapper
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell() => Container.Resolve<MainWindow>();
	}
}
