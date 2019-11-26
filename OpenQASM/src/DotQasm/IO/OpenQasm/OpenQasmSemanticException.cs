using DotQasm.IO.OpenQasm.Ast;

namespace DotQasm.IO.OpenQasm {

/// <summary>
/// Base class for OpenQASM semantic analysis exceptions
/// </summary>
public class OpenQasmSemanticException: OpenQasmException {
    public OpenQasmSemanticException(OpenQasmAstContext ctx, string msg) : base (ctx.Position, msg) {}
    public OpenQasmSemanticException(int position, string msg) : base (position, msg) {}
}

}