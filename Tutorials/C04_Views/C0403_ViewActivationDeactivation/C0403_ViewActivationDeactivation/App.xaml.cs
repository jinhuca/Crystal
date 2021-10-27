using System.Windows;
using C0403_ViewActivationDeactivation.Views;
using Crystal;

namespace C0403_ViewActivationDeactivation
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
