using Crystal;

namespace C3201App.ViewModels
{
  public class ShellViewModel : BindableBase
  {
    private string _title = "Crystal Application for PubSubEvent";
    public string Title
    {
      get=> _title;
      set => SetProperty(ref _title, value);
    }

    public ShellViewModel()
    {

    }
  }
}
