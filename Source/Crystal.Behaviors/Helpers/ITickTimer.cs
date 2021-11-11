using System;
using System.Diagnostics.CodeAnalysis;

namespace Crystal.Behaviors
{
  [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "This isn't an exception.")]
  interface ITickTimer
  {
    event EventHandler Tick;
    void Start();
    void Stop();
    TimeSpan Interval { get; set; }
  }
}
