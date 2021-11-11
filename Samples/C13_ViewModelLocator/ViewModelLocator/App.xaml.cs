using Crystal;
using System.Windows;
using ViewModelLocator.Views;

namespace ViewModelLocator
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();
	}
}
