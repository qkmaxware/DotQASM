using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class QftGeneratorArguments {
    public int Qubits = 5;
}

public class QftGenerator : ICircuitGenerator<QftGeneratorArguments> {
    public Circuit Generate(QftGeneratorArguments generatorArgs) {
        var Circuit = new Circuit(generatorArgs.Qubits + " Qubit Quantum Fourier Transform");
        var qreg = Circuit.AllocateQubits(generatorArgs.Qubits);

        Operators.Qft op = new Operators.Qft();
        op.Invoke(qreg);
        
        return Circuit;
    }
}

}