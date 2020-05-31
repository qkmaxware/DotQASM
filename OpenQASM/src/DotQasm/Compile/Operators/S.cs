using System;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class S : BaseQuantumOperator, IAdjoint, IControllable {

    public static readonly Gate SGate = Gate.U1(Math.PI / 2);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(SGate);
        }
    }

    public IQuantumOperator Adjoint() {
        return new Sdg();
    }

    public IControlledQuantumOperator Controlled() {
        return new CS();
    }
}

public class CS : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, S.SGate);
        }
    }
}

public class Sdg : BaseQuantumOperator, IAdjoint, IControllable {

    public static readonly Gate SdgGate = Gate.U1(-Math.PI / 2);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(SdgGate);
        }
    }

    public IQuantumOperator Adjoint() {
        return new S();
    }

    public IControlledQuantumOperator Controlled() {
        return new CSdg();
    }
}

public class CSdg : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Sdg.SdgGate);
        }
    }
}

}