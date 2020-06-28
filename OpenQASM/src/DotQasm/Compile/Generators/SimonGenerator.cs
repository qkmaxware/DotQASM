using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class SimonGenerator : ICircuitGenerator<string> {
    public Circuit Generate(string bitstring) {
        Circuit circ = new Circuit($"Simons Algorithm for {bitstring}");
        var length = bitstring.Length;
        var qubits = 2 * length;
        
        var input = circ.AllocateQubits(length);
        var ancilla = circ.AllocateQubits(length);
        var inputcbits = circ.AllocateCbits(length);
        var ancillacbits = circ.AllocateCbits(length);

        // Apply hadamard gate before query
        foreach (var qubit in input) {
            qubit.H();
        }

        // Apply the oracle
        for (var i = 0; i < length; i++) {
            if (bitstring[i] == '1') {
                for (var j = 0; j < length; j++) {
                    input[i].CX(ancilla[j]);
                }
            }
        }

        // Apply hadamard gate after query
        foreach (var qubit in input) {
            qubit.H();
        }

        // Measure all qubits
        foreach (var (qubit, cbit) in input.Zip(inputcbits, (q,c) => (q,c))) {
            qubit.Measure(cbit);
        }
        foreach (var (qubit, cbit) in ancilla.Zip(ancillacbits, (q,c) => (q,c))) {
            qubit.Measure(cbit);
        }

        return circ;
    }
}

}