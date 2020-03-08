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

    [Value(0, MetaName = "input file", Required=true, HelpText = "Input OpenQASM file path")]
    public string QasmFile {get; set;}

    [Value(1, MetaName = "output file", Required=false, HelpText = "Output OpenQASM file path", Default=null)]
    public string OutputQasmFile {get; set;}

    [Option('o', "optimization", Required = false, HelpText = "Apply optimization strategy")]
    public IEnumerable<string> Optimizations { get; set; }

    private static List<IOptimization<LinearSchedule, LinearSchedule>> optimizationList = new List<IOptimization<LinearSchedule, LinearSchedule>>(){
        new Optimization.Strategies.CombineGates(),
        new Optimization.Strategies.HardwareScheduling()
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
        TimeSpan totalTime = TimeSpan.FromSeconds(0);
        var fmt = "{0,-24} {1}";
        foreach (var opt in opts) {
            Stopwatch st = Stopwatch.StartNew();
            circuit.GateSchedule = opt.Transform((LinearSchedule)circuit.GateSchedule);
            var stepTime = st.Elapsed;
            totalTime += stepTime;
            Console.WriteLine(string.Format(fmt ,opt.Name + "...", stepTime));
        }

        if (opts.Count() > 0) {
            Console.WriteLine(new String('-', 42));
            Console.WriteLine(string.Format(fmt, "=", totalTime));
        }

        // Output results to file
        if (OutputQasmFile != null) {
            using (var writer = new StreamWriter(OutputQasmFile)) {
                IO.OpenQasm.OpenQasmEmitter.EmitCircuit(circuit, writer);
            }
            Console.WriteLine();
            Console.WriteLine(string.Format("Wrote optimized contents to '{0}'", OutputQasmFile));
        }

        return Status.Success;
    }

}

}