using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO.OpenQasm.Ast;
using DotQasm.IO;
using CommandLine.Text;

namespace DotQasm.Tools.Commands {

/// <summary>
/// Command to convert OpenQASM programs to other quantum frameworks
/// </summary>
[Verb("convert", HelpText="Transpile OpenQASM to another quantum language")]
public class Transpile : BaseCommand {
    [Option('l', "lang", Default="OpenQasm", HelpText="Emitted language specifier")]
    public string Language {get; set;}
    private string outf;
    [Option('o', "out", HelpText = "Converted file output path")]
    public string OutputFile {
        get {
            if (outf != null) {
                return outf;
            } else {
                return Path.GetFileNameWithoutExtension(QasmFile);
            }
        } 
        set {
            outf = value;
        }
    }

    [Value(0, MetaName="file", Required=true, HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    [Usage()]
    public static IEnumerable<Example> Examples {
        get {
            foreach (var transpiler in Transpilers) {
                yield return new Example(
                    "Convert OpenQASM circuit to " + transpiler.FormatName,
                    new Transpile {
                        QasmFile = "./file.qasm",
                        Language = transpiler.FormatName
                    }
                );
            }
        }
    }

    private static List<IFileConverter<Circuit, string>> transpilers = new List<IFileConverter<Circuit, string>>(){
        new IO.QSharp.QSharpTranspiler(),
        new IO.ProjectQ.ProjectQTranspiler(),
        new IO.Qiskit.QiskitTranspiler()
    };
    public static IEnumerable<IFileConverter<Circuit, string>> Transpilers => transpilers.AsReadOnly();

    public override Status Exec(){
        // Get transpiler
        var transpiler = Transpilers.Where((x) => x.FormatName.ToLower() == Language.ToLower() || x.FormatExtension.ToLower() == Language.ToLower()).FirstOrDefault();

        if (transpiler != null) {
            // Convert
            string NameOnly = Path.GetFileName(QasmFile);
            string OutputNameOnly = Path.GetFileName(OutputFile) + "." + transpiler.FormatExtension;
            Console.Write(QasmFile + " -> " + OutputNameOnly + " ... ");

            var circuit = ReadFileAsCircuit(QasmFile);
            
            var result = transpiler.Convert(circuit);
            using (StreamWriter writer = new StreamWriter(OutputNameOnly)) { writer.Write(result); }

            Console.WriteLine("Done");
            return Status.Success;
        } else {
            throw new Exception(string.Format("Language '{0}' is not yet supported", Language));
        }
    }
}

}