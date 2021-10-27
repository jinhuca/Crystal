using System.Windows;
using Crystal;

namespace C0100_App
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
			moduleCatalog.AddModule<C0100ModuleA.C0100ModuleAModule>();
			moduleCatalog.AddModule<C0100ModuleB.C0100ModuleBModule>();
		}
	}
}
