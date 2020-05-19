using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for applying a single qubit gate
/// </summary>
public class SwapEvent: IEvent {

    public IEnumerable<Qubit> QuantumDependencies {get; private set;}
    public virtual IEnumerable<Cbit> ClassicalDependencies { 
        get => null;
        protected set {}
    }

    public string Name => "swap";

    public SwapEvent (Qubit a, Qubit b) {
        this.QuantumDependencies = new Qubit[]{a, b};
    }

}

}