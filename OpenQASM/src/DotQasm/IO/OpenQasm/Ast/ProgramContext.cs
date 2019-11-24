using System.Collections.Generic;

namespace DotQasm.IO.OpenQasm.Ast {

public class ProgramContext: OpenQasmAstContext {
    public double Version;
    public readonly List<StatementContext> Statements = new List<StatementContext>();
}

}