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
public interface IControlledQuantumOperator {
    /// <summary>
    /// Invoke the controlled operator on the given qubits
    /// </summary>
    /// <param name="control">control qubit</param>
    /// <param name="register">target qubit(s)</param>
    void Invoke(Qubit control, IEnumerable<Qubit> register);
}

/// <summary>
/// Base class for quantum operators, including DSL for applying operations
/// </summary>
public abstract class BaseQuantumOperator : IQuantumOperator {
    /// <summary>
    /// Invoke the operator on the given qubits
    /// </summary>
    /// <param name="register">target qubit(s)</param>
    public abstract void Invoke(IEnumerable<Qubit> register);

    /// <summary>
    /// DSL for applying a quantum operator to a register
    /// </summary>
    /// <param name="op"></param>
    /// <param name="register"></param>
    /// <returns></returns>
    public static IEnumerable<Qubit> operator | (BaseQuantumOperator op, IEnumerable<Qubit> register) {
        op.Invoke(register);
        return register;
    } 

    /// <summary>
    /// DSL for applying a quantum operator to a single qubit
    /// </summary>
    /// <param name="op"></param>
    /// <param name="qubit"></param>
    /// <returns></returns>
    public static IEnumerable<Qubit> operator | (BaseQuantumOperator op, Qubit qubit) {
        return op | new Qubit[]{ qubit };
    }
}

/// <summary>
/// Base class for controlled quantum operators, including DSL for applying operations
/// </summary>
public abstract class BaseControlledQuantumOperator : IControlledQuantumOperator {
    /// <summary>
    /// Invoke the operator on the given qubits
    /// </summary>
    /// <param name="control">control qubit</param>
    /// <param name="register">target qubit(s)</param>
    public abstract void Invoke(Qubit control, IEnumerable<Qubit> register);

    /// <summary>
    /// DSL for applying a quantum operator to a register
    /// </summary>
    /// <param name="op">operator</param>
    /// <param name="qubits">tuple with control qubit and target register</param>
    /// <returns>target register</returns>
    public static IEnumerable<Qubit> operator | (BaseControlledQuantumOperator op, (Qubit ctrl, IEnumerable<Qubit> register) qubits) {
        op.Invoke(qubits.ctrl, qubits.register);
        return qubits.register;
    } 

    /// <summary>
    /// DSL for applying a quantum operator to a single qubit
    /// </summary>
    /// <param name="op">operator</param>
    /// <param name="qubits">tuple with control qubit and target qubit</param>
    /// <returns>target qubit</returns>
    public static IEnumerable<Qubit> operator | (BaseControlledQuantumOperator op, (Qubit ctrl, Qubit qubit) qubits) {
        var register = new Qubit[]{ qubits.qubit };
        op.Invoke(qubits.ctrl, register);
        return register;
    }
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
/// Indicate that a given operator has a controlled version
/// </summary>
public interface IControllable {
    /// <summary>
    /// Compute the controlled version of this operator
    /// </summary>
    IControlledQuantumOperator Controlled();
}

/// <summary>
/// Indicate that a given operator has a controlled adjoint
/// </summary>
public interface IControlledAdjoint {
    /// <summary>
    /// Compute or retrieve the controlled adjoint of this operator
    /// </summary>
    IControlledQuantumOperator ControlledAdjoint();
}

/// <summary>
/// Base class for an operator that is Hermitian
/// </summary>
public abstract class BaseHermitianOperator : BaseQuantumOperator, IAdjoint {
    public IQuantumOperator Adjoint() {
        return this;
    }
}

}