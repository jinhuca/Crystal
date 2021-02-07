using Crystal.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegionContext.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private string _title = "Crystal Unity Application";
		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		public MainWindowViewModel()
		{
		}
	}
}
