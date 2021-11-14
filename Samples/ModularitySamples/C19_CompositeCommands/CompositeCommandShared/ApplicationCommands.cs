using Crystal;

namespace CompositeCommandShared
{

  public class ApplicationCommands : IApplicationCommands
  {
    public CompositeCommand SaveCommand { get; } = new CompositeCommand();
  }
}
