using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CH : BaseControlledOperator {
    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            args.control.ControlledApply(qubit, Gate.Hadamard);
        }
    }
}

public class H : BaseHermitianOperator, IControllable<IEnumerable<Qubit>> {
    public IControlledOperator<IEnumerable<Qubit>> Controlled() {
        return new CH();
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(Gate.Hadamard);
        }
    }
}

}