using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CY : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Gate.PauliY);
        }
    }
}

public class Y : BaseHermitianOperator , IControllable {
    public IControlledQuantumOperator Controlled() {
        return new CY();
    }
    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.PauliY);
        }
    }
}

}