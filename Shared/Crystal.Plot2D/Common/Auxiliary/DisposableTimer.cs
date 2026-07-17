using System;
using System.Diagnostics;

namespace Crystal.Plot2D.Common.Auxiliary;

public sealed class DisposableTimer : IDisposable {
  private readonly bool isActive = true;
  private readonly string name;
  private readonly Stopwatch timer;
  public DisposableTimer(string name) : this(name: name, isActive: true) { }

  public DisposableTimer(string name, bool isActive) {
    this.name = name;
    this.isActive = isActive;
    if(isActive) {
      timer = Stopwatch.StartNew();
      Trace.WriteLine(message: name + ": started " + DateTime.Now.TimeOfDay);
    }
  }

  #region IDisposable Members

  public void Dispose() {
    //#if DEBUG
    if(isActive) {
      var duration = timer.ElapsedMilliseconds;
      Trace.WriteLine(message: name + ": elapsed " + duration + " ms.");
      timer.Stop();
    }
    //#endif
  }

  #endregion
}
