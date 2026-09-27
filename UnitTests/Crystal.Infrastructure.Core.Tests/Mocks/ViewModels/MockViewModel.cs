

using System.ComponentModel;
using Crystal.Infrastructure.Core.Mvvm;

namespace Crystal.Infrastructure.Core.Tests.Mocks.ViewModels;

public class MockViewModel : BindableBase {
  private int mockProperty;

  public int MockProperty {
    get {
      return this.mockProperty;
    }

    set {
      this.SetProperty(ref mockProperty, value);
    }
  }

  internal void InvokeOnPropertyChanged() {
    RaisePropertyChanged(nameof(MockProperty));
  }
}
