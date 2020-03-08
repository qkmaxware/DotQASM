using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for a quantum barrier
/// </summary>
public class BarrierEvent: IEvent {
    public IEnumerable<Cbit> ClassicalDependencies => null;
    public IEnumerable<Qubit> QuantumDependencies {get; protected set;}
    public string Name => "barrier";
    public BarrierEvent(IEnumerable<Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
    }
}

}