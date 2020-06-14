using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class Toffoli : BaseOperator<(Qubit a, Qubit b, IEnumerable<Qubit> register)> {
    /// <summary>
    /// Invoke the controlled operator on the given qubits
    /// </summary>
    /// <param name="controls">tuple of control qubits</param>
    /// <param name="register">target qubit(s)</param>
    public override void Invoke((Qubit a, Qubit b, IEnumerable<Qubit> register) args) {
        /*
        gate ccx a,b,c { 
        */
        foreach (var c in args.register) {
            c.Apply(Gate.Hadamard);                                                                 // h c;
            args.b.CX(c); c.Apply(Tdg.TdgGate);                                                 // cx b,c; tdg c;
            args.a.CX(c); c.Apply(T.TGate);                                                     // cx a,c; t c;
            args.b.CX(c); c.Apply(Tdg.TdgGate);                                                 // cx b,c; tdg c;
            args.a.CX(c); args.b.Apply(T.TGate); c.Apply(T.TGate); c.Apply(Gate.Hadamard);  // cx a,c; t b; t c; h c;
            args.a.CX(args.b); args.a.Apply(T.TGate); args.b.Apply(Tdg.TdgGate);    // cx a,b; t a; tdg b;
            args.a.CX(args.b);                                                              // cx a,b;
        }
        /*
        }
        */
    }
}

}