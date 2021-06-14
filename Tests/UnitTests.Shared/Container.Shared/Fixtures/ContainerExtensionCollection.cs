using Xunit;

namespace Container.Shared.Fixtures
{
  public class ContainerExtension { }

  [CollectionDefinition(nameof(ContainerExtension), DisableParallelization = true)]
  public class ContainerExtensionCollection : ICollectionFixture<ContainerExtension>
  {
  }
}
