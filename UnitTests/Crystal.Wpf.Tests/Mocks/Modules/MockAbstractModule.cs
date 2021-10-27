using System;
using Crystal;

namespace Crystal.Wpf.Tests.Mocks.Modules
{
	public abstract class MockAbstractModule : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{

		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}
	}

	public class MockInheritingModule : MockAbstractModule
	{
	}
}
