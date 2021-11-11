using Crystal;
using System.Windows;
using UsingInvokeCommandAction.Views;

namespace UsingInvokeCommandAction
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
  }
}
