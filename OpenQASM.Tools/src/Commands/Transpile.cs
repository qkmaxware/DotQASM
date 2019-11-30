using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

public enum SupportedLanguages {
    OpenQASM, QSharp, Quil, Scaffold, ProjectQ, QISkit
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
        string NameOnly = Path.GetFileName(QasmFile);
        string OutputNameOnly = Path.GetFileName(OutputFile);
        Console.Write(QasmFile + " -> " + OutputNameOnly + " ... ");

        // TODO transpile

        Console.WriteLine("Saved");
        return Status.Failure;
    }
}

}