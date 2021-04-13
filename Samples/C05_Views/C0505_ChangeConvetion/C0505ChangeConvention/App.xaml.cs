using C0505ChangeConvention.Views;
using Crystal;
using Crystal.Unity;
using System;
using System.Reflection;
using System.Windows;

namespace C0505ChangeConvention
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			base.RegisterTypes(containerRegistry);
		}

		protected override void ConfigureViewModelLocator()
		{
			base.ConfigureViewModelLocator();
			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
			{
				var viewName = viewType.FullName;
				var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
				var viewModelName = $"{viewName}ViewModel, {viewAssemblyName}";
				return Type.GetType(viewModelName);
			});
		}
	}
}
