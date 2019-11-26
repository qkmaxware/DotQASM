using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public class OpaqueGateDeclContext: StatementContext {

    public string GateName {get; private set;}
    public List<string> ClassicalArguments {get; private set;}
    public List<string> QuantumArguments {get; private set;} 

    public OpaqueGateDeclContext(int position, string name, List<string> classicalArgs, List<string> quantumArgs): base(position) {
        this.GateName = name;
        this.ClassicalArguments = classicalArgs;
        this.QuantumArguments = quantumArgs;
    }

}

}