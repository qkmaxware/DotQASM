using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class MeasurementEvent: IEvent {
    public IEnumerable<Circuit.Cbit> ClassicalDependencies {get; protected set;}
    public IEnumerable<Circuit.Qubit> QuantumDependencies {get; protected set;}

    public MeasurementEvent(IEnumerable<Circuit.Cbit> cds, IEnumerable<Circuit.Qubit> qds) {
        this.ClassicalDependencies = cds;
        this.QuantumDependencies = qds;
    }
}

}