using Crystal;
using UsingEventAggregator.Core;

namespace ModuleA.ViewModels
{
  public class MessageViewModel : BindableBase
  {
    private readonly IEventAggregator _ea;

    private string _message = "Message to Send";
    public string Message
    {
      get => _message;
      set => SetProperty(ref _message, value);
    }

    public DelegateCommand SendMessageCommand { get; private set; }

    public MessageViewModel(IEventAggregator ea)
    {
      _ea = ea;
      SendMessageCommand = new DelegateCommand(SendMessage);
    }

    private void SendMessage()
    {
      _ea.GetEvent<MessageSentEvent>().Publish(Message);
    }
  }
}
