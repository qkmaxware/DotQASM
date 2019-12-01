using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class GateEvent: IEvent {

    public IEnumerable<Circuit.Qubit> QuantumDependencies {get; private set;}
    public virtual IEnumerable<Circuit.Cbit> ClassicalDependencies { 
        get => null;
        protected set {}
    }
    public Gate Operator {get; private set;}

    public GateEvent (Gate gate, IEnumerable<Circuit.Qubit> members) {
        this.QuantumDependencies = members;
        this.Operator = gate;
    }

}

}