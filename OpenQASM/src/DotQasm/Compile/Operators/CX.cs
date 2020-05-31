using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CNot : BaseQuantumOperator {
    private Qubit control;

    public CNot(Qubit control) {
        this.control = control;
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var target in register) {
            control.CX(target);
        }
    }
}

}