using System.Windows;
using Crystal;
using Crystal.Unity;

namespace T0202UnityBootstrapper
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<Shell>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
			moduleCatalog.AddModule<T0202UnityModule.T0202UnityModule>();
		}
	}
}
