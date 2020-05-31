using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CX : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Gate.PauliX);
        }
    }
}

public class X : BaseHermitianOperator , IControllable {
    public IControlledQuantumOperator Controlled() {
        return new CX();
    }
    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.PauliX);
        }
    }
}

}