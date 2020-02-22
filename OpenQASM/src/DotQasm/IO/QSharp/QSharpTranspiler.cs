using System.Text;
using DotQasm.IO.OpenQasm.Ast;

namespace DotQasm.IO.QSharp {

public class QSharpTranspiler : IConverter<ProgramContext, string> {
    public string Convert(ProgramContext program) {
        StringBuilder sb = new StringBuilder();

        sb.Append("// Code generated from DotQasm");
        sb.Append("namespace Qasm {");

        foreach (var statement in program.Statements) {
            
        }

        sb.Append("}");

        return sb.ToString();
    }
}

}