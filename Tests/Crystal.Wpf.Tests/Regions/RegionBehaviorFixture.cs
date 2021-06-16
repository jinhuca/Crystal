using System;
using Crystal;
using Crystal.Wpf.Tests.Mocks;
using Xunit;

namespace Crystal.Wpf.Tests.Regions
{

	public class RegionBehaviorFixture
	{
		[Fact]
		public void CannotChangeRegionAfterAttach()
		{
			var ex = Assert.Throws<InvalidOperationException>(() =>
			{
				TestableRegionBehavior regionBehavior = new TestableRegionBehavior();

				regionBehavior.Region = new MockPresentationRegion();

				regionBehavior.Attach();
				regionBehavior.Region = new MockPresentationRegion();
			});

		}

		[Fact]
		public void ShouldFailWhenAttachedWithoutRegion()
		{
			var ex = Assert.Throws<InvalidOperationException>(() =>
			{
				TestableRegionBehavior regionBehavior = new TestableRegionBehavior();
				regionBehavior.Attach();
			});

		}

		[Fact]
		public void ShouldCallOnAttachWhenAttachMethodIsInvoked()
		{
			TestableRegionBehavior regionBehavior = new TestableRegionBehavior();

			regionBehavior.Region = new MockPresentationRegion();

			regionBehavior.Attach();

			Assert.True(regionBehavior.onAttachCalled);
		}

		private class TestableRegionBehavior : RegionBehavior
		{
			public bool onAttachCalled;

			protected override void OnAttach()
			{
				onAttachCalled = true;
			}
		}
	}


}
