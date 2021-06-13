using System.ComponentModel;
using Crystal;

namespace Crystal.UnitTests.Mocks.ViewModels
{
	public class MockViewModel : BindableBase
	{
		private int mockProperty;

		public int MockProperty
		{
			get
			{
				return mockProperty;
			}

			set
			{
				SetProperty(ref mockProperty, value);
			}
		}

		internal void InvokeOnPropertyChanged()
		{
			RaisePropertyChanged(nameof(MockProperty));
		}
	}
}
