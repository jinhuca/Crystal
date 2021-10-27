using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Container.Shared.Fixtures.Bootstrapper
{
  public class BootstrapperNullModuleCatalogFixture : BootstrapperFixtureBase
  {
    [Fact]
    public void NullModuleCatalogThrowsOnDefaultModuleInitialization()
    {
      var bootstrapper = new NullModuleCatalogBootstrapper();

      AssertExceptionThrownOnRun(bootstrapper, typeof(InvalidOperationException), "IModuleCatalog");
    }
  }
}
