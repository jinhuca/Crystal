using Crystal;
using System;

namespace ModuleA.Models
{
  public class Person : BindableBase
  {
    private string _firstName;
    public string FirstName
    {
      get => _firstName;
      set => SetProperty(ref _firstName, value);
    }

    private string _lastName;
    public string LastName
    {
      get => _lastName;
      set => SetProperty(ref _lastName, value);
    }

    private int _age;
    public int Age
    {
      get => _age;
      set => SetProperty(ref _age, value);
    }

    private DateTime? _lastUpdated;
    public DateTime? LastUpdated
    {
      get => _lastUpdated;
      set => SetProperty(ref _lastUpdated, value);
    }

    public override string ToString() => $"{LastName}, {FirstName}";
  }
}
