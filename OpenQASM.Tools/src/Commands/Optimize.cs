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
using DotQasm.Hardware;
using CommandLine.Text;

namespace DotQasm.Tools.Commands {

/// <summary>
/// Command to apply optimization algorithms to OpenQASM programs
/// </summary>
[Verb("optimize", HelpText="Optimize an OpenQASM file")]
public class Optimize : ICommand {

    [Value(0, MetaName = "input file", Required=true, HelpText = "Input OpenQASM file path")]
    public string QasmFile {get; set;}

    [Option('e', "emit", HelpText="File path to emit optimized OpenQASM to", Default=null)]
    public string OutputQasmFile {get; set;}

    [Option('o', "optimization", Required = false, HelpText = "Apply optimization strategy(s) ';' separated", Separator=';')]
    public IEnumerable<string> Optimizations { get; set; }

    [Option('h', "hardware-config", HelpText = "Specify a hardware configuration to optimize against")]
    public string HardwareConfiguration {get; set;}

    [Usage()]
    public static IEnumerable<Example> Examples {
        get {
            foreach (var optimization in AvailableOptimizations) {
                yield return new Example(
                    "Apply \"" + optimization.Name + "\" optimization", 
                    new Optimize{ 
                        QasmFile = "./file.qasm", 
                        Optimizations = new string[]{ optimization.Name } 
                    }
                );
            }
        }
    }

    private static List<IOptimization<LinearSchedule, LinearSchedule>> optimizationList = new List<IOptimization<LinearSchedule, LinearSchedule>>(){
        new Optimization.Strategies.CombineGates(),
        new Optimization.Strategies.HardwareScheduling(),
        new Optimization.Strategies.SplitCombinedOperations(),
        new Optimization.Strategies.SwapDecompose(),
    };

    public static IEnumerable<IOptimization<LinearSchedule, LinearSchedule>> AvailableOptimizations => optimizationList.AsReadOnly();

    private YamlDotNet.Serialization.Deserializer yamlParser = (new YamlDotNet.Serialization.DeserializerBuilder())
        .IgnoreUnmatchedProperties()
        .Build();
    private T ParseYaml<T>(string path) {
        if (path != null) {
            return yamlParser.Deserialize<T>(File.ReadAllText(path));
        } else {
            return default(T);
        }
    }

    public Status Exec() {
        // Read source file
        PhysicalFile SourceQasmFile = new PhysicalFile(QasmFile);
        if (!SourceQasmFile.Exists()) {
            Console.WriteLine(string.Format("File `{0}` does not exist", SourceQasmFile.PhysicalPath));
            return Status.Failure;

        }
        Circuit circuit = null; // By default schedule is a LinearSchedule
        PhysicalDirectory SourceDirectory = new PhysicalDirectory(SourceQasmFile.PhysicalDirectory);
        var source = SourceQasmFile.Contents;
        try {
            circuit = DotQasm.IO.OpenQasm.Parser.ParseCircuit(source, SourceDirectory);
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(SourceQasmFile.PhysicalPath, source));
            return Status.Failure;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }
        circuit.Name = SourceQasmFile.Name;

        // Get optimizations
        var opts = Optimizations.SelectMany((x) => optimizationList.Where((y) => x == y.Name));

        // Read other parameters
        HardwareConfiguration hw = ParseYaml<HardwareConfiguration>(this.HardwareConfiguration);

        // Run optimizations (print how long each one takes)
        TimeSpan totalTime = TimeSpan.FromSeconds(0);
        var fmt = "{0,-24} {1}";
        foreach (var opt in opts) {
            // Assing necessary values to optimizer
            if (opt is IUsing<HardwareConfiguration> hardwareOpt) {
                hardwareOpt.Use(hw);
            }
            if (opt is IUsing<PhysicalFile> sourceFileOpt) {
                sourceFileOpt.Use(SourceQasmFile);
            }

            // Run the optimizer
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