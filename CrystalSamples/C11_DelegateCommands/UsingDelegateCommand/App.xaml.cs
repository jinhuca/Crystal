using Crystal;
using System.Windows;
using UsingDelegateCommand.Views;

namespace UsingDelegateCommand
{
	public partial class App
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
