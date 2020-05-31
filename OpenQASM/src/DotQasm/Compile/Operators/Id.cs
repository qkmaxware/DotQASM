using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CId : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Gate.Identity);
        }
    }
}

public class Id : BaseHermitianOperator, IControllable {
    public IControlledQuantumOperator Controlled() {
        return new CId();
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.Identity);
        }
    }
}

}