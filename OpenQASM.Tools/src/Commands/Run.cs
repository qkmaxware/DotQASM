using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("run", HelpText="Execute a quantum assembly file")]
public class Run : ICommand {
    [Option('b', "backend", Default="simulator", HelpText="Backend identifier string")]
    public string Backend {get; set;}

    [Value(0, MetaName="file", HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    public Status Exec(){
        return Status.Failure;
    }
}

}