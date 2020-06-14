using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class DeutschJozsaGenerator : ICircuitGenerator<Func<List<bool>, bool>> {
    public Circuit Generate(Func<List<bool>, bool> fn) {
        
        var length = 5; 

        var Circuit = new Circuit("Deutsch-Jozsa");
        var qreg = Circuit.AllocateQubits(length); 
        var ancilla = Circuit.AllocateQubit();
        var creg = Circuit.AllocateCbits(length);

        // Set initial state
        ancilla.X();

        // Create superposition
        foreach (var q in qreg) {
            q.H();
        }
        ancilla.H();

        // Construct and apply oracle

        // Uncompute ancilla
        ancilla.H();
        ancilla.X();

        // Obtain results
        foreach (var (q, c) in qreg.Zip(creg, (q,c) => (q, c))) {
            q.H();
            q.Measure(c);
        }

        return Circuit;
    }
}

}