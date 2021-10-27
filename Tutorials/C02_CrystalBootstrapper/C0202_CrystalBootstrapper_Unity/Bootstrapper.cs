using System.Windows;
using Crystal;
using Crystal.Unity;

namespace C0202_CrystalBootstrapper_Unity
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
