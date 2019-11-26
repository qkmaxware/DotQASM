namespace DotQasm.IO.OpenQasm.Ast {

public class ResetContext: QuantumOperationContext {
    public ArgumentContext Argument {get; private set;}

    public ResetContext(int position, ArgumentContext arg): base(position) {
        this.Argument = arg;
    }
}

}