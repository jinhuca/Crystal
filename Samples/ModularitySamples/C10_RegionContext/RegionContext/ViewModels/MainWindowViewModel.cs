using Crystal;

namespace RegionContext.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private string _title = "Crystal Region Context";
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
