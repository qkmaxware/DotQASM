namespace DotQasm.IO.OpenQasm.Ast {

public class ArgumentContext: OpenQasmAstContext {
    public string ArgumentName {get; private set;}
    public int? ArgumentOffset {get; private set;}

    public ArgumentContext(string name) {
        this.ArgumentName = name;
        this.ArgumentOffset = null;
    }

    public ArgumentContext(string name, int offset) {
        this.ArgumentName = name;
        this.ArgumentOffset = offset;
    }
}

}