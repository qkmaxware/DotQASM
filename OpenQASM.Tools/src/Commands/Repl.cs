using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("repl", HelpText="Interactive quantum circuit Read-Evaluate-Print-Loop")]
public class Repl : ICommand {
    [Option('q', "qubits", Default=5, HelpText="Number of qubits")]
    public int Qubits {get; set;}

    public Status Exec() {
        Console.WriteLine(
@"OpenQASM interactive console
type 'exit' to quit, 'help' for more information"
        );
        Console.WriteLine();

        while (true) {
            Console.Write("|0> ");
            string input = Console.ReadLine();
            if (input == "exit") {
                break;
            } else if (input == "help") {
                // Write header
                int col1 = 32; int col2 = 50; int col3 = 24;
                string format = "| {0,-"+col1+"} | {1,-"+col2+"} | {2,-"+col3+"} |";
                Console.WriteLine(new string('-', col1 + col2 + col3 + 10));
                Console.WriteLine(
                    string.Format(format, "Statement" , "Description", "Example")
                );
                Console.WriteLine(new string('-', col1 + col2 + col3 + 10));

                // Write each statement that I can use
                Console.WriteLine(string.Format(format, "qreg name[size];" , "Declare a named register of qubits", "qreg q[5];"));
                Console.WriteLine(string.Format(format, "creg name[size];" , "Declare a named register of bits", "creg c[5];"));
                Console.WriteLine(string.Format(format, "gate name(params) qargs" , "Declare a unitary gate", "gate id a {U(0,0,0) a;}"));
                Console.WriteLine(string.Format(format, "opaque name(params) qargs;" , "Declare an opaque gate", "opaque x a"));
                Console.WriteLine(string.Format(format, "U(theta,phi,lambda) qubit|qreg;" , "Apply built-in single qubit gate(s)", "U(pi/2,2*pi/3,0) q[0];"));
                Console.WriteLine(string.Format(format, "CX qubit|qreg,qubit|qreg;" , "Apply built-in CNOT gate(s)", "CX q[0],q[1];"));
                Console.WriteLine(string.Format(format, "measure qubit|qreg -> bit|creg;" , "Make measurement(s) in Z basis", "measure q -> c;"));
                Console.WriteLine(string.Format(format, "reset qubit|qreg;" , "Prepare qubit(s) in |0>", "reset q[0];"));
                Console.WriteLine(string.Format(format, "gatename(params) qargs;" , "Apply a user-defined unitary gate", "crz(pi/2) q[1],q[0];"));
                Console.WriteLine(string.Format(format, "if(creg==int) qop;" , "Conditionally apply quantum operation", "if(c==5) CX q[0],q[1];"));
                continue;
            } else {
                try{
                    // Lex
                    List<Token> tokens = null;
                    using (StringReader reader = new StringReader(input)) {
                        tokens = Lexer.Tokenize(reader);
                    }

                    // Parse
                    IO.OpenQasm.Parser p = new IO.OpenQasm.Parser(tokens);
                    var stmt = p.ParseStatement();

                    // Execute
                } catch (OpenQasmException ex) {
                    Console.WriteLine(ex.Format("command.qasm", input));
                    continue;
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                    continue;
                }
            }
        }
        return Status.Failure;
    }
}

}