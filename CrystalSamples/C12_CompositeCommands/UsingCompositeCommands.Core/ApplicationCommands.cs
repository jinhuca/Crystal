using Crystal.Commands;

namespace UsingCompositeCommands.Core
{
	public interface IApplicationCommands
	{
		CompositeCommand SaveAllCommand { get; }
	}

	public class ApplicationCommands : IApplicationCommands
	{
		public CompositeCommand SaveAllCommand { get; } = new CompositeCommand();
	}
}
