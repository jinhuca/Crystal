using Crystal;
using GenericDelegateCommand.Views;
using System.Windows;

namespace GenericDelegateCommand
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
  }
}
