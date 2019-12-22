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
        // Read in text
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

            // Verify compatibility with 'Circuit' object
            OpenQasm2CircuitVisitor builder = new OpenQasm2CircuitVisitor();
            builder.VisitProgram(program);
            OpenQasmSemanticAnalyser semanticAnalyser = builder.Analyser;
            
            // Compute runtime
            var timer = new BasicTimeEstimator();
            TimeSpan? longTime = timer.ShortestTimeBetween(
                builder.Circuit.GateSchedule.First, 
                builder.Circuit.GateSchedule.Last
            );

            // Print analysis results
            int[] widths = new int[]{24,42};
            var fmt ="{0,-"+widths[0]+"} {1,-"+widths[1]+"}";
            Console.WriteLine(string.Format(fmt, "Property", "Value"));
            Console.WriteLine(string.Format(fmt, new string('-', widths[0]), new string('-', widths[1])));
            Console.WriteLine(string.Format(fmt, "QASM Statements", semanticAnalyser.StatementCount));
            Console.WriteLine(string.Format(fmt, "Quantum Bits", semanticAnalyser.QubitCount));
            Console.WriteLine(string.Format(fmt, "Classic Bits", semanticAnalyser.CbitCount));
            Console.WriteLine(string.Format(fmt, "Scheduled Events", builder.Circuit.GateSchedule.EventCount));
            Console.WriteLine(string.Format(fmt, "First Event", builder.Circuit.GateSchedule.First.Current.GetType()));
            Console.WriteLine(string.Format(fmt, "Last Event", builder.Circuit.GateSchedule.Last.Current.GetType()));
            Console.WriteLine(string.Format(fmt, "Gate Uses", semanticAnalyser.GateUseCount));
            Console.WriteLine(string.Format(fmt, "Measurements", semanticAnalyser.MeasurementCount));
            Console.WriteLine(string.Format(fmt, "Resets", semanticAnalyser.ResetCount));
            Console.WriteLine(string.Format(fmt, "Barriers", semanticAnalyser.BarrierCount));
            Console.WriteLine(string.Format(fmt, "Conditionals", semanticAnalyser.ClassicalConditionCount));
            Console.WriteLine(string.Format(fmt, "Est. Time", (longTime.HasValue ? "~" + longTime.Value.Milliseconds + "ns" : "?")));
        
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(filename, contents));
            return Status.Failure;
        
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            Console.Write(ex.StackTrace);
            return Status.Failure;
        }

        return Status.Success;
    }

}

}