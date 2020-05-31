using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

public class DeutschGenerator : ICircuitGenerator<Func<bool, bool>> {
    public Circuit Generate(Func<bool, bool> f) {
        var Circuit = new Circuit("Deutsch Algorithm");
        
        var qreg = Circuit.AllocateQubits(2);
        var cbit = Circuit.AllocateCbit();

        // Set initial state
        qreg[1].X();

        // Enter superposition
        foreach (var qubit in qreg) {
            qubit.H();
        }

        // Apply matrix based on f
        switch ( (f(true), f(false)) ) {
            case (true, false): // Identity
                qreg[0].CX(qreg[1]);
                break;
            case (false, true): // Inversion
                qreg[0].X();
                break;
            case (true, true):  // Constant true
                qreg[0].X();
                qreg[1].X();
                break;
            case (false, false):// Constant false
                // 2x2 Identity, do nothing
                break;
        }
        // ---

        // Obtain results
        qreg[0].H();
        qreg[0].Measure(cbit);
        
        return Circuit;
    }
}

}