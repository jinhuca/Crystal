using Crystal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C33App.ViewModels
{
  internal class ShellViewModel : BindableBase
  {
    private string _title = "Crystal Application";
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }
    public ShellViewModel()
    {
    }
  }
}
