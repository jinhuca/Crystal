using System.Windows;
using C0601_DelegateCommands.Views;
using Crystal;

namespace C0601_DelegateCommands
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
