using Crystal;
using C3201SolutionCore;

namespace C3201ModulePublisher.ViewModels
{
  public class PublisherViewModel : BindableBase
  {
    private readonly IEventAggregator _eventAggregator;
    private string _message = "message to send";
    public string Message
    {
      get => _message;
      set => SetProperty(ref _message, value);
    }

    public DelegateCommand PublishMessageCommand { get; }

    public PublisherViewModel(IEventAggregator ea)
    {
      _eventAggregator = ea;
      PublishMessageCommand = new DelegateCommand(SendMessage);
    }

    private void SendMessage()
    {
      _eventAggregator.GetEvent<PubSubMessageEvent>().Publish(Message);
    }
  }
}
