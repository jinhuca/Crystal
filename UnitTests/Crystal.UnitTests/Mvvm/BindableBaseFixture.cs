using Crystal.UnitTests.Mocks.ViewModels;
using Xunit;

namespace Crystal.UnitTests.Mvvm
{
  public class BindableBaseFixture
  {
    [Fact]
    public void SetPropertyMethodShouldSetTheNewValue()
    {
      int value = 10;
      MockViewModel mockViewModel = new();

      Assert.Equal(0, mockViewModel.MockProperty);

      mockViewModel.MockProperty = value;
      Assert.Equal(value, mockViewModel.MockProperty);
    }

    [Fact]
    public void SetPropertyMethodShouldNotSetTheNewValue()
    {
      int value = 10, newValue = 10;
      MockViewModel mockViewModel = new() { MockProperty = value };

      bool invoked = false;
      mockViewModel.PropertyChanged += (o, e) => { if (e.PropertyName.Equals("MockProperty")) invoked = true; };
      mockViewModel.MockProperty = newValue;

      Assert.False(invoked);
      Assert.Equal(value, mockViewModel.MockProperty);
    }

    [Fact]
    public void SetPropertyMethodShouldRaisePropertyRaised()
    {
      bool invoked = false;
      MockViewModel mockViewModel = new();

      mockViewModel.PropertyChanged += (o, e) => { if (e.PropertyName.Equals("MockProperty")) invoked = true; };
      mockViewModel.MockProperty = 10;

      Assert.True(invoked);
    }

    [Fact]
    public void OnPropertyChangedShouldExtractPropertyNameCorrectly()
    {
      bool invoked = false;
      MockViewModel mockViewModel = new();

      mockViewModel.PropertyChanged += (o, e) => { if (e.PropertyName.Equals("MockProperty")) invoked = true; };
      mockViewModel.InvokeOnPropertyChanged();

      Assert.True(invoked);
    }
  }
}
