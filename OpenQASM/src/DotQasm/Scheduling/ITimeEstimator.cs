using System;

namespace DotQasm.Scheduling {

public interface ITimeEstimator {
    TimeSpan TimeOf (IEvent evt);
    TimeSpan? ShortestTimeBetween (IEventGraphIterator start, IEventGraphIterator end);
    TimeSpan? LongestTimeBetween (IEventGraphIterator start, IEventGraphIterator end);
}

}