using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO.OpenQasm.Ast;
using DotQasm.IO;

namespace DotQasm.Tools.Commands {

public enum SupportedLanguages {
    OpenQASM, ProjectQ, QISkit, QSharp, Quil, Scaffold
}

[Verb("convert", HelpText="Transpile OpenQASM to another quantum language")]
public class Transpile : ICommand {
    [Option('l', "lang", Default=SupportedLanguages.OpenQASM, HelpText="Emitted language specifier")]
    public SupportedLanguages Language {get; set;}
    private string outf;
    [Option('o', "out", HelpText = "Converted file output path")]
    public string OutputFile {
        get {
            if (outf != null) {
                return outf;
            } else {
                return QasmFile + "." + Language.ToString().ToLower();
            }
        } 
        set {
            outf = value;
        }
    }

    [Value(0, MetaName="file", HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    public Status Exec(){
        ITranspiler<OpenQasmAstContext, string> transpiler = null;
        switch (Language) {
            case SupportedLanguages.OpenQASM:
                // transpiler = transpiler ?? new IO.OpenQASM.Transpiler();
            case SupportedLanguages.ProjectQ:
                // transpiler = transpiler ?? new IO.ProjectQ.Transpiler();
            case SupportedLanguages.QISkit:
                // transpiler = transpiler ?? new IO.QISkit.Transpiler();
            case SupportedLanguages.QSharp:
                transpiler = transpiler ?? (ITranspiler<OpenQasmAstContext, string>)new IO.QSharp.QSharpTranspiler();
                goto case SupportedLanguages.Scaffold;
            case SupportedLanguages.Quil:
                // transpiler = transpiler ?? new IO.Quil.Transpiler();
            case SupportedLanguages.Scaffold:
                // transpiler = transpiler ?? new IO.Scaffold.Transpiler();
                string NameOnly = Path.GetFileName(QasmFile);
                string OutputNameOnly = Path.GetFileName(OutputFile);
                Console.Write(QasmFile + " -> " + OutputNameOnly + " ... ");

                // var node = ...
                // var content = transpiler?.Transpile(node);
                // using (StreamWriter writer = new StreamWriter(OutputFile)) { writer.Write(content) }

                Console.WriteLine("Saved");
                return Status.Success;
            default:
                throw new Exception(string.Format("Language '{0}' is not yet supported", Language));
        }
    }
}

}