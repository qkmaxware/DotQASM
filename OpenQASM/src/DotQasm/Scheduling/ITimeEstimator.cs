using System;
using DotQasm.Scheduling;

namespace DotQasm.Scheduling {

/// <summary>
/// Interface for classes that can estimate the time between two event graph iterators
/// </summary>
public interface ITimeEstimator {
    /// <summary>
    /// Compute the time between two events
    /// </summary>
    /// <param name="start">start event</param>
    /// <param name="end">end event</param>
    /// <returns>sum of the longest path from the start to end event</returns>
    TimeSpan? TimeBetween (IEvent start, IEvent end);
}

/// <summary>
/// Interface for classes that estimate the time of a particular quantum operation
/// </summary>
public interface ILatencyEstimator {
    /// <summary>
    /// Compute the time of a given event
    /// </summary>
    /// <param name="evt">event to check</param>
    TimeSpan TimeOf(IEvent evt);
}

}