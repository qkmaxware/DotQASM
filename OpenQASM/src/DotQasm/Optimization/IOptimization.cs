using DotQasm.Scheduling;

namespace DotQasm.Optimization {

/// <summary>
/// Generic optimization strategy
/// </summary>
/// <typeparam name="From"></typeparam>
/// <typeparam name="To"></typeparam>
public interface IOptimization<From,To> where From:ISchedule where To:ISchedule {
    /// <summary>
    /// Optimization name
    /// </summary>
    string Name {get;}
    /// <summary>
    /// Optimization description
    /// </summary>
    string Description {get;}
    /// <summary>
    /// Transform from one schedule to an optimized one
    /// </summary>
    /// <param name="schedule">schedule to optimize</param>
    /// <returns>optimized schedule</returns>
    To Transform(From schedule);
}

}