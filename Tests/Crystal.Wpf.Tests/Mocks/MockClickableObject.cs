using System.Windows.Controls.Primitives;

namespace Crystal.Wpf.Tests.Mocks
{
	internal class MockClickableObject : ButtonBase
	{
		public void RaiseClick()
		{
			OnClick();
		}
	}
}
