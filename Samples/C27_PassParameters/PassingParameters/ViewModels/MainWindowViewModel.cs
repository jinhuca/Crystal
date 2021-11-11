using Crystal;

namespace PassingParameters.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private string _title = "Crystal Application";
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
