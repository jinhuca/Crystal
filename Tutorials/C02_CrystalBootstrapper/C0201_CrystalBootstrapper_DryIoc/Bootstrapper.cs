using System.Windows;
using Crystal;
using Crystal.DryIoc;

namespace C0201_CrystalBootstrapper_DryIoc
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
