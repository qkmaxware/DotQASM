namespace DotQasm.IO.OpenQasm.Ast {

public class ArgumentContext: OpenQasmAstContext {
    public string ArgumentName {get; private set;}
    public int? ArgumentOffset {get; private set;}

    public bool IsArrayMember => ArgumentOffset != null;

    public ArgumentContext(int position, string name) : base(position) {
        this.ArgumentName = name;
        this.ArgumentOffset = null;
    }

    public ArgumentContext(int position, string name, int offset) : base(position) {
        this.ArgumentName = name;
        this.ArgumentOffset = offset;
    }
}

}