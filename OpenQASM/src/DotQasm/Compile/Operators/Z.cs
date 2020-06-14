using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CZ : BaseControlledOperator {
    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            qubit.H();
            args.control.CX(qubit);
            qubit.H();
        }
    }
}

public class Z : BaseHermitianOperator , IControllable<IEnumerable<Qubit>> {
    public IControlledOperator<IEnumerable<Qubit>> Controlled() {
        return new CZ();
    }
    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.PauliZ);
        }
    }
}

}