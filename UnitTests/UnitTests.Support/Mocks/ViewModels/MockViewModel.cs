using Crystal;

namespace UnitTests.Support.Mocks.ViewModels
{
	public class MockViewModel : BindableBase
	{
		private IService _mockService;
		public IService MockService
		{
			get => _mockService;
			set => SetProperty(ref _mockService, value);
		}

		public MockViewModel(IService mockService)
		{
			_mockService = mockService;
		}
	}
}
