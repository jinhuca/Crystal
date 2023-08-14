namespace Crystal.UnitTests.Mocks.ViewModels
{
  public class MockViewModel : BindableBase
  {
    private int mockProperty;

    public int MockProperty
    {
      get => mockProperty;

      set => SetProperty(ref mockProperty, value);
    }

    internal void InvokeOnPropertyChanged()
    {
      RaisePropertyChanged(nameof(MockProperty));
    }
  }
}
