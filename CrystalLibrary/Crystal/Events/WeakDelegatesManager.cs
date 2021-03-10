using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager'
	public class WeakDelegatesManager
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager'
	{
		private readonly List<DelegateReference> _listeners = new List<DelegateReference>();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager.AddListener(Delegate)'
		public void AddListener(Delegate listener)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager.AddListener(Delegate)'
		{
			_listeners.Add(new DelegateReference(listener, false));
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager.RemoveListener(Delegate)'
		public void RemoveListener(Delegate listener)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager.RemoveListener(Delegate)'
		{
			//Remove the listener, and prune collected listeners
			_listeners.RemoveAll(reference => reference.TargetEquals(null) || reference.TargetEquals(listener));
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager.Raise(params object[])'
		public void Raise(params object[] args)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WeakDelegatesManager.Raise(params object[])'
		{
			_listeners.RemoveAll(listener => listener.TargetEquals(null));

			foreach (Delegate handler in _listeners.Select(listener => listener.Target).Where(listener => listener != null).ToList())
			{
				handler.DynamicInvoke(args);
			}
		}
	}
}
