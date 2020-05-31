using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CZ : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Gate.PauliZ);
        }
    }
}

public class Z : BaseHermitianOperator , IControllable {
    public IControlledQuantumOperator Controlled() {
        return new CZ();
    }
    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.PauliZ);
        }
    }
}

}