using Crystal;

namespace CompositeCommandShared
{
  public interface IApplicationCommands
  {
    CompositeCommand SaveCommand { get; }
  }
}
