using Crystal;

namespace UnitTests.Support.Support.Mocks
{
	public class MockModuleInitializer : IModuleInitializer
	{
		public bool LoadCalled;

		public void Initialize(IModuleInfo moduleInfo)
		{
			LoadCalled = true;
		}
	}
}
