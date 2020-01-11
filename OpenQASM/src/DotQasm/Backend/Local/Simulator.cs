using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DotQasm.Backend.Local {
    
/// <summary>
/// Simple quantum simulator
/// </summary>
public class Simulator : IBackend<SimulatorResult> {
 
    private static Random rnd = new Random();

    /// <summary>
    /// Set the random seed used in measurement
    /// </summary>
    /// <param name="seed">random seed</param>
    public static void SetRandomSeed(int seed) {
        rnd = new Random(seed);
    }

    /// <summary>
    /// Number of qubits in the simulated machine
    /// </summary>
    /// <value>qubit count</value>
    public int QubitCount {get; private set;}
    /// <summary>
    /// Number of possible qubit states
    /// </summary>
    /// <value>state count</value>
    public int StateCount => (this.QubitCount == 0 ? 0 : 1 << this.QubitCount);
 
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
        RebuildAmplitudes();
        this.amplitudes[0] = new Complex(1,0); // Start off in the first '|0..0> = 0' state
    }
 
    private void RebuildAmplitudes() {
        int oldStateCount = (this.amplitudes != null ? this.amplitudes.Count : 0);
        List<Complex> na = new List<Complex>(this.StateCount);
        for (int i = 0; i < na.Capacity; i++) {
            na.Add(Complex.Zero);
        }
        // TODO update state probabilities from old set if it exists
        this.amplitudes = na;
    }

    /// <summary>
    /// forcefully normalize the state vector in case accumulated floating point errors are too much
    /// </summary>
    public void ForceNormalize() {
        double sum = 0;
        foreach (var amp in this.amplitudes) {
            sum += amp.SqrMagnitude();
        }
        
        if (sum != 0) {
            var mag = Math.Sqrt(sum);
            for (int i = 0; i < this.amplitudes.Count; i++) {
                this.amplitudes[i] /= mag;
            }
        }
    }
 
    private int QubitMask(int qubit) {
        return (1 << (QubitCount - 1)) >> qubit;
    }

    /// <summary>
    /// Add qubits to the machine, qubits appended to the left of the previous
    /// </summary>
    /// <param name="count">number of qubits to add</param>
    private void AddQubits (int count = 0) {
        var delta = Math.Max(0, count);
        this.QubitCount += delta;
        if (delta != 0) {
            RebuildAmplitudes(); //TODO preserve old probabilities
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
 
        // Bitshift left to put a 1 on left leftmost bit, then bitshift right to select the qubit at the given index
        int stride = QubitMask(TargetBit);
        int stride_gap = stride << 1;
        for (int g = 0; g < StateCount; g+=stride_gap) {
            for(int i = g; i < g + stride; i++) {
                Complex ai = op.Matrix[0,0] * amplitudes[i] + op.Matrix[0,1] * amplitudes[i + stride];
                Complex ai2= op.Matrix[1,0] * amplitudes[i] + op.Matrix[1,1] * amplitudes[i + stride];

                amplitudes[i] = ai;
                amplitudes[i+stride] = ai2;       
            }
        }
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

        int controlStride = QubitMask(ControlBit);
        int targetStride = QubitMask(TargetBit);
        int stride_gap = targetStride << 1;

        for (int g = 0; g < StateCount; g+=stride_gap) {
            for(int i = g; i < g + targetStride; i++) {
                if ((i & controlStride) < 1) {
                    // Control bit is not set
                    continue;
                }
                Complex ai = op.Matrix[0,0] * amplitudes[i] + op.Matrix[0,1] * amplitudes[i + targetStride];
                Complex ai2= op.Matrix[1,0] * amplitudes[i] + op.Matrix[1,1] * amplitudes[i + targetStride];

                amplitudes[i] = ai;
                amplitudes[i+targetStride] = ai2;       
            }
        }
        /*
        for (int state = 0; state < this.StateCount; state++) {
            if ((state & controlStride) < 1) {
                // Control bit is not set
                continue;
            }
            var mask = state & targetStride;

            var state1 = state & ~mask; //amplitudes[*1c*0t*...]
            var state2 = state | mask;  //amplitudes[*1c*1t*...]

            var amp1 = op.Matrix[0,0] * amplitudes[state1] + op.Matrix[0,1] * amplitudes[state2];
            var amp2 = op.Matrix[1,0] * amplitudes[state1] + op.Matrix[1,1] * amplitudes[state2];

            this.amplitudes[state1] = amp1;
            this.amplitudes[state2] = amp2;
        }*/
    }

    /// <summary>
    /// Measure the given qubit, collapse the remainder of the states
    /// </summary>
    /// <param name="TargetBit">qubit to measure</param>
    /// <returns>0 or 1 for the given qubit state</returns>
    public State Measure (int TargetBit) {
        if (TargetBit < 0 || TargetBit >= this.QubitCount) {
            throw new ArgumentOutOfRangeException("Target bit is outside of the range of simulator qubits");
        }

        double prob0 = 0;
        double prob1 = 0;
        int mask = QubitMask(TargetBit);

        // Compute probabilities 
        // TODO lots of calls to sqrt, try to do better
        for (int state = 0; state < StateCount; state++) {
            if ((state & mask) > 0) {
                // State has a '1' at qubit
                prob1 += amplitudes[state].SqrMagnitude();
            } else {
                // State has a 0 at qubit
                prob0 += amplitudes[state].SqrMagnitude();
            }
        }

        // Select either 0 or 1 along the number line
        double position = rnd.NextDouble();
        bool IsZero = position <= prob0;

        // Collapse all the states
        // If the state is one we selected, renormalize
        // Else set the state probability to 0
        var magnitude = Math.Sqrt((IsZero ? prob0 : prob1));
        for (int state = 0; state < StateCount; state++) {
            if ((state & mask) > 0) {
                // State has a '1'
                amplitudes[state] = (!IsZero ? amplitudes[state] / magnitude : new Complex(0,0));
            } else {
                // State has a '0'
                amplitudes[state] = (IsZero ? amplitudes[state] / magnitude : new Complex(0,0));
            }
        }

        return IsZero ? State.Zero : State.One;
    }

    /// <summary>
    /// Measure all qubits
    /// </summary>
    /// <returns>integer with each bit set to the measured value of the qubit</returns>
    public int MeasureAll() {
        // Randomly pick a state based on probabilities
        int result = 0;
        double prob = rnd.NextDouble();
        double selectedProb = 0;
        for (int state = 0; state < this.amplitudes.Count; state++) {
            selectedProb += amplitudes[state].SqrMagnitude();
            if (prob <= selectedProb) {
                result = state;
                break;
            }
        }

        // Collapse to that state
        for (int state = 0; state < this.amplitudes.Count; state++) {
            if (state == result) {
                this.amplitudes[state] = Complex.One;
            } else {
                this.amplitudes[state] = Complex.Zero;
            }
        }

        return result;
    }
 
    /// <summary>
    /// Prepare qubit in the |0> state
    /// </summary>
    /// <param name="TargetBit">qubit to reset to the |0> state</param>
    public void Zero(int TargetBit) {
        if (TargetBit < 0 || TargetBit >= this.QubitCount) {
            throw new ArgumentOutOfRangeException("Target bit is outside of the range of simulator qubits");
        }
        int mask = QubitMask(TargetBit);

        State state = Measure(TargetBit);
        switch (state) {
            case State.Zero:
                break; // Already got the state we want
            case State.One:
                ApplyGate(TargetBit, Gate.PauliX); // Flip the bit
                break;
        }
    }

    /// <summary>
    /// Execute the given quantum circuit
    /// </summary>
    /// <param name="circuit">quantum circuit to execute</param>
    /// <returns>task which returns the results of the simulation</returns>
    public Task<SimulatorResult> Exec(Circuit circuit) {
        return new Task<SimulatorResult>(() => {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<int> register = new List<int>(new int[circuit.BitCount]);
            
            // TODO
            /*
            foreach (var ScheduledOperator in circuit.Schedule) {
                ApplyGate(ScheduledOperator.Qubits[0].QubitIndex, ScheduledOperator.Gate);
            }*/

            return new SimulatorResult(watch.Elapsed, amplitudes.AsReadOnly(), register);
        });
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
