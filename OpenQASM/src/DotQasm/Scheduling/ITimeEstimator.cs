using System;
using DotQasm.Scheduling;

namespace DotQasm.Scheduling {

/// <summary>
/// Interface for classes that can estimate the time between two event graph iterators
/// </summary>
public interface ITimeEstimator {
    TimeSpan? ShortestTimeBetween (IEventGraphIterator start, IEventGraphIterator end);
    TimeSpan? ShortestTimeOf (ISchedule schedule) {
        return ShortestTimeBetween(schedule.First, schedule.Last);
    }
}

}