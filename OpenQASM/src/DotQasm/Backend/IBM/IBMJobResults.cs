using System;
using System.Text;
using System.Linq;
using System.Text.Json;

namespace DotQasm.Backend.IBM {

/// <summary>
/// Results of quantum circuit execution specific to the IBM backends
/// </summary>
public class IBMJobResults : BackendResult {
    public string JobId => Job.id;
    public bool Success {get; private set;}

    private Api.IBMApiJob Job {get; set;}
    public Api.IBMApiQObjectResult RawResult {get; private set;}

    public IBMJobResults(IBackend backend, TimeSpan taskTime, Api.IBMApiJob job) {
        // Custom stuff
        this.Job = job;
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
        StringBuilder sb = new StringBuilder();

        // Inherited stuff
        sb.AppendLine(base.ToString());

        // Custom stuff
        int col1 = 32;
        sb.AppendLine(new string('-', col1 + 4));
        sb.AppendLine(string.Format("| {0,-"+col1+"} |", "Job"));
        sb.AppendLine(new string('-', col1 + 4));
        sb.AppendLine(JobId);
        sb.AppendLine();

        sb.AppendLine(new string('-', col1 + 4));
        sb.AppendLine(string.Format("| {0,-"+col1+"} |", "Quantum Object"));
        sb.AppendLine(new string('-', col1 + 4));
        sb.AppendLine(JsonSerializer.Serialize(Job.qObject));
        sb.AppendLine();

        return sb.ToString();
    }
}

}