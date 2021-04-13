using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0403ModuleApp
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			var temp = new DirectoryModuleCatalog() { ModulePath = @".\ModuleApp" };
			return new DirectoryModuleCatalog() { ModulePath = @".\ModuleApp" };
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			base.RegisterTypes(containerRegistry);
		}
	}
}
