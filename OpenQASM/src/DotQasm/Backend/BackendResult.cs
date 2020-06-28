using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DotQasm.Backend {

/// <summary>
/// Base class for the results of a quantum circuit's execution
/// </summary>
public class BackendResult {
    /// <summary>
    /// Name of the backend the results came from
    /// </summary>
    public string BackendName {get; protected set;}
    /// <summary>
    /// Total time spent being processed
    /// </summary>
    public TimeSpan TotalTime {get; protected set;}
    /// <summary>
    /// Total time spend being run
    /// </summary>
    public TimeSpan ExecutionTime {get; protected set;}
    /// <summary>
    /// Probabilities per each potential state
    /// </summary>
    public Dictionary<int, double> StateProbabilityHistogram {get; protected set;}
    /// <summary>
    /// Most likely state
    /// </summary>
    public int State => StateProbabilityHistogram.Aggregate((current, next) => current.Value > next.Value ? current : next).Key;

    /// <summary>
    /// Create an empty backend result
    /// </summary>
    public BackendResult() {}

    /// <summary>
    /// Unique identifier for this result
    /// </summary>
    public virtual string ResultIdentifier() {
        return this.GetHashCode().ToString();
    }

    /// <summary>
    /// Create a summary report for these results
    /// </summary>
    /// <returns>stringified report</returns>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        
        int col1 = 32; int col2 = 50; int col3 = 24;
        string format = "| {0,-"+col1+"} | {1,-"+col2+"} | {2,-"+col3+"} |";
        sb.AppendLine(new string('-', col1 + col2 + col3 + 10));
        sb.AppendLine(string.Format(format, "Backend" , "Total Time", "Execution Time"));
        sb.AppendLine(new string('-', col1 + col2 + col3 + 10));
        sb.AppendLine(string.Format(format, BackendName, TotalTime, ExecutionTime));

        if (StateProbabilityHistogram != null) {
            sb.AppendLine();
            sb.AppendLine(new string('-', col1 + 4));
            sb.AppendLine(string.Format("| {0,-"+col1+"} |", "Histogram"));
            sb.AppendLine(new string('-', col1 + 4));
            int stateCol = Convert.ToString(StateProbabilityHistogram.Keys.Max(), 2).Length;

            string histogramFormat = "{0} |{1} {2:0.##} %";
            foreach (var state in StateProbabilityHistogram) {
                var probability = state.Value;
                var percent = probability * 100;
                sb.AppendLine(
                    string.Format(
                        histogramFormat, 
                        Convert.ToString(state.Key, 2).PadLeft(stateCol, '0'), 
                        new string('|', (int)percent / 2), 
                        percent
                    )
                );
            }
        }


        return sb.ToString();
    }
}

}