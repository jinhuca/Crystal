using Crystal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
