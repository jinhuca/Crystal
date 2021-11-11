using Crystal;
using System.Windows;

namespace BasicBootstrapper
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
