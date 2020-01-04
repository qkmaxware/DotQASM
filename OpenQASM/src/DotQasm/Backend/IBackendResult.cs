using System;

namespace DotQasm.Backend {

public interface IBackendResult {
    TimeSpan TotalTime {get;}
    TimeSpan ExecutionTime {get;}
}

}