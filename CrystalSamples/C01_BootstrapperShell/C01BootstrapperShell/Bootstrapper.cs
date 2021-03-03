using C01BootstrapperShell.Views;
using Crystal.Ioc;
using Crystal.Unity;
using System.Windows;

namespace C01BootstrapperShell
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<View1>();
		}
	}
}
