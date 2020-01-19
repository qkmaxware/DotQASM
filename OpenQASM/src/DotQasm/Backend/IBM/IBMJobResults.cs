using System;
using System.Text;
using System.Linq;

namespace DotQasm.Backend.IBM {

public class IBMJobResults : BackendResult {
    public string JobId {get; private set;}
    public bool Success {get; private set;}

    public Api.IBMApiQObjectResult RawResult {get; private set;}

    public IBMJobResults(IBackend backend, string jobid, TimeSpan taskTime, Api.IBMApiJob job) {
        // Custom stuff
        this.JobId = jobid;
        this.RawResult = job.qObjectResult;

        // Inherited stuff
        this.BackendName = backend.GetType().ToString();
        this.TotalTime = taskTime;
        this.ExecutionTime = RawResult != null ? TimeSpan.FromSeconds(RawResult.time_taken) : this.TotalTime;
        this.Success = job.HasResults();
        if (RawResult != null && RawResult.results.Length > 0) {
            double totalShots = RawResult.results[0].shots;
            this.StateProbabilityHistogram = RawResult.results[0].data.counts.ToDictionary(
                pair => Convert.ToInt32(pair.Key, 16),
                pair => pair.Value / totalShots
            );
        }
    }

    public override string ToString() {
        // Custom stuff
        StringBuilder sb = new StringBuilder();
        int col1 = 32;
        sb.AppendLine(new string('-', col1 + 4));
        sb.AppendLine(string.Format("| {0,-"+col1+"} |", "Job"));
        sb.AppendLine(new string('-', col1 + 4));
        sb.AppendLine(JobId);
        sb.AppendLine();

        // Inherited stuff
        sb.Append(base.ToString());

        return sb.ToString();
    }
}

}