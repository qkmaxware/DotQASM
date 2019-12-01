using System;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public interface IEvent {
    IEnumerable<Circuit.Qubit> QuantumDependencies { get; }
    IEnumerable<Circuit.Cbit> ClassicalDependencies { get; }
}

}