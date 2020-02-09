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

[Verb("render", HelpText="Render circuit diagram")]
public class Render : ICommand {

    [Value(0, MetaName="file", Required=true, HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    [Value(1, MetaName="output", Default="circuit.svg", HelpText="SVG output file path")]
    public string SvgPath {get; set;}

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

        var emitter = new DotQasm.IO.Svg.SvgEmitter();

        using (StreamWriter writer = new StreamWriter(SvgPath)) {
            emitter.Emit(circuit, writer);
        }

        Console.WriteLine(string.Format("Rendered circuit to '{0}'", SvgPath));

        return Status.Success;
    }

}

}