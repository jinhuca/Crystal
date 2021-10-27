using Xunit;

namespace Crystal.Tests
{
    [CollectionDefinition(nameof(ContainerExtension), DisableParallelization = true)]
    public class ContainerExtensionCollection : ICollectionFixture<ContainerExtension>
    {
    }
}
