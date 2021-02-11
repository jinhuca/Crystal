using Crystal.Ioc;
using Crystal.Unity;
using System.Windows;

namespace C01BootstrapperSample1
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainView>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			
		}
	}
}
