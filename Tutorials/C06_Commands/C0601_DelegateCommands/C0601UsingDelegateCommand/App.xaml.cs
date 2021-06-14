using C0601UsingDelegateCommand.Views;
using Crystal;
using System.Windows;

namespace C0601UsingDelegateCommand
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
