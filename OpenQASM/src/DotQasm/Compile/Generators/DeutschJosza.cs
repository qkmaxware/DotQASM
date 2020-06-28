using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class DeutschJoszaGenerator : ICircuitGenerator<(int qubits, IOperator<IEnumerable<Qubit>> oracle)> {
    public Circuit Generate((int qubits, IOperator<IEnumerable<Qubit>> oracle) args) {
        // https://qiskit.org/textbook/ch-algorithms/deutsch-josza.html
        var circ = new Circuit($"Deutsch-Josza with {args.qubits} qubits");

        var n = args.qubits; // length of the input string

        var inputs = circ.AllocateQubits(n);
        var ancilla = circ.AllocateQubit();
        var creg = circ.AllocateCbits(n);

        // Put the input qubits into the |+> state 
        foreach (var qubit in inputs)
            qubit.H();

        // Put the ancilla qubit into the |-> state
        ancilla.X();
        ancilla.H();

        // Apply oracle
        args.oracle.Invoke(inputs.Append(ancilla));

        // Clean up
        foreach (var qubit in inputs)
            qubit.H();

        foreach (var (qubit, cbit) in inputs.Zip(creg, (q,c) => (q,c))) {
            qubit.Measure(cbit);
        }

        return circ;
    }
}

}