using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.Local {

/// <summary>
/// Results of quantum circuit execution specific to the local simulator
/// </summary>
public class SimulatorResult: BackendResult {

    public IEnumerable<Complex> State {get; private set;}
    public IEnumerable<State> ClassicalRegister {get; private set;}

    public SimulatorResult(IBackend backend, TimeSpan time, IEnumerable<Complex> state, IEnumerable<State> register) {
        // Custom stuff
        this.State = state;
        this.ClassicalRegister = register;

        // Inherited stuff
        this.BackendName = backend.GetType().ToString();
        this.TotalTime = time;
        this.ExecutionTime = time;
        this.StateProbabilityHistogram = this.State.Select((amplitude, state) => new KeyValuePair<int, double>(
            state, amplitude.SqrMagnitude()
        )).ToDictionary(pair => pair.Key, pair => pair.Value);

    }
}

}