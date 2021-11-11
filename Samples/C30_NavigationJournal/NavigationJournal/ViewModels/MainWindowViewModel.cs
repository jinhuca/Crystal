using Crystal;

namespace NavigationJournal.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private string _title = "Crystal Application";
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    public MainWindowViewModel()
    {

    }
  }
}
