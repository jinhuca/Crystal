using C33App.Views;
using Crystal;
using System.Windows;

namespace C33App
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<Shell>();
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      base.ConfigureModuleCatalog(moduleCatalog);
    }
  }
}
