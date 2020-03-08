using System;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Generic scheduled quantum event
/// </summary>
public interface IEvent {
    /// <summary>
    /// Quantum dependencies for this event
    /// </summary>
    IEnumerable<Qubit> QuantumDependencies { get; }
    /// <summary>
    /// Classical dependencies for this event
    /// </summary>
    IEnumerable<Cbit> ClassicalDependencies { get; }
    /// <summary>
    /// Name of the event
    /// </summary>
    string Name {get;}
}

}