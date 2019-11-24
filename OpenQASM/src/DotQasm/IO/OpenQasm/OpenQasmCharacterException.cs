namespace DotQasm.IO.OpenQasm {

public class OpenQasmCharacterException: OpenQasmException {
    public OpenQasmCharacterException(int pos, string msg) : base (pos, msg) {}
}

}