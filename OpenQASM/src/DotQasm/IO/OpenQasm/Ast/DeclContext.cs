namespace DotQasm.IO.OpenQasm.Ast {

public enum DeclType {
    Quantum, Classical
}

public class DeclContext: StatementContext {

    public string VariableName {get; private set;}
    public DeclType Type {get; private set;}
    public int Amount {get; private set;}

    public DeclContext(DeclType type, string name, int amount) {
        this.VariableName = name;
        this.Type = type;
        this.Amount = amount;
    }

}

}