using Crystal;
using System.IO;
using System.Windows;

namespace LoadModuleFromDirectory
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

		protected override IModuleCatalog CreateModuleCatalog()
		{
			var mPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Modules");
			return new DirectoryModuleCatalog { ModulePath = mPath };
		}
	}
}
