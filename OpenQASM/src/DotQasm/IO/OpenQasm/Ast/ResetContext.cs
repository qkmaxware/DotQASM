namespace DotQasm.IO.OpenQasm.Ast {

public class ResetContext: QuantumOperationContext {
    public ArgumentContext Argument {get; private set;}

    public ResetContext(ArgumentContext arg) {
        this.Argument = arg;
    }
}

}