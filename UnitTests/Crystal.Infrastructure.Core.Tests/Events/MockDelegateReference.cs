using Crystal.Infrastructure.Core.Events;

namespace Crystal.Infrastructure.Core.Tests.Events;

class MockDelegateReference : IDelegateReference {
  public Delegate Target { get; set; }

  public MockDelegateReference() {

  }

  public MockDelegateReference(Delegate target) {
    Target = target;
  }
}
