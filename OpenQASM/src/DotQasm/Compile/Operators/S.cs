using System;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class S : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>>, IControllable<IEnumerable<Qubit>> {

    public static readonly Gate SGate = Gate.U1(Math.PI / 2);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(SGate);
        }
    }

    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return new Sdg();
    }

    public IControlledOperator<IEnumerable<Qubit>> Controlled() {
        return new CS();
    }
}

public class CS : BaseControlledOperator {
    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            args.control.ControlledApply(qubit, S.SGate);
        }
    }
}

public class Sdg : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>>, IControllable<IEnumerable<Qubit>> {

    public static readonly Gate SdgGate = Gate.U1(-Math.PI / 2);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(SdgGate);
        }
    }

    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return new S();
    }

    public IControlledOperator<IEnumerable<Qubit>> Controlled() {
        return new CSdg();
    }
}

public class CSdg : BaseControlledOperator {
    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            args.control.ControlledApply(qubit, Sdg.SdgGate);
        }
    }
}

}