using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CommandLine;
using DotQasm;
using DotQasm.Backend.Local;
using DotQasm.IO.OpenQasm;
using DotQasm.IO.OpenQasm.Ast;
using CommandLine.Text;

namespace DotQasm.Tools.Commands {

/// <summary>
/// Command to access interactive REPL terminal
/// </summary>
[Verb("repl", HelpText="Interactive quantum circuit Read-Evaluate-Print-Loop")]
public class Repl : ICommand {
    [Option('q', "qubits", Default=5, HelpText="Number of qubits")]
    public int Qubits {get; set;}

    [Usage()]
    public static IEnumerable<Example> Examples {
        get {
            yield return new Example(
                "Read-evaluate-print loop on 10 qubit simulated machine",
                new Repl {
                    Qubits = 10
                }
            );
        }
    }

    public Status Exec() {
        Console.WriteLine(
@"-----------------------------------------------------------------------
| OpenQASM interactive console                                        |
-----------------------------------------------------------------------
type 'exit' to quit, 'help' for information, 'print' for state info"
        );
        Console.WriteLine();

        Simulator sim = new Simulator(Qubits);
        OpenQasm2CircuitVisitor builder = new OpenQasm2CircuitVisitor();

        // Register default gates (U and CX gates already exist in the OpenQASM spec)
        builder.RegisterGate(Gate.Hadamard);
        builder.RegisterGate(Gate.Identity);
        builder.RegisterGate(Gate.PauliX);
        builder.RegisterGate(Gate.PauliY);
        builder.RegisterGate(Gate.PauliZ);

        while (true) {
            Console.Write("|0> ");
            string input = Console.ReadLine();
            if (input == "exit") {
                return Status.Success;
            } else if (input == "print") {
                var fmt = "{1}|{0}>";
                bool first = true;
                for(int i = 0; i < sim.StateCount; i++) {
                    var value = sim[i];
                    if (!value.Equals(Complex.Zero)) {
                        if (first != true) {
                            Console.Write (" + ");
                        } else {
                            first = false;
                        }
                        Console.Write(string.Format(fmt, Convert.ToString(i, 2).PadLeft(sim.QubitCount, '0'), value));
                    }
                }
                Console.WriteLine();
                continue;
            } else if (input == "help") {
                // Write header
                int col1 = 32; int col2 = 50; int col3 = 24;
                string format = "| {0,-"+col1+"} | {1,-"+col2+"} | {2,-"+col3+"} |";

                // Write each statement that I can use
                Console.WriteLine(new string('-', col1 + col2 + col3 + 10));
                Console.WriteLine(
                    string.Format(format, "Statement" , "Description", "Example")
                );
                Console.WriteLine(new string('-', col1 + col2 + col3 + 10));

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
                Console.WriteLine();
                
                // Write each gate that can be used
                Console.WriteLine(new string('-', col1 + col2 + col3 + 10));
                Console.WriteLine(
                    string.Format(format, "Gate" , "Description", "Example")
                );
                Console.WriteLine(new string('-', col1 + col2 + col3 + 10));

                Console.WriteLine(string.Format(format, "U", "Built-in parametric rotation gate", "U(0,0,0) a;"));
                Console.WriteLine(string.Format(format, "CX", "Built-in CNOT gate", "CX q[0],q[1];"));
                Console.WriteLine(string.Format(format, "i", "Predeclared identity gate", "i a;"));
                Console.WriteLine(string.Format(format, "h", "Predeclared hadamard gate", "h a;"));
                Console.WriteLine(string.Format(format, "x", "Predeclared pauli-x gate", "x a;"));
                Console.WriteLine(string.Format(format, "y", "Predeclared pauli-y gate", "y a;"));
                Console.WriteLine(string.Format(format, "z", "Predeclared pauli-z gate", "z a;"));
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

                    // Verify semantics
                    builder.VisitStatement(stmt);
                    if (builder.Circuit.QubitCount > sim.QubitCount) {
                        // Issue 1
                        throw new Exception("Failed to allocate qubit register, index outside of machine bounds");
                    }
                    if (builder.Circuit.BitCount > sim.RegisterSize) {
                        // Issue 2
                        throw new Exception("Failed to allocate classical register, index outside of machine bounds");
                    }

                    // Execute new events
                    var task = sim.Exec(builder.Circuit);
                    task.RunSynchronously();
                    var result = task.Result;

                    // Clear events for next time so we don't double execute
                    builder.Circuit.GateSchedule.ClearSchedule();
                } catch (OpenQasmException ex) {
                    Console.WriteLine(ex.Format(string.Empty, input));
                    continue;
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                    break;
                }
            }
        }
        return Status.Failure;
    }
}

}