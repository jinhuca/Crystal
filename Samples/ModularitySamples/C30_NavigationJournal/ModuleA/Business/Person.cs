using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModuleA.Business
{
  public class Person : INotifyPropertyChanged
  {
    #region Properties

    private string _firstName;
    public string FirstName
    {
      get => _firstName;
      set
      {
        _firstName = value;
        OnPropertyChanged();
      }
    }

    private string _lastName;
    public string LastName
    {
      get => _lastName;
      set
      {
        _lastName = value;
        OnPropertyChanged();
      }
    }

    private int _age;
    public int Age
    {
      get => _age;
      set
      {
        _age = value;
        OnPropertyChanged();
      }
    }

    private DateTime? _lastUpdated;
    public DateTime? LastUpdated
    {
      get => _lastUpdated;
      set
      {
        _lastUpdated = value;
        OnPropertyChanged();
      }
    }

    #endregion //Properties

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyname = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }

    #endregion //INotifyPropertyChanged

    public override string ToString() => $"{LastName}, {FirstName}";
  }
}
