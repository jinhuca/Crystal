namespace UnitTests.Support.Mocks
{
	public class DependentB : IDependentB
	{
		public DependentB(IService service)
		{
			MyService = service;
		}

		public IService MyService { get; set; }
	}

	public interface IDependentB
	{
		IService MyService { get; }
	}
}
