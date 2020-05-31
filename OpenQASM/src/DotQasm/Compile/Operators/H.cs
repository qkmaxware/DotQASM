using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CH : BaseControlledQuantumOperator {
    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, Gate.Hadamard);
        }
    }
}

public class H : BaseHermitianOperator, IControllable {
    public IControlledQuantumOperator Controlled() {
        return new CH();
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.Hadamard);
        }
    }
}

}