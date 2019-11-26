namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Base class for OpenQASM syntactic analysis exceptions
/// </summary>
public class OpenQasmSyntaxException: OpenQasmException {
    public OpenQasmSyntaxException(Token at, string msg) : base (at.Position, msg) {}
    public OpenQasmSyntaxException(int position, string msg) : base (position, msg) {}
}

}