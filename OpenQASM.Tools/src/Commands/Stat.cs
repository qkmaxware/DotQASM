using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;

using DotQasm;
using DotQasm.IO.OpenQasm;
using DotQasm.Scheduling;

namespace DotQasm.Tools.Commands {

[Verb("stat", HelpText="")]
public class Stat : ICommand {

    [Value(0, MetaName="file", HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    [Option('o', "optimization", Required = false, HelpText = "Optimizations to apply")]
    public IEnumerable<string> InputFiles { get; set; }

    public Status Exec() {
        string contents = null;
        string filename = null;
        string directory = null;
        string ext = null;
        try {
            contents = File.ReadAllText(QasmFile);
            filename = Path.GetFileName(QasmFile);
            directory = Path.GetDirectoryName(QasmFile);
            ext = Path.GetExtension(QasmFile);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }

        List<Token> tokens;
        try {
            // Verify lexical analysis
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (StringReader reader = new StringReader(contents)) {
                tokens = Lexer.Tokenize(reader);
            }

            // Verify syntatic analysis
            IO.OpenQasm.Parser parser = new IO.OpenQasm.Parser(tokens);
            parser.IncludeSearchPath = directory;

            var program = ext switch {
                ".qasm"=> parser.ParseFile(),           // QASM files must start with QASM
                "qasm" => parser.ParseFile(),           // QASM files must start with QASM
                _ => parser.ParseProgram()              // Non QASM files are treated as *.inc files
            };
            var parsetime = stopwatch.Elapsed;

            // Verify compatibility with 'Circuit' object
            OpenQasm2CircuitVisitor builder = new OpenQasm2CircuitVisitor();
            builder.VisitProgram(program);
            OpenQasmSemanticAnalyser semanticAnalyser = builder.Analyser;
            
            var timer = new BasicTimeEstimator();
            var longTime = timer.LongestTimeBetween(
                builder.Circuit.GateSchedule.First, 
                builder.Circuit.GateSchedule.Last
            );

            var fmt ="{0,-24} {1,-10}";
            Console.WriteLine(string.Format(fmt, "Property", "Value"));
            Console.WriteLine(string.Format(fmt, new string('-', 24), new string('-', 10)));
            Console.WriteLine(string.Format(fmt, "Processing Time", parsetime.Milliseconds + "ms"));
            Console.WriteLine(string.Format(fmt, "Quantum Bits", semanticAnalyser.QubitCount));
            Console.WriteLine(string.Format(fmt, "Classic Bits", semanticAnalyser.CbitCount));
            Console.WriteLine(string.Format(fmt, "Gate Uses", semanticAnalyser.GateUses));
            Console.WriteLine(string.Format(fmt, "Instructions", semanticAnalyser.InstructionCount));
            Console.WriteLine(string.Format(fmt, "Est. Running Time", "~" + ((longTime?.Milliseconds.ToString()) ?? "?") + "ms"));
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(filename, contents));
            return Status.Failure;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }

        return Status.Success;
    }

}

}