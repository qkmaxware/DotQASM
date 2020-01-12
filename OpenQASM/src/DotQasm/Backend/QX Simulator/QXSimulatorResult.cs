using System;
using System.Diagnostics;

namespace DotQasm.Backend.QX {

public class QXSimulatorResult : IBackendResult {
    public TimeSpan TotalTime => throw new NotImplementedException();

    public TimeSpan ExecutionTime => throw new NotImplementedException();
}

}