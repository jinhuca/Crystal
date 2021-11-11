using Crystal;
using ModuleA;
using System.Windows;

namespace RegionSimple
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<Shell>();

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
			moduleCatalog.AddModule<ModuleAModule>();
		}
	}
}
