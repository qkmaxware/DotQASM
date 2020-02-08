using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO;
using DotQasm.Scheduling;
using DotQasm.Optimization;
using DotQasm.IO.OpenQasm;
using System.Diagnostics;

namespace DotQasm.Tools.Commands {

[Verb("optimize", HelpText="Optimize an OpenQASM file")]
public class Optimize : ICommand {

    [Value(0, MetaName = "input file", HelpText = "input OpenQASM file path")]
    public string QasmFile {get; set;}

    [Value(1, MetaName = "output file", HelpText = "output OpenQASM file path")]
    public string OutputQasmFile {get; set;}

    [Option('o', "optimization", Required = false, HelpText = "Apply optimization strategy")]
    public IEnumerable<string> Optimizations { get; set; }

    private static List<IOptimization<LinearSchedule, LinearSchedule>> optimizationList = new List<IOptimization<LinearSchedule, LinearSchedule>>(){
        new Optimization.Strategies.CombineGates()
    };

    public static IEnumerable<IOptimization<LinearSchedule, LinearSchedule>> AvailableOptimizations => optimizationList.AsReadOnly();

    public Status Exec() {
        // Read source file
        if (!File.Exists(QasmFile)) {
            Console.WriteLine(string.Format("File `{0}` does not exist", QasmFile));
            return Status.Failure;

        }
        Circuit circuit = null; // By default schedule is a LinearSchedule
        var source = File.ReadAllText(QasmFile);
        try {
            circuit = DotQasm.IO.OpenQasm.Parser.ParseCircuit(source, new PhysicalDirectory(Path.GetDirectoryName(QasmFile)));
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(QasmFile, source));
            return Status.Failure;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }
        circuit.Name = Path.GetFileNameWithoutExtension(QasmFile);

        // Get optimizations
        var opts = Optimizations.SelectMany((x) => optimizationList.Where((y) => x == y.Name));

        // Run optimizations (print how long each one takes)
        foreach (var opt in opts) {
            Stopwatch st = Stopwatch.StartNew();
            Console.Write(opt.Name + "... ");
            circuit.GateSchedule = opt.Transform((LinearSchedule)circuit.GateSchedule);
            Console.WriteLine(st.Elapsed);
        }

        // Output results to file
        using (var writer = new StreamWriter(OutputQasmFile)) {
            IO.OpenQasm.Emitter.EmitCircuit(circuit, writer);
        }

        return Status.Success;
    }

}

}