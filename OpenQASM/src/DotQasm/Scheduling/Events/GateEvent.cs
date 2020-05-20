using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for applying a single qubit gate
/// </summary>
public class GateEvent: IEvent {

    public IEnumerable<Qubit> QuantumDependencies {get; private set;}
    public virtual IEnumerable<Cbit> ClassicalDependencies { 
        get => null;
        protected set {}
    }
    public Gate Operator {get; private set;}

    public string Name => Operator.Symbol;

    public GateEvent (Gate gate, IEnumerable<Qubit> members) {
        this.QuantumDependencies = members;
        this.Operator = gate;
    }

    public GateEvent (Gate gate, Qubit member) : this(gate, new Qubit[]{ member }) {}

    public override string ToString() {
       return GetType().ToString(); 
    }
}

}