using System.Windows;
using Crystal;
using System.Collections.Generic;
using System.Text;

namespace Container.Shared.Mocks
{
	internal partial class NullModuleCatalogBootstrapper
	{
		protected override IModuleCatalog CreateModuleCatalog()
		{
			return null;
		}

		protected override DependencyObject CreateShell()
		{
			return null;
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}
  }
}
