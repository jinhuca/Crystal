using C0601_DelegateCommandBasics.Views;
using Crystal;
using System.Windows;

namespace C0601_DelegateCommandBasics
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();
	}
}
