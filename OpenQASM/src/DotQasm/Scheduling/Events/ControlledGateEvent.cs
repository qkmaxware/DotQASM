using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class ControlledGateEvent: IEvent {

    public IEnumerable<Circuit.Qubit> QuantumDependencies {get; private set;}
    public Circuit.Qubit ControlQubit => QuantumDependencies.FirstOrDefault();
    public IEnumerable<Circuit.Qubit> TargetQubits => QuantumDependencies.Skip(1);
    public virtual IEnumerable<Circuit.Cbit> ClassicalDependencies { 
        get => null;
        protected set {}
    }
    public Gate Operator {get; private set;}

    public ControlledGateEvent (Gate gate, Circuit.Qubit control, IEnumerable<Circuit.Qubit> targets) {
        this.QuantumDependencies = targets.Prepend(control);
        this.Operator = gate;
    }

    public ControlledGateEvent (Gate gate, IEnumerable<Circuit.Qubit> dependencies) {
        this.QuantumDependencies = dependencies;
        this.Operator = gate;
    }

}

}