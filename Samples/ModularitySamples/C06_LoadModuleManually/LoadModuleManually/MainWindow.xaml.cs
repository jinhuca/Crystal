using Crystal;
using System.Windows;

namespace LoadModuleManually
{
  public partial class MainWindow
	{
		private IModuleManager _moduleManager;

		public MainWindow(IModuleManager moduleManager)
		{
			InitializeComponent();
			_moduleManager = moduleManager;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_moduleManager.LoadModule("Module");
		}
	}
}
