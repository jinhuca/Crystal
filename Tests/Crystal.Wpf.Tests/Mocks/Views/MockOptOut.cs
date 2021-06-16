using System.Windows;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks.Views
{
	public class MockOptOut : FrameworkElement
	{
		public MockOptOut()
		{
			ViewModelLocator.SetAutoWireViewModel(this, false);
		}
	}
}
