using System.Windows;
using Crystal.Unity;
using ModuleA;
using Modules.Views;
using Crystal;

namespace Modules
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
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
