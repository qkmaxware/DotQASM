using System;

namespace DotQasm.Scheduling {
    
/// <summary>
/// Base class for any listener who supports only OpenQasm operations
/// </summary>
public abstract class BaseOpenQasmEventVisitor {
    public void Visit(IEvent evt) {
        switch (evt) {
            case BarrierEvent barrier:
                VisitBarrier(barrier); break;
            case ControlledGateEvent controlledGate:
                VisitControlledGate(controlledGate); break;
            case GateEvent gate:
                VisitGate(gate); break;
            case IfEvent ifEvent:
                VisitIf(ifEvent); break;
            case MeasurementEvent measurement:
                VisitMeasurement(measurement); break;
            case ResetEvent reset:
                VisitReset(reset); break;
            default:
                VisitUnsupportedOperation(evt); break; 
        }
    }

    public virtual void VisitUnsupportedOperation(IEvent statement) {
        throw new InvalidOperationException(statement.GetType() + " is not supported by " + this.GetType());
    }

    public abstract void VisitBarrier (BarrierEvent barrier);

    public abstract void VisitControlledGate (ControlledGateEvent controlledGate);

    public abstract void VisitGate (GateEvent gate);

    public abstract void VisitIf (IfEvent @if);

    public abstract void VisitMeasurement (MeasurementEvent measurement);

    public abstract void VisitReset (ResetEvent reset);
}

}