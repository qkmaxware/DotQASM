using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public class UnitaryOperationContext: QuantumOperationContext, ICustomGateOperation {
    public string OperationName {get; private set;}
    public List<double> ClassicalParametres {get; private set;}
    public List<ArgumentContext> QuantumParametres {get; private set;}

    public UnitaryOperationContext(int position, string name, List<double> classicalParams, List<ArgumentContext> quantumParams): base(position) {
        this.OperationName = name;
        this.ClassicalParametres = classicalParams;
        this.QuantumParametres = quantumParams;
    } 
}

}