using System.Windows;
using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Unity;
using ModuleA;
using Modules.Views;

namespace Modules
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			var moduleAType = typeof(ModuleAModule);
			moduleCatalog.AddModule(new ModuleInfo()
			{
				ModuleName = moduleAType.Name,
				ModuleType = moduleAType.AssemblyQualifiedName,
				InitializationMode = InitializationMode.OnDemand
			});
		}
	}
}
