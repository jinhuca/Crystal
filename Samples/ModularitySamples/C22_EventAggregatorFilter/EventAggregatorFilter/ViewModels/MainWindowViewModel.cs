using Crystal;

namespace EventAggregatorFilter.ViewModels
{
  internal class MainWindowViewModel : BindableBase
  {
    private string _title = "Crystal Event Aggregator";
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }
  }
}
