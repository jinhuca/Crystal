using System.Windows;
using Crystal;

namespace C0504_LoadManually.Views
{
	public partial class MainWindow
	{
		private readonly IModuleManager _moduleManager;

		public MainWindow(IModuleManager moduleManager)
		{
			InitializeComponent();
			_moduleManager = moduleManager;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_moduleManager.LoadModule("ModuleAModule");
		}
	}
}
