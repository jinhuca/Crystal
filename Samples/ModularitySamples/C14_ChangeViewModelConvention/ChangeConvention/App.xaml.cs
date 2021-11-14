using ChangeConvention.Views;
using Crystal;
using System;
using System.Reflection;
using System.Windows;

namespace ChangeConvention
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

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
