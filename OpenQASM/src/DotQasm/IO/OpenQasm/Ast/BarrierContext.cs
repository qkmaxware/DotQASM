using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public class BarrierContext: StatementContext, ICustomGateOperation {
    public List<ArgumentContext> Arguments {get; private set;}

    public BarrierContext(List<ArgumentContext> args) {
        this.Arguments = args;
    }
}

}