using System.Windows;
using Crystal;

namespace C0102_UnityApp
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
			moduleCatalog.AddModule<C0102_ModuleA.C0102_ModuleAModule>();
			moduleCatalog.AddModule<C0102_ModuleB.C0102_ModuleBModule>();
		}
	}
}
