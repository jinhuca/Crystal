using Crystal.Ioc;
using Crystal.Mvvm;
using Crystal.Unity;
using CustomRegistration.ViewModels;
using System.Windows;

namespace CustomRegistration
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

		protected override void ConfigureViewModelLocator()
		{
			base.ConfigureViewModelLocator();
			ViewModelLocationProvider.Register<MainWindow, CustomViewModel>();
		}
	}
}
