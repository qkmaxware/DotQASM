using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public class BarrierContext: StatementContext, ICustomGateOperation {
    public List<ArgumentContext> Arguments {get; private set;}

    public BarrierContext(int position, List<ArgumentContext> args): base(position) {
        this.Arguments = args;
    }
}

}