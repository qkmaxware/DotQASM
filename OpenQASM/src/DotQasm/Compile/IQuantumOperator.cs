using System;
using System.Collections.Generic;

namespace DotQasm.Compile
{

/// <summary>
/// Indicates that the given class is a quantum operator
/// </summary>
/// <typeparam name="I">type of value to operate on</typeparam>
public interface IOperator<I> {
    void Invoke(I value);
}

/// <summary>
/// Indicate that a given class is a controlled operator
/// </summary>
public interface IControlledOperator<T> : IOperator<(Qubit control, T register)> {}

public abstract class BaseOperator<T> : IOperator<T> {
    /// <summary>
    /// Invoke the operator on the given inputs
    /// </summary>
    /// <param name="value">inputs</param>
    public abstract void Invoke(T value);
}

/// <summary>
/// Base class for controlled quantum operators, including DSL for applying operations
/// </summary>
public abstract class BaseControlledOperator : BaseOperator<(Qubit control, IEnumerable<Qubit> register)>, IControlledOperator<IEnumerable<Qubit>> {}

/// <summary>
/// Indicate that a given operator has an adjoint
/// </summary>
public interface IAdjoint<T> {
    /// <summary>
    /// Compute or retrieve the adjoint of this operator
    /// </summary>
    IOperator<T> Adjoint();
}

/// <summary>
/// Indicate that a given operator has a controlled version
/// </summary>
public interface IControllable<T> {
    /// <summary>
    /// Compute the controlled version of this operator
    /// </summary>
    IControlledOperator<T> Controlled();
}

/// <summary>
/// Indicate that a given operator has a controlled adjoint
/// </summary>
public interface IControlledAdjoint<T> {
    /// <summary>
    /// Compute or retrieve the controlled adjoint of this operator
    /// </summary>
    IControlledOperator<T> ControlledAdjoint();
}

/// <summary>
/// Base class for an operator that is Hermitian
/// </summary>
public abstract class BaseHermitianOperator : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>> {
    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return this;
    }
}

}