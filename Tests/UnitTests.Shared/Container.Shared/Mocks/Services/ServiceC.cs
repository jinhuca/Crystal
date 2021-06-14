namespace Crystal.Mocks.Services
{
	public class ServiceC : IServiceC
	{
		public ServiceC(IServiceB serviceB)
		{
			ServiceB = serviceB;
		}

		public IServiceB ServiceB { get; }
	}
}
