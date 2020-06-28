using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class QftGenerator : ICircuitGenerator<int> {
    public Circuit Generate(int qubits = 5) {
        var Circuit = new Circuit(qubits + " Qubit Quantum Fourier Transform");
        var qreg = Circuit.AllocateQubits(qubits);

        Operators.Qft op = new Operators.Qft();
        op.Invoke(qreg);
        
        return Circuit;
    }
}

}