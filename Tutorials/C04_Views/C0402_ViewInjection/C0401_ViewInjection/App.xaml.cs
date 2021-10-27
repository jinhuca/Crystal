using System.Windows;
using C0401_ViewInjection.Views;
using Crystal;

namespace C0401_ViewInjection
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
