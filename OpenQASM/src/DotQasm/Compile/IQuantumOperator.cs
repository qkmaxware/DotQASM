using System;
using System.Collections.Generic;

namespace DotQasm.Compile {

/// <summary>
/// Indicates that the given class is a quantum operator
/// </summary>
public interface IQuantumOperator {
    /// <summary>
    /// Invoke the operator on the given qubits
    /// </summary>
    /// <param name="register">target qubit(s)</param>
    void Invoke(IEnumerable<Qubit> register);
}

/// <summary>
/// Indicate that a given class is a controlled operator
/// </summary>
public interface IControlledOperator {
    /// <summary>
    /// Invoke the controlled operator on the given qubits
    /// </summary>
    /// <param name="control">control qubit</param>
    /// <param name="register">target qubit(s)</param>
    void Invoke(Qubit control, IEnumerable<Qubit> register);
}

/// <summary>
/// Indicate that a given operator has an adjoint
/// </summary>
public interface IAdjoint {
    /// <summary>
    /// Compute or retrieve the adjoint of this operator
    /// </summary>
    IQuantumOperator Adjoint();
}

/// <summary>
/// Indicate that a given operator has a controlled adjoint
/// </summary>
public interface IControlledAdjoint {
    /// <summary>
    /// Compute or retrieve the controlled adjoint of this operator
    /// </summary>
    IControlledOperator ControlledAdjoint();
}

}