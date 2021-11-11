using Crystal;
using System.Collections.ObjectModel;
using UsingEventAggregator.Core;

namespace ModuleB.ViewModels
{
  public class MessageListViewModel : BindableBase
  {
    private readonly IEventAggregator _ea;

    private ObservableCollection<string> _messages = new ObservableCollection<string>();
    public ObservableCollection<string> Messages
    {
      get => _messages;
      set => SetProperty(ref _messages, value);
    }

    public MessageListViewModel(IEventAggregator ea)
    {
      _ea = ea;
      _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);
    }

    private void MessageReceived(string message)
    {
      Messages.Add(message);
    }
  }
}
