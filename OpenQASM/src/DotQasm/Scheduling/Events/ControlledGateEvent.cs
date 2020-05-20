using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for a applying a controlled quantum gate
/// </summary>
public class ControlledGateEvent: IEvent {

    public IEnumerable<Qubit> QuantumDependencies {get; private set;}
    public Qubit ControlQubit => QuantumDependencies.FirstOrDefault();
    public IEnumerable<Qubit> TargetQubits => QuantumDependencies.Skip(1);
    public string Name => "c" + Operator.Symbol;

    public virtual IEnumerable<Cbit> ClassicalDependencies { 
        get => null;
        protected set {}
    }
    public Gate Operator {get; private set;}

    public ControlledGateEvent (Gate gate, Qubit control, IEnumerable<Qubit> targets) {
        this.QuantumDependencies = targets.Prepend(control);
        this.Operator = gate;
    }

    public ControlledGateEvent (Gate gate, Qubit control, Qubit target) : this(gate, control, new Qubit[] { target }) {}

    public ControlledGateEvent (Gate gate, IEnumerable<Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
        this.Operator = gate;
    }

    public override string ToString() {
       return GetType().ToString(); 
    }
}

}