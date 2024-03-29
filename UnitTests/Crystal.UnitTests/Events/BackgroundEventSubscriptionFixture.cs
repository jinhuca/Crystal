using System;
using System.Threading;
using Xunit;

namespace Crystal.UnitTests.Events
{
  public class BackgroundEventSubscriptionFixture
  {
    [Fact]
    public void ShouldReceiveDelegateOnDifferentThread()
    {
      ManualResetEvent completeEvent = new(false);
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
      SynchronizationContext calledSyncContext = null;

      void action(object obj)
      {
        calledSyncContext = SynchronizationContext.Current;
        completeEvent.Set();
      }

      IDelegateReference actionDelegateReference = new MockDelegateReference() { Target = (Action<object>)action };
      IDelegateReference filterDelegateReference = new MockDelegateReference() { Target = (Predicate<object>)delegate { return true; } };

      var eventSubscription = new BackgroundEventSubscription<object>(actionDelegateReference, filterDelegateReference);


      var publishAction = eventSubscription.GetExecutionStrategy();

      Assert.NotNull(publishAction);

      publishAction.Invoke(null);

      completeEvent.WaitOne(5000);

      Assert.NotEqual(SynchronizationContext.Current, calledSyncContext);
    }

    [Fact]
    public void ShouldReceiveDelegateOnDifferentThreadNonGeneric()
    {
      var completeEvent = new ManualResetEvent(false);
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
      SynchronizationContext calledSyncContext = null;

      void action()
      {
        calledSyncContext = SynchronizationContext.Current;
        completeEvent.Set();
      }

      IDelegateReference actionDelegateReference = new MockDelegateReference() { Target = (Action)action };

      var eventSubscription = new BackgroundEventSubscription(actionDelegateReference);

      var publishAction = eventSubscription.GetExecutionStrategy();

      Assert.NotNull(publishAction);

      publishAction.Invoke(null);

      completeEvent.WaitOne(5000);

      Assert.NotEqual(SynchronizationContext.Current, calledSyncContext);
    }
  }
}
