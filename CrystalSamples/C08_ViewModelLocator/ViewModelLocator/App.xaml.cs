using Crystal;
using Crystal.Unity;
using System.Windows;
using ViewModelLocator.Views;

namespace ViewModelLocator
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
	}
}
