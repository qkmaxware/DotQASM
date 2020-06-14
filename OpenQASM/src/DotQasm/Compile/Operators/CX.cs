using System.Linq;
using System.Collections.Generic;
using System;

namespace DotQasm.Compile.Operators {

/// <summary>
/// Controlled not operator
/// </summary>
public class CNot : BaseControlledOperator {
    public CNot() {}

    public override void Invoke((Qubit control, IEnumerable<Qubit> register) args) {
        foreach (var qubit in args.register) {
            args.control.CX(qubit);
        }
    }
}

}