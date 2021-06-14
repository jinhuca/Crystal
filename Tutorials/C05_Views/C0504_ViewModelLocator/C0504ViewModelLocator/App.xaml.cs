using C0504ViewModelLocator.Views;
using Crystal.Unity;
using Crystal;
using System.Windows;

namespace C0504ViewModelLocator
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
