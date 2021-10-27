using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using C0502_ModuleA.Annotations;
using Crystal;

namespace C0502_ModuleA.ViewModels
{
  public class ViewAViewModel : BindableBase
  {
    private int _countOfItems;
    public int CountOfItems
    {
      get => _countOfItems;
      set => SetProperty(ref _countOfItems, value);
    }

    private bool _isEnabled;

    public bool IsEnabled
    {
      get => _isEnabled;
      set => SetProperty(ref _isEnabled, value);
    }

    public DelegateCommand AddCountCommand { get; }

    public ViewAViewModel()
    {
      AddCountCommand = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);
      IsEnabled = true;
    }

    private void Execute()
    {
      CountOfItems++;
    }
  }
}
