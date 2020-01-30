using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class BarrierEvent: IEvent {
    public IEnumerable<Cbit> ClassicalDependencies => null;
    public IEnumerable<Qubit> QuantumDependencies {get; protected set;}

    public BarrierEvent(IEnumerable<Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
    }
}

}