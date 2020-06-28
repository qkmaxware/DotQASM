using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class Swap : BaseOperator<(Qubit, Qubit)>, IControllable<(Qubit a, Qubit b)>{
    public Swap() {}

        public IControlledOperator<(Qubit a, Qubit b)> Controlled() {
            return new ControlledSwap();
        }

        public override void Invoke((Qubit, Qubit) args) {
        args.Item2.CX(args.Item1);
        args.Item1.CX(args.Item2);
        args.Item2.CX(args.Item1);
    }
}

public class ControlledSwap : IControlledOperator<(Qubit a, Qubit b)> {
    static DotQasm.Compile.Operators.Toffoli tofolli = new Toffoli();

    public ControlledSwap() {}

    public void Invoke((Qubit control, (Qubit a, Qubit b) register) value){
        value.register.a.CX(value.register.b);
        tofolli.Invoke((value.control, value.register.a, new Qubit[]{ value.register.b }));
        value.register.a.CX(value.register.b);
    }
}

}