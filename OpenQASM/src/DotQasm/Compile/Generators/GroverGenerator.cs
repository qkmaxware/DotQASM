using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class GroverGenerator : ICircuitGenerator<IOperator<IEnumerable<Qubit>>> {

    private static Operators.Diffusion grovers = new Operators.Diffusion();

    public Circuit Generate(IOperator<IEnumerable<Qubit>> oracle) {
        var circ = new Circuit(); 
        // https://qiskit.org/textbook/ch-algorithms/grover.html

        var N = 4;
        var qubits = (int)Math.Max(Math.Ceiling(Math.Sqrt(N)), 1);
        var qreg = circ.AllocateQubits(qubits);

        // Step 1.
        /* 
            The amplitude amplification procedure starts out in the uniform superposition |s⟩, 
            which is easily constructed from |s⟩=H⊗n|0⟩n
        */
        foreach (var q in qreg) {
            q.H();
        }

        // Repeat Steps 2, 3 roughly sqrt(N) times (min 1)
        for (var repetition = 0; repetition < qubits; repetition++) {

            // Step 2.
            /*
                We apply the oracle reflection Uf to the state |s⟩
            */
            oracle.Invoke(qreg);

            // Step 3.
            /*
                We now apply an additional reflection Us about the state |s⟩ : Us=2|s⟩⟨s|−1. 
                This transformation maps the state to UsUf|s⟩ and completes the transformation
            */
            grovers.Invoke(qreg);

        }

        return circ;
    }
}

}