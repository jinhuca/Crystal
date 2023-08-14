using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crystal.UnitTests.Commands
{
  /// <summary>
  /// Provides minimum functionality BindableBase based class in order to expose
  /// GetPropertyChangeSubscribedLength to test if PropertyObserver's
  /// unsubscribing to PropertyChanged is working properly.
  /// </summary>
  public abstract class TestPurposeBindableBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public int GetPropertyChangedSubscribledLength()
    {
      return PropertyChanged?.GetInvocationList()?.Length ?? 0;
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
      if (Equals(storage, value)) return false;

      storage = value;
      RaisePropertyChanged(propertyName);

      return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
      OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
      PropertyChanged?.Invoke(this, args);
    }
  }
}
