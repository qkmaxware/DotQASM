namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Base class for OpenQASM lexicographic analysis exceptions
/// </summary>
public class OpenQasmCharacterException: OpenQasmException {
    public OpenQasmCharacterException(int pos, string msg) : base (pos, msg) {}
}

}