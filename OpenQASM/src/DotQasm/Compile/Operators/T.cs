using System;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class T : BaseQuantumOperator, IAdjoint, IControllable {

    public static readonly Gate TGate = Gate.U1(Math.PI / 4);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(TGate);
        }
    }

    public IQuantumOperator Adjoint() {
        return new Tdg();
    }

    public IControlledQuantumOperator Controlled() {
        return new CT();
    }
}

public class CT : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, T.TGate);
        }
    }
}

public class Tdg : BaseQuantumOperator, IAdjoint, IControllable {

    public static readonly Gate TdgGate = Gate.U1(-Math.PI / 4);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(TdgGate);
        }
    }

    public IQuantumOperator Adjoint() {
        return new T();
    }

    public IControlledQuantumOperator Controlled() {
        return new CTdg();
    }
}

public class CTdg : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Tdg.TdgGate);
        }
    }
}

}