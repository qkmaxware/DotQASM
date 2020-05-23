using System;
using System.Collections.Generic;

namespace DotQasm.Compile {

/// <summary>
/// Indicates that the given class is a quantum operator
/// </summary>
public interface IQuantumOperator {
    void Invoke(IEnumerable<Qubit> register);
}

}