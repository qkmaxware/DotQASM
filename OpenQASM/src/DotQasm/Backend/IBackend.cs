using System;
using System.Threading.Tasks;

namespace DotQasm.Backend {

/// <summary>
/// Interface representing any backend that can run a quantum circuit
/// </summary>
public interface IBackend {
    /// <summary>
    /// Check if backend is available
    /// </summary>
    /// <returns>true if the backend is available</returns>
    bool IsAvailable();
    /// <summary>
    /// Check if the backend supports a given gate
    /// </summary>
    /// <param name="gate">gate to test</param>
    /// <returns>true if the gate is supported</returns>
    bool SupportsGate(Gate gate);
    /// <summary>
    /// Execute the given quantum circuit
    /// </summary>
    /// <param name="circuit">circuit to execute</param>
    /// <returns>Task with the eventual results of the circuit</returns>
    Task<BackendResult> Exec(Circuit circuit);
}

}