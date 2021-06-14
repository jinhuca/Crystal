using Crystal;
using Crystal.Unity;
using SimpleDelegateCommand.Views;
using System.Windows;

namespace SimpleDelegateCommand
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
