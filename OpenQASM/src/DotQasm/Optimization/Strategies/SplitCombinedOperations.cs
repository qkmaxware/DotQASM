using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using DotQasm.Scheduling;

namespace DotQasm.Optimization.Strategies {

public class SplitCombinedOperations : IOptimization<LinearSchedule, LinearSchedule> {

    public string Name => "Split";

    public string Description => "Split operations on many qubits into a series each acting on one qubit";

    public LinearSchedule Transform(LinearSchedule schedule) {
        List<IEvent> newSchedule = new List<IEvent>();

        foreach (var evt in schedule) {
            switch (evt) {
                // Gate on many Qubits -> many gates applied to one qubit each
                case GateEvent gateEvent: {
                    foreach (var qubit in gateEvent.QuantumDependencies) {
                        var ng = new GateEvent(gateEvent.Operator, qubit);
                        newSchedule.Add(ng);
                    }
                } break;
                // Controlled gate with many targets -> many controlled gates with one target each
                case ControlledGateEvent controlledGate: {
                    foreach (var qubit in controlledGate.TargetQubits) {
                        var ng = new ControlledGateEvent(controlledGate.Operator, controlledGate.ControlQubit, qubit);
                        newSchedule.Add(ng);
                    }
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