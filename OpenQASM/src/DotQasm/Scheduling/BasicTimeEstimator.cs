using System;
using System.Linq;

namespace DotQasm.Scheduling {

public class BasicTimeEstimator : ITimeEstimator {

    // Default times based on https://github.com/Qiskit/ibmq-device-information/tree/master/backends/yorktown/V1
    public TimeSpan SingleGateTime = TimeSpan.FromMilliseconds(2.8);
    public TimeSpan MultipleGateTime = TimeSpan.FromMilliseconds(2.9);
    public TimeSpan ClassicalCheckLatency = TimeSpan.FromMilliseconds(1);
    public TimeSpan MeasurementTime = TimeSpan.FromMilliseconds(1);
    public TimeSpan ResetTime = TimeSpan.FromMilliseconds(1);
    public TimeSpan BarrierTime = new TimeSpan();
    public TimeSpan OtherEventTime = TimeSpan.FromMilliseconds(1);

    public TimeSpan TimeOf (IEvent evt) {
        return evt switch {
            BarrierEvent be => BarrierTime,
            MeasurementEvent me => MeasurementTime,
            ResetEvent re => ResetTime,
            // If is a classical check + a quantum gate
            IfEvent ie => ClassicalCheckLatency + TimeOf(ie.Event),
            // Quantum gates are split into single qubit and multi-qubit timespans
            GateEvent ge => ge.QuantumDependencies.Count() > 1 ? MultipleGateTime : SingleGateTime,
            // Other events just use a default time
            _ => OtherEventTime
        };
    }

    public TimeSpan? ShortestTimeBetween (IEventGraphIterator start, IEventGraphIterator end) {
        return DotQasm.Search.AStarSearch.Path(start, end, (evt) => {
            return 0; 
        })
        ?.Select(x => TimeOf(x.Current))
        ?.Aggregate((a,b) => a.Add(b));
    }
}

}