using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class Toffoli {
    /// <summary>
    /// Invoke the controlled operator on the given qubits
    /// </summary>
    /// <param name="controls">tuple of control qubits</param>
    /// <param name="register">target qubit(s)</param>
    void Invoke((Qubit a, Qubit b) controls, IEnumerable<Qubit> register) {
        /*
        gate ccx a,b,c { 
        */
        foreach (var c in register) {
            c.Apply(Gate.Hadamard);                                                                 // h c;
            controls.b.CX(c); c.Apply(Tdg.TdgGate);                                                 // cx b,c; tdg c;
            controls.a.CX(c); c.Apply(T.TGate);                                                     // cx a,c; t c;
            controls.b.CX(c); c.Apply(Tdg.TdgGate);                                                 // cx b,c; tdg c;
            controls.a.CX(c); controls.b.Apply(T.TGate); c.Apply(T.TGate); c.Apply(Gate.Hadamard);  // cx a,c; t b; t c; h c;
            controls.a.CX(controls.b); controls.a.Apply(T.TGate); controls.b.Apply(Tdg.TdgGate);    // cx a,b; t a; tdg b;
            controls.a.CX(controls.b);                                                              // cx a,b;
        }
        /*
        }
        */
    }
}

}