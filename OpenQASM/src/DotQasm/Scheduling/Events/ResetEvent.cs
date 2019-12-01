using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class ResetEvent: IEvent {
    public IEnumerable<Circuit.Cbit> ClassicalDependencies => null;
    public IEnumerable<Circuit.Qubit> QuantumDependencies {get; protected set;}

    public ResetEvent(IEnumerable<Circuit.Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
    }
}

}