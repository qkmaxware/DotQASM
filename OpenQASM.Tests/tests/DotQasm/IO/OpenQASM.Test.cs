using System.IO;
using System.Numerics;
using DotQasm;
using DotQasm.IO.OpenQasm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DotQasm.IO {

[TestClass]
public class OpenQasmTest {
    [TestMethod]
    public void TestParser() {
        string script = 
@"OPENQASM 2.0; 
qreg q[3];
creg c0[1];
creg c1[1];
creg c2[1];
// optional post-rotation for state tomography
gate post q { } 
u3(0.3,0.2,0.1) q[0];
h q[1];
cx q[1],q[2];
barrier q;
cx q[0],q[1];
h q[0];
measure q[0] -> c0[0];
measure q[1] -> c1[0];
if(c0==1) z q[2];
if(c1==1) x q[2];
post q[2];
measure q[2] -> c2[0];";
        using (StringReader reader = new StringReader(script)) {
            Parser p = new Parser(Lexer.Tokenize(reader));
            p.ParseFile();
        }
    }
}

}