using System;
using Crystal;

namespace Crystal.Tests.Events
{
	class MockDelegateReference : IDelegateReference
	{
		public Delegate Target { get; set; }

		public MockDelegateReference()
		{

		}

		public MockDelegateReference(Delegate target)
		{
			Target = target;
		}
	}
}
