using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class Swap : BaseOperator<(Qubit, Qubit)> {
    public Swap() {}

    public override void Invoke((Qubit, Qubit) args) {
        args.Item2.CX(args.Item1);
        args.Item1.CX(args.Item2);
        args.Item2.CX(args.Item1);
    }
}

}