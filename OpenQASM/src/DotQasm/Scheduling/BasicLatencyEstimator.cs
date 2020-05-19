using System;
using System.Linq;

namespace DotQasm.Scheduling {

/// <summary>
/// Constant latency estimator assumes all quantum operations take 1 millisecond
/// </summary>
public class ConstantLatencyEstimator : ILatencyEstimator {
    public TimeSpan TimeOf (IEvent evt) {
        return TimeSpan.FromMilliseconds(1);
    }
}

public class IntegerLatencyEstimator : ILatencyEstimator {
    public TimeSpan TimeOf (IEvent evt) {
        return evt switch {
            MeasurementEvent me         => TimeSpan.FromMilliseconds(3),
            ResetEvent re               => TimeSpan.FromMilliseconds(3),
            IfEvent ie                  => TimeSpan.FromMilliseconds(2),
            GateEvent ge                => TimeSpan.FromMilliseconds(1),
            ControlledGateEvent cge     => TimeSpan.FromMilliseconds(2),
            // Other events just use a default time
            BarrierEvent be             => TimeSpan.FromMilliseconds(1),
            _                           => TimeSpan.FromMilliseconds(1),
        };
    }
}

/// <summary>
/// Basic class for computing latency of a given quantum operation
/// </summary>
public class BasicLatencyEstimator : ILatencyEstimator {
    private static readonly double ns = 1e-6;

    // Default times based on https://github.com/Qiskit/ibmq-device-information/blob/master/backends/yorktown/V1/version_log.md
    public TimeSpan SingleGateTime = TimeSpan.FromMilliseconds(150 * ns);   // Given for gate timings
    public TimeSpan MultipleGateTime = TimeSpan.FromMilliseconds(211 * ns); // AVERAGE OF 190,190,250,250,150,240 for CX times
    public TimeSpan ClassicalCheckLatency = TimeSpan.FromMilliseconds(1);
    public TimeSpan MeasurementTime = TimeSpan.FromMilliseconds(1);
    public TimeSpan ResetTime = TimeSpan.FromMilliseconds(1 + 150 * ns);    // Set to MeasurementTime + SingleGateTime
    public TimeSpan BarrierTime = new TimeSpan();                           // Is a compiler pragma and takes no time
    public TimeSpan OtherEventTime = new TimeSpan();                        // Unidentified events get this time

    public TimeSpan TimeOf (IEvent evt) {
        return evt switch {
            BarrierEvent be => BarrierTime,
            MeasurementEvent me => MeasurementTime,
            ResetEvent re => ResetTime,
            // If is a classical check + a quantum gate
            IfEvent ie => ClassicalCheckLatency + TimeOf(ie.Event),
            GateEvent ge => SingleGateTime,
            ControlledGateEvent cge => MultipleGateTime,
            // Other events just use a default time
            _ => OtherEventTime
        };
    }
}

}