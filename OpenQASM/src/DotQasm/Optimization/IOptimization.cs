using DotQasm.Scheduling;

namespace DotQasm.Optimization {

public interface IOptimization<From,To> where From:ISchedule where To:ISchedule {
    string Name {get;}
    string Description {get;}
    To Transform(From schedule);
}

}