using System;
using DotQasm.Scheduling;

namespace DotQasm.Scheduling {

/// <summary>
/// Interface for classes that can estimate the time between two event graph iterators
/// </summary>
public interface ITimeEstimator {
    TimeSpan? TimeBetween (IEvent start, IEvent end);
}

/// <summary>
/// Interface for classes that estimate the time of a particular quantum operation
/// </summary>
public interface ILatencyEstimator {
    TimeSpan TimeOf(IEvent evt);
}

}