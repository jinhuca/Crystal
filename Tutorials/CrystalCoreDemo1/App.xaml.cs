using System.Windows;
using Crystal;

namespace CrystalCoreDemo1
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
			moduleCatalog.AddModule<ModuleA.ModuleAModule>();
		}
	}
}
