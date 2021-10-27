using System.Collections;
using System.Collections.Generic;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks
{
	internal class MockRegionBehaviorCollection : Dictionary<string, IRegionBehavior>, IRegionBehaviorCollection
	{
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
