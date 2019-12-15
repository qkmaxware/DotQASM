using System.IO;

namespace DotQasm.IO {

public interface ITranspiler<ASTin, ASTout> {
    ASTout Transpile(ASTin program);
}

}