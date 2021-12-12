using Crystal;
using System;

namespace C33ModulePublisher.ViewModels
{
  internal class PublisherViewModel : BindableBase
  {
    private readonly IEventAggregator _eventAggregator;
    
    private int _age = 7;
    public int Age
    {
      get => _age;
      set => SetProperty(ref _age, value);
    }

    private string _name = String.Empty;
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    private double _gdp = 0.0;
    public double Gdp
    {
      get => _gdp;
      set => SetProperty(ref _gdp, value);
    }

    public DelegateCommand PublishCommand { get; set; }

    public PublisherViewModel()
    {

    }
  }
}
