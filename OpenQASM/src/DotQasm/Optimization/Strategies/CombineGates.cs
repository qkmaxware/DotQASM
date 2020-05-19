using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using DotQasm.Scheduling;

namespace DotQasm.Optimization.Strategies {

public class CombineGates : BaseOptimizationStrategy<LinearSchedule, LinearSchedule> {

    public override string Name => "Combine";

    public override string Description => "Combine neighboring gates into a single gate";

    private bool CanCombine(GateEvent second, GateEvent first) {
        if (
            first.QuantumDependencies.Count() == second.QuantumDependencies.Count() 
            &&
            first.QuantumDependencies.All(value => second.QuantumDependencies.Contains(value))
        ) {
            return true;
        } else {
            return false;
        }
    }

    private GateEvent Combine(GateEvent second, GateEvent first) {
        // first * second
        return new GateEvent(
            first.Operator.Multiply(second.Operator),
            first.QuantumDependencies
        );
    }

    public override LinearSchedule Transform(LinearSchedule schedule) {
        List<IEvent> newSchedule = new List<IEvent>();

        // Start at the last event and work my way towards the first event
        GateEvent previous = null;
        foreach (var next in schedule.Reverse()) {
            if (next is GateEvent) {
                if (previous != null) {
                    if (CanCombine(previous, (GateEvent)next)) {
                        previous = Combine(previous, (GateEvent)next);
                    } else {
                        newSchedule.Add(previous);
                        previous = (GateEvent)next;
                    }
                } else {
                    previous = (GateEvent)next;
                }
            } else {
                if (previous != null) {
                    newSchedule.Add(previous);
                    previous = null;
                }
                newSchedule.Add(next);
            }
        }

        if (previous != null) {
            newSchedule.Add(previous);
        }

        newSchedule.Reverse();
        return new LinearSchedule(newSchedule);
    }
}

}