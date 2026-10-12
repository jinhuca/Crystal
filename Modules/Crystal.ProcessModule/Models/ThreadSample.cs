using ThreadState = System.Diagnostics.ThreadState;
using System.Diagnostics;

namespace Crystal.ProcessModule.Models;

/// <summary>
/// One thread owned by a process, sampled for the expandable thread list. <see cref="TotalProcessorTime"/>
/// is the thread's cumulative user+kernel CPU time; <see cref="WaitReason"/> is meaningful only when
/// <see cref="State"/> is <see cref="ThreadState.Wait"/> (null otherwise). Priority and start time can be
/// unreadable for protected/exiting threads, in which case they arrive as 0 / null.
/// </summary>
/// <param name="ThreadId">The OS thread identifier (TID).</param>
/// <param name="TotalProcessorTime">Cumulative user+kernel CPU time consumed by the thread.</param>
/// <param name="State">The thread's scheduling state.</param>
/// <param name="WaitReason">Why the thread is waiting, or null when it isn't in the wait state.</param>
/// <param name="BasePriority">The thread's base priority.</param>
/// <param name="CurrentPriority">The thread's current (dynamic) priority.</param>
/// <param name="StartTime">When the thread started, or null when the OS wouldn't report it.</param>
public sealed record ThreadSample(
    int ThreadId,
    TimeSpan TotalProcessorTime,
    ThreadState State,
    ThreadWaitReason? WaitReason,
    int BasePriority,
    int CurrentPriority,
    DateTime? StartTime);
