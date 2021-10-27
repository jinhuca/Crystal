using System.Windows;
using Crystal;

namespace C0401_ViewDiscovery
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
