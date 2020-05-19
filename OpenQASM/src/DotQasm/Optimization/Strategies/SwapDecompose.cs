using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using DotQasm.Scheduling;

namespace DotQasm.Optimization.Strategies {

public class SwapDecompose : BaseOptimizationStrategy<LinearSchedule, LinearSchedule> {

    public override string Name => "Swap Decompose";

    public override string Description => "Decompose swap events into hardware specific gates";

    public override LinearSchedule Transform(LinearSchedule schedule) {
        List<IEvent> newSchedule = new List<IEvent>();

        foreach (var evt in schedule) {
            switch (evt) {
                case SwapEvent swapEvent: {
                    // SWAP is 3 CX operations on many quantum computers
                    newSchedule.Add(new ControlledGateEvent(Gate.PauliX, swapEvent.QuantumDependencies.ElementAt(0), new Qubit[]{swapEvent.QuantumDependencies.ElementAt(1)}));
                    newSchedule.Add(new ControlledGateEvent(Gate.PauliX, swapEvent.QuantumDependencies.ElementAt(1), new Qubit[]{swapEvent.QuantumDependencies.ElementAt(0)}));
                    newSchedule.Add(new ControlledGateEvent(Gate.PauliX, swapEvent.QuantumDependencies.ElementAt(0), new Qubit[]{swapEvent.QuantumDependencies.ElementAt(1)}));
                } break;
                default: {
                    newSchedule.Add(evt);
                } break;
            }
        }

        return new LinearSchedule(newSchedule);
    }
}

}