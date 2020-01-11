using System;
using System.Numerics;
using System.Collections.Generic;

namespace DotQasm.Backend.Local {

public class SimulatorResult: IBackendResult {

    public TimeSpan TotalTime {get; private set;}

    public TimeSpan ExecutionTime {get; private set;}

    public IEnumerable<Complex> State {get; private set;}
    public IEnumerable<int> ClassicalRegister {get; private set;}

    public SimulatorResult(TimeSpan time, IEnumerable<Complex> state, IEnumerable<int> register) {
        TotalTime = time;
        ExecutionTime = time;
        this.State = state;
        this.ClassicalRegister = register;
    }
}

}