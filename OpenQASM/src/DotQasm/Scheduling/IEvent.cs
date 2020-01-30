using System;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public interface IEvent {
    IEnumerable<Qubit> QuantumDependencies { get; }
    IEnumerable<Cbit> ClassicalDependencies { get; }
}

}