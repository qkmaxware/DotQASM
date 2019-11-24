namespace DotQasm.IO.OpenQasm {

public class OpenQasmSyntaxException: OpenQasmException {
    public OpenQasmSyntaxException(Token at, string msg) : base (at.Position, msg) {}
    public OpenQasmSyntaxException(int position, string msg) : base (position, msg) {}
}

}