using System.IO;
using DotQasm.Scheduling;

namespace DotQasm.Optimization.Strategies {

public abstract class BaseOptimizationStrategy<From, To>: IOptimization<From, To> where From:ISchedule where To:ISchedule {
    /// <summary>
    /// Optimization name
    /// </summary>
    public abstract string Name {get;}
    /// <summary>
    /// Optimization description
    /// </summary>
    public abstract string Description {get;}
    /// <summary>
    /// Transform from one schedule to an optimized one
    /// </summary>
    /// <param name="schedule">schedule to optimize</param>
    /// <returns>optimized schedule</returns>
    public abstract To Transform(From schedule);

    /// <summary>
    /// Directory to store generated data files
    /// </summary>
    protected string DataDirectoryPath => "./.qasmdata";

    /// <summary>
    /// Ensure the data file directory exists, create it if it does not
    /// </summary>
    protected void MakeDataDirectory() {
        if (!Directory.Exists(DataDirectoryPath)) {
            Directory.CreateDirectory(DataDirectoryPath);
        }
    }

    /// <summary>
    /// Create a file within the data directory
    /// </summary>
    /// <param name="filename">filename</param>
    /// <returns>Writer to edit the file</returns>
    protected StreamWriter MakeDataFile(string filename) {
        MakeDataDirectory();
        var path = Path.Join(DataDirectoryPath, filename);
        return new StreamWriter(path);
    }
}

}