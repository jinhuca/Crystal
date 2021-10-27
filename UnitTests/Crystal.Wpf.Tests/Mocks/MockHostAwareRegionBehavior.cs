using System.Windows;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks
{
	public class MockHostAwareRegionBehavior : IHostAwareRegionBehavior
	{
		public IRegion Region { get; set; }

		public void Attach()
		{

		}

		public DependencyObject HostControl { get; set; }
	}
}
