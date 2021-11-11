using CompositeCommandShared;
using Crystal;

namespace ActiveAwareCommands.ViewModels
{
  internal class MainWindowViewModel : BindableBase
  {
    private string _title = "Crystal Composite Command with IActiveAware";
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    private IApplicationCommands _applicationCommands;
    public IApplicationCommands ApplicationCommands
    {
      get => _applicationCommands;
      set => SetProperty(ref _applicationCommands, value);
    }

    public MainWindowViewModel(IApplicationCommands applicationCommands)
    {
      ApplicationCommands = applicationCommands;
    }
  }
}
