using System;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class T : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>>, IControllable<IEnumerable<Qubit>>{

    public static readonly Gate TGate = Gate.U1(Math.PI / 4);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(TGate);
        }
    }

    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return new Tdg();
    }

    public IControlledOperator<IEnumerable<Qubit>> Controlled() {
        return new CT();
    }
}

public class CT : BaseControlledOperator {
    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            args.control.ControlledApply(qubit, T.TGate);
        }
    }
}

public class Tdg : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>>, IControllable<IEnumerable<Qubit>> {

    public static readonly Gate TdgGate = Gate.U1(-Math.PI / 4);

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(TdgGate);
        }
    }

    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return new T();
    }

    public IControlledOperator<IEnumerable<Qubit>> Controlled() {
        return new CTdg();
    }
}

public class CTdg : BaseControlledOperator {
    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            args.control.ControlledApply(qubit, Tdg.TdgGate);
        }
    }
}

}