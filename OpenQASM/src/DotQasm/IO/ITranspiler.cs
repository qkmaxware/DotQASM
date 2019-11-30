using System.IO;

namespace DotQasm.IO {

public interface Transpiler<ASTin, ASTout> {
    ASTout Transpile(ASTin program);
}

}