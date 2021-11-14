using Crystal;
using ModuleA;
using System.Windows;

namespace LoadModuleManually
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
			var moduleType = typeof(Module);
			moduleCatalog.AddModule(new ModuleInfo
			{
				ModuleName = moduleType.Name,
				ModuleType = moduleType.AssemblyQualifiedName,
				InitializationMode = InitializationMode.OnDemand
			});
		}
	}
}
