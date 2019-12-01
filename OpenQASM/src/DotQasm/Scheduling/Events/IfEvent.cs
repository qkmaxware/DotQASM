using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class IfEvent: IEvent {

    public Circuit.Cbit ClassicalBit { get; private set;}
    public int LiteralValue {get; private set;}
    public IEnumerable<Circuit.Cbit> ClassicalDependencies {get; protected set;}
    public IEnumerable<Circuit.Qubit> QuantumDependencies => Event?.QuantumDependencies;

    public IEvent Event {get; private set;}

    public IfEvent(Circuit.Cbit ClassicalBit, int LiteralValue, IEvent evt) {
        this.ClassicalBit = ClassicalBit;
        this.ClassicalDependencies = new Circuit.Cbit[]{ ClassicalBit };
        this.LiteralValue = LiteralValue;

        this.Event = evt;
    }

}

}