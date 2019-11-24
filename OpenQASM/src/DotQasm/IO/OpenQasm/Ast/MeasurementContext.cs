namespace DotQasm.IO.OpenQasm.Ast {

public class MeasurementContext: QuantumOperationContext {
    public ArgumentContext QuatumArgument {get; private set;}
    public ArgumentContext ClassicalArgument {get; private set;}

    public MeasurementContext(ArgumentContext quantumArg, ArgumentContext classicalArg) {
        this.QuatumArgument = quantumArg;
        this.ClassicalArgument = classicalArg;
    }
}

}