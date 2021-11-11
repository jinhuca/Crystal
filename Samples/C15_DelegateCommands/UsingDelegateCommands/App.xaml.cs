using Crystal;
using System.Windows;
using UsingDelegateCommands.Views;

namespace UsingDelegateCommands
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
  }
}
