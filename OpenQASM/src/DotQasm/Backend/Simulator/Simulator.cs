using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DotQasm.Backend {
    
/// <summary>
/// Simple quantum simulator
/// </summary>
public class Simulator : IBackend {
 
    /// <summary>
    /// Number of qubits in the simulated machine
    /// </summary>
    /// <value>qubit count</value>
    public int QubitCount {get; private set;}
    /// <summary>
    /// Number of possible qubit states
    /// </summary>
    /// <value>state count</value>
    public int StateCount {get; private set;}
 
    private List<Complex> amplitudes;
    /// <summary>
    /// Retrive complex amplitude for a given state
    /// </summary>
    /// <returns>amplitude</returns>
    public Complex this[int state] => (this.amplitudes == null ? new Complex() : this.amplitudes[state]);

    /// <summary>
    /// Create a new simulator 
    /// </summary>
    /// <param name="initialQubits">number of qubits</param>
    public Simulator(int initialQubits = 0) {
        this.QubitCount = initialQubits;
        RecomputeStateCount();
        RebuildAmplitudes();
    }
 
    private void RebuildAmplitudes() {
        List<Complex> na = new List<Complex>(this.StateCount);
        if (amplitudes != null) {
            for(int i = 0; i < amplitudes.Count; i++) {
                if (i >= na.Count)
                    break;
                na[i] = amplitudes[i];
            }
        }
        this.amplitudes = na;
    }
 
    private void RecomputeStateCount() {
        this.StateCount = (this.QubitCount == 0 ? 0 : 1 << this.QubitCount);
    }
 
    /// <summary>
    /// Add qubits to the machine
    /// </summary>
    /// <param name="count">number of qubits to add</param>
    public void AddQubits (int count = 0) {
        var delta = Math.Max(0, count);
        this.QubitCount += delta;
        if (delta != 0) {
            RecomputeStateCount();
            RebuildAmplitudes();
        }
    }
 
    /// <summary>
    /// Check if the backend supports the given gate
    /// </summary>
    /// <param name="op">quantum gate</param>
    /// <returns>true if the backend supports the gate</returns>
    public bool SupportsGate(Gate op) {
        return op.QubitCount == 1;
    }
 
    /// <summary>
    /// Apply a 1 qubit gate
    /// </summary>
    /// <param name="TargetBit">qubit to apply to</param>
    /// <param name="op">gate to apply</param>
    public void ApplyGate (int TargetBit, Gate op) {
        if (!SupportsGate(op)) {
            throw new UnsupportedGateException(op);
        }
        if (TargetBit < 0 || TargetBit >= this.QubitCount) {
            throw new ArgumentOutOfRangeException("Target bit is outside of the range of simulator qubits");
        }
 
        // TODO
    }
 
    /// <summary>
    /// Measure the given qubit
    /// </summary>
    /// <param name="TargetBit">qubit to measure</param>
    public void Measure (int TargetBit) {
        // TODO
    }

    /// <summary>
    /// Apply the controlled version of a single qubit
    /// </summary>
    /// <param name="ControlBit">control qubit</param>
    /// <param name="TargetBit">qubit to apply to</param>
    /// <param name="op">gate to apply</param>
    public void ApplyControlledGate(int ControlBit, int TargetBit, Gate op) {
        if (!SupportsGate(op)) {
            throw new UnsupportedGateException(op);
        }
        if (ControlBit < 0 || ControlBit >= this.QubitCount) {
            throw new ArgumentOutOfRangeException("Control bit is outside of the range of simulator qubits");
        }
        if (TargetBit < 0 || TargetBit >= this.QubitCount) {
            throw new ArgumentOutOfRangeException("Target bit is outside of the range of simulator qubits");
        }
 
        // TODO
    }
 
    /// <summary>
    /// Execute the given quantum circuit
    /// </summary>
    /// <param name="circuit">quantum circuit to execute</param>
    /// <returns>task which returns the results of the simulation</returns>
    public Task Exec(Circuit circuit) {
        // TODO
        /*
        foreach (var ScheduledOperator in circuit.Schedule) {
            ApplyGate(ScheduledOperator.Qubits[0].QubitIndex, ScheduledOperator.Gate);
        }*/
        return null;
    }

    /// <summary>
    /// Check if this backend is available
    /// </summary>
    /// <returns>true if the backend is available to use</returns>
    public bool IsAvailable() {
        return true;
    }
}
 
}
