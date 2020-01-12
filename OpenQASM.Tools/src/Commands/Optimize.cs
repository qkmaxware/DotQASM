using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CommandLine;
using DotQasm;
using DotQasm.Backend.Local;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("optimize", HelpText="Optimize an OpenQASM file")]
public class Optimize : ICommand {

    [Value(0, MetaName = "input file", HelpText = "input OpenQASM file path")]
    public string QasmFile {get; set;}

    [Value(0, MetaName = "output file", HelpText = "output OpenQASM file path")]
    public string OutputQasmFile {get; set;}

    [Option('o', "optimization", Required = false, HelpText = "Apply optimization strategy")]
    public IEnumerable<string> Optimizations { get; set; }

    public Status Exec() {

        return Status.Failure;
    }

}

}