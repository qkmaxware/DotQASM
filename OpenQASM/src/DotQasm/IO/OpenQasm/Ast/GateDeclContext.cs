using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public class GateDeclContext: StatementContext {
    public string GateName {get; private set;}
    public List<string> ClassicalArguments {get; private set;}
    public List<string> QuantumArguments {get; private set;} 
    public List<ICustomGateOperation> Operations {get; private set;}

    public GateDeclContext(string name, List<string> classicalArgs, List<string> quantumArgs, List<ICustomGateOperation> operations) {
        this.GateName = name;
        this.ClassicalArguments = classicalArgs;
        this.QuantumArguments = quantumArgs;
        this.Operations = operations;
    }
}

}