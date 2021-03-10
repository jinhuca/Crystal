using Crystal;

namespace UsingIActiveAwareCommands.Core
{
	public interface IApplicationCommands
	{
		CompositeCommand SaveCommand { get; }
	}

	public class ApplicationCommands : IApplicationCommands
	{
		private CompositeCommand _saveCommand = new CompositeCommand(true);
		public CompositeCommand SaveCommand
		{
			get { return _saveCommand; }
		}
	}
}
