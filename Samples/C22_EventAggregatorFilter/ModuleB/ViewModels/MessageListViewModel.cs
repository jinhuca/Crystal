using Crystal;
using EAMessageCore;
using System.Collections.ObjectModel;

namespace ModuleB.ViewModels
{
  internal class MessageListViewModel : BindableBase
  {
    private readonly IEventAggregator _eventAggregator;

    private ObservableCollection<string> _messages = new ObservableCollection<string>();
    public ObservableCollection<string> Messages
    {
      get => _messages;
      set => SetProperty(ref _messages, value);
    }

    public MessageListViewModel(IEventAggregator eventAggregator)
    {
      _eventAggregator = eventAggregator;
      _eventAggregator.GetEvent<MessageSentEvent>().Subscribe(MessageReceived, ThreadOption.PublisherThread, false, filter => filter.Contains("Crystal"));
    }

    private void MessageReceived(string message)
    {
      Messages.Add(message);
    }
  }
}
