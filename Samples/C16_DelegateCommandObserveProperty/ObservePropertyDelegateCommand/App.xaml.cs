using Crystal;
using ObservePropertyDelegateCommand.Views;
using System.Windows;

namespace ObservePropertyDelegateCommand
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
  }
}
