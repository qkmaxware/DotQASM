using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class CRx : BaseControlledQuantumOperator {
    private Gate gate;

    public CRx(Gate gate) {
        this.gate = gate;
    }
    public CRx(float angle) : this(Gate.Rx(angle)) {}

    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, gate);
        }
    }
}


public class Rx : BaseHermitianOperator, IControllable {

    private Gate gate;

    public Rx(float angle) {
        this.gate = Gate.Rx(angle);
    }

    public IControlledQuantumOperator Controlled() {
        return new CRx(gate);
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(gate);
        }
    }
}

public class CRy : BaseControlledQuantumOperator {
    private Gate gate;

    public CRy(Gate gate) {
        this.gate = gate;
    }
    public CRy(float angle) : this(Gate.Ry(angle)) {}

    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, gate);
        }
    }
}

public class Ry : BaseHermitianOperator , IControllable {

    private Gate gate;

    public Ry(float angle) {
        this.gate = Gate.Ry(angle);
    }

    public IControlledQuantumOperator Controlled() {
        return new CRy(gate);
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(gate);
        }
    }
}

public class CRz : BaseControlledQuantumOperator {
    private Gate gate;

    public CRz(Gate gate) {
        this.gate = gate;
    }
    public CRz(float angle) : this(Gate.Rz(angle)) {}

    public override void Invoke(Qubit control, IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            control.ControlledApply(qubit, gate);
        }
    }
}

public class Rz : BaseHermitianOperator , IControllable {

    private Gate gate;

    public Rz(float angle) {
        this.gate = Gate.Rz(angle);
    }

    public IControlledQuantumOperator Controlled() {
        return new CRz(gate);
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        foreach (var qubit in register) {
            qubit.Apply(gate);
        }
    }
}

}