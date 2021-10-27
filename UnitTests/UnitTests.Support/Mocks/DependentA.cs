namespace UnitTests.Support.Mocks
{
	public class DependentA : IDependentA
	{
		public DependentA(IDependentB dependentB)
		{
			MyDependentB = dependentB;
		}

		public IDependentB MyDependentB { get; set; }
	}

	public interface IDependentA
	{
		IDependentB MyDependentB { get; }
	}
}
