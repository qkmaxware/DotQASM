namespace DotQasm.IO.OpenQasm.Ast {

public class IfContext: StatementContext {
    public string ClassicalVariableName {get; private set;}
    public int ClassicalVariableValue {get; private set;}
    public QuantumOperationContext Operation {get; private set;}

    public IfContext(string var, int value, QuantumOperationContext stmt) {
        this.ClassicalVariableName = var;
        this.ClassicalVariableValue = value;
        this.Operation = stmt;
    }
}

}