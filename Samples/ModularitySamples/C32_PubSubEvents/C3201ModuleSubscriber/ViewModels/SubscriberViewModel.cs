using C3201SolutionCore;
using Crystal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3201ModuleSubscriber.ViewModels
{
  public class SubscriberViewModel : BindableBase
  {
    private readonly IEventAggregator _eventAggregator;
    private ObservableCollection<string> _messages = new ObservableCollection<string>();
    public ObservableCollection<string> Messages
    {
      get => _messages;
      set => SetProperty(ref _messages, value);
    }

    public SubscriberViewModel(IEventAggregator ea)
    {
      _eventAggregator = ea;
      _eventAggregator.GetEvent<PubSubMessageEvent>().Subscribe(MessageReceived);
    }

    private void MessageReceived(string message)
    {
      Messages.Add(message);
    }
  }
}
