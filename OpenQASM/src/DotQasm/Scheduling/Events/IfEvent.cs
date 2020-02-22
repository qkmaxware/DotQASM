using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Event for classical conditional checking 
/// </summary>
public class IfEvent: IEvent {

    public int LiteralValue {get; private set;}
    public IEnumerable<Cbit> ClassicalDependencies {get; protected set;}
    public IEnumerable<Qubit> QuantumDependencies => Event?.QuantumDependencies;

    public IEvent Event {get; private set;}

    public IfEvent(IEnumerable<Cbit> ClassicalBits, int LiteralValue, IEvent evt) {
        this.ClassicalDependencies = ClassicalBits;
        this.LiteralValue = LiteralValue;

        this.Event = evt;
    }

}

}