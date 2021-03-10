using Crystal.Unity;
using Crystal.Ioc;
using System.Windows;

namespace C0201CrystalBootstrapper
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell() => Container.Resolve<MainWindow>();
	}
}
