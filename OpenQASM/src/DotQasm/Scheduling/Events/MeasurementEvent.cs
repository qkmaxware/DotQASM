using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for qubit measurements
/// </summary>
public class MeasurementEvent: IEvent {
    public IEnumerable<Cbit> ClassicalDependencies {get; protected set;}
    public IEnumerable<Qubit> QuantumDependencies {get; protected set;}

    public string Name => "measurement";

    public MeasurementEvent(IEnumerable<Cbit> cds, IEnumerable<Qubit> qds) {
        this.ClassicalDependencies = cds;
        this.QuantumDependencies = qds;
    }

    public override string ToString() {
       return GetType().ToString(); 
    }
}

}