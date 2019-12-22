using System;
using DotQasm.Scheduling;

namespace DotQasm.Scheduling {

public interface ITimeEstimator {
    TimeSpan? ShortestTimeBetween (IEventGraphIterator start, IEventGraphIterator end);
    TimeSpan? ShortestTimeOf (ISchedule schedule) {
        return ShortestTimeBetween(schedule.First, schedule.Last);
    }
}

}