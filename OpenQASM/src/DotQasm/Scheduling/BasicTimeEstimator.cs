using System;
using System.Linq;

namespace DotQasm.Scheduling {

public class BasicTimeEstimator : ITimeEstimator {

    // Default times based on IBM QX2
    public TimeSpan SingleGateTime = TimeSpan.FromMilliseconds(0.052);
    public TimeSpan MultipleGateTime = new TimeSpan();
    public TimeSpan ClassicalCheckLatency = new TimeSpan();
    public TimeSpan MeasurementTime = new TimeSpan();
    public TimeSpan ResetTime = new TimeSpan();
    public TimeSpan BarrierTime = new TimeSpan();
    public TimeSpan OtherEventTime = new TimeSpan();

    public TimeSpan TimeOf (IEvent evt) {
        return evt switch {
            BarrierEvent be => BarrierTime,
            MeasurementEvent me => MeasurementTime,
            ResetEvent re => ResetTime,
            // If is a classical check + a quantum gate
            IfEvent ie => ClassicalCheckLatency + (evt.QuantumDependencies.Count() > 1 ? MultipleGateTime : SingleGateTime),
            // Quantum gates are split into single qubit and multi-qubit timespans
            GateEvent ge => evt.QuantumDependencies.Count() > 1 ? MultipleGateTime : SingleGateTime,
            _ => OtherEventTime
        };
    }

    public TimeSpan ShortestTimeBetween (IEventGraphIterator start, IEventGraphIterator end) {
        return DotQasm.Search.AStarSearch.Path(start, end, Search.AStarSearch.SortOrder.Shortest, (evt) => {
            return 0; // No Heuristic
        })
        .Select(x => TimeOf(x.Current))
        .Aggregate((a,b) => a.Add(b));
    }

    public TimeSpan LongestTimeBetween (IEventGraphIterator start, IEventGraphIterator end) {
        return DotQasm.Search.AStarSearch.Path(start, end, Search.AStarSearch.SortOrder.Longest, (evt) => {
            return 0; // No Heuristic
        })
        .Select(x => TimeOf(x.Current))
        .Aggregate((a,b) => a.Add(b));
    }
}

}