using System.Windows;
using C0504_LoadManually.Views;
using C0504_ModuleA;
using Crystal;

namespace C0504_LoadManually
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			var moduleAType = typeof(ModuleAModule);
			moduleCatalog.AddModule(new ModuleInfo
			{
				ModuleName = moduleAType.Name,
				ModuleType = moduleAType.AssemblyQualifiedName,
				InitializationMode = InitializationMode.OnDemand
			});
		}
	}
}
