using Moq;
using Xunit;
using static Crystal.ContainerLocator;

namespace Crystal.UnitTests.Ioc
{
	public class ContainerLocator { }

	[CollectionDefinition(nameof(ContainerLocator), DisableParallelization = true)]
	public class ContainerLocatorCollection : ICollectionFixture<ContainerLocator>
	{
	}

	[Collection(nameof(ContainerLocator))]
	public class ContainerLocatorFixture
	{
		[Fact]
		public void FactoryCreatesContainerExtension()
		{
			ResetContainer();
			Assert.Null(Current);
			SetContainerExtension(() => new Mock<IContainerExtension>().Object);
			Assert.NotNull(Current);
		}

		[Fact]
		public void ResetNullsCurrentContainer()
		{
			ResetContainer();
			Assert.Null(Current);
			SetContainerExtension(() => new Mock<IContainerExtension>().Object);
			Assert.NotNull(Current);
			ResetContainer();
			Assert.Null(Current);
		}

		[Fact]
		public void FactoryOnlySetsContainerOnce()
		{
			ResetContainer();
			var container = new Mock<IContainerExtension>().Object;
			var container2 = new Mock<IContainerExtension>().Object;
			/*
			SetContainerExtension(() => container);
			Assert.Same(container, Container);

			SetContainerExtension(() => container2);
			Assert.NotSame(container2, Container);
			Assert.Same(container, Container);
			*/
		}
	}
}
