using Modules.Views;
using System.Windows;
using System;
using Crystal.Unity;
using Crystal;

namespace Modules
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : CrystalApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new XamlModuleCatalog(new Uri("/Modules;component/ModuleCatalog.xaml", UriKind.Relative));
        }
    }
}
