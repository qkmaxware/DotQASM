using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for resetting a qubit
/// </summary>
public class ResetEvent: IEvent {
    public IEnumerable<Cbit> ClassicalDependencies => null;
    public IEnumerable<Qubit> QuantumDependencies {get; protected set;}
    public string Name => "reset";
    public ResetEvent(IEnumerable<Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
    }
}

}