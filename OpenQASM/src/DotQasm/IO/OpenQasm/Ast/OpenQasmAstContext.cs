namespace DotQasm.IO.OpenQasm.Ast {

public class OpenQasmAstContext {
    public int Position {get; private set;}

    public OpenQasmAstContext(int position) {
        this.Position = position;
    }
}

}