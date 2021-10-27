﻿using System.Windows;

namespace Crystal.Wpf.Tests.Mocks
{
	class MockFrameworkContentElement : FrameworkContentElement
	{
		public void RaiseLoaded()
		{
			this.RaiseEvent(new RoutedEventArgs(LoadedEvent));
		}

		public void RaiseUnloaded()
		{
			this.RaiseEvent(new RoutedEventArgs(UnloadedEvent));
		}
	}
}
