using System;

namespace DotQasm.Backend {

public class IBMJobResults : IBackendResult {
    public string JobId {get; private set;}

    public TimeSpan TotalTime {get; private set;}

    public TimeSpan ExecutionTime {get; private set;}

    public object Data {get; private set;}

    public IBMJobResults(string jobid, TimeSpan taskTime, TimeSpan codeTime, object data) {
        this.JobId = jobid;
        this.TotalTime = taskTime;
        this.ExecutionTime = codeTime;
        this.Data = data;
    }
}

}