using System.Windows;
using C0301_ModuleA;
using C0301_ModuleB;
using Crystal;

namespace C0301_RegionApp
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<Shell>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
			moduleCatalog.AddModule<C0301ModuleAModule>();
			moduleCatalog.AddModule<C0301ModuleBModule>();
		}
	}
}
