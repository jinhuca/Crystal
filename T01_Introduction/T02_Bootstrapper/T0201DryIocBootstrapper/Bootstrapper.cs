using System.Windows;
using Crystal;
using Crystal.DryIoc;

namespace T0102DryIocBootstrapper
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
			moduleCatalog.AddModule<T0201DryIocModule.T0201DryIocModule>();
		}
	}
}
  