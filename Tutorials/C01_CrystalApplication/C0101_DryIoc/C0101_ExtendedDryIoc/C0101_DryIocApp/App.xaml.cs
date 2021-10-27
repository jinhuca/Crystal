using System.Windows;
using Crystal;

namespace C0101_DryIocApp
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
			moduleCatalog.AddModule<C0101_ModuleA.C0101_ModuleAModule>();
			moduleCatalog.AddModule<C0101_ModuleB.C0101_ModuleBModule>();
		}
	}
}
