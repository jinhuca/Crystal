using Crystal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModelLocator.ViewModels
{
	public class CustomViewModel : BindableBase
  {
    private string _title = "Custom ViewModel Application";
    public string Title
    {
      get { return _title; }
      set { SetProperty(ref _title, value); }
    }

    public CustomViewModel()
    {

    }
  }
}
