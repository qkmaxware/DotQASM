using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class BarrierEvent: IEvent {
    public IEnumerable<Circuit.Cbit> ClassicalDependencies => null;
    public IEnumerable<Circuit.Qubit> QuantumDependencies {get; protected set;}

    public BarrierEvent(IEnumerable<Circuit.Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
    }
}

}