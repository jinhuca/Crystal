using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Container.Shared.Fixtures.Mvvm
{
  [Collection(nameof(ContainerExtension))]
  public class ViewModelLocatorFixture
  {
    [StaFact]
    public void ShouldLocateViewModelAndResolveWithContainer()
    {
      var bootstrapper = new MockBootstrapper();
      bootstrapper.Run();

      bootstrapper.ContainerRegistry.Register<IService, MockService>();

      var view = new MockView();
      Assert.Null(view.DataContext);

      ViewModelLocator.SetAutoWireViewModel(view, true);
      Assert.NotNull(view.DataContext);
      Assert.IsType<MockViewModel>(view.DataContext);

      Assert.NotNull(((MockViewModel)view.DataContext).MockService);
    }
  }
}
