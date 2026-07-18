// NOTE: CrystalMmiModule.cs declares `namespace Crystal.Wmi;` even though it lives in the
// Crystal.Mmi project alongside everything else, which is under `Crystal.Mmi.*`. That looks
// like a leftover from a Wmi -> Mmi rename rather than something intentional -- flagging it
// here since it's why this test file imports Crystal.Wmi instead of Crystal.Mmi.
using Crystal.Wmi;
using Xunit;

namespace Crystal.Mmi.Tests;

public class CrystalMmiModuleTests {
  // Both methods are unimplemented stubs today. These tests pin that current behavior;
  // once the module is actually implemented, both should be rewritten to assert real
  // registration/initialization behavior instead of an exception.
  [Fact]
  public void OnInitialized_ThrowsNotImplementedException() {
    var module = new CrystalMmiModule();

    var exception = Record.Exception(() => module.OnInitialized(null!));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact]
  public void RegisterTypes_ThrowsNotImplementedException() {
    var module = new CrystalMmiModule();

    var exception = Record.Exception(() => module.RegisterTypes(null!));

    Assert.IsType<NotImplementedException>(exception);
  }
}
