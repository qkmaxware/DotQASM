namespace DotQasm.IO.OpenQasm {

public class OpenQasmIncludeException : OpenQasmException {
    public OpenQasmIncludeException(int position, string filename) 
    : base (position, string.Format("Included file '{0}' not found", filename)) {}
}

}