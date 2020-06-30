using DotQasm.Scheduling;

namespace DotQasm.Optimization {

/// <summary>
/// Generic optimization strategy
/// </summary>
/// <typeparam name="From"></typeparam>
/// <typeparam name="To"></typeparam>
public interface IOptimization<From,To> : System.IChainableAction<From,To> where From:ISchedule where To:ISchedule {
    /// <summary>
    /// Optimization name
    /// </summary>
    string Name {get;}
    /// <summary>
    /// Optimization description
    /// </summary>
    string Description {get;}
}

}