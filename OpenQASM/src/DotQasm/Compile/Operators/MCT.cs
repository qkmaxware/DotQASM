using System.Linq;
using System.Collections.Generic;
using System;

namespace DotQasm.Compile.Operators {

/// <summary>
/// Multi-controlled Toffoli gate, does not allocate additional ancilla
/// </summary>
public class MCT : BaseOperator<(IEnumerable<Qubit> controls, Qubit target)> {

    private static MCU1 mcu_pi = new MCU1(Math.PI);

    public override void Invoke((IEnumerable<Qubit> controls, Qubit target) value) {
        // https://qiskit.org/documentation/_modules/qiskit/circuit/library/standard_gates/x.html
        // MCXGrayCode is the MCT using Grey codes without requiring ancilla

        value.target.H();
        mcu_pi.Invoke(value);
        value.target.H();
    }
}

}