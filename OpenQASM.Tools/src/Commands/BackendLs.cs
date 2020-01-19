using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CommandLine;
using DotQasm;
using DotQasm.Backend.Local;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("backends", HelpText="List all available backends")]
public class BackendLs : ICommand {

    public Status Exec() {
        int col1 = 64;

        foreach (var provider in Run.Providers) {
            Console.WriteLine(new string('-', col1 + 4));
            Console.WriteLine(string.Format("| {0,-"+col1+"} |", provider.ProviderAbbreviation + " (" + provider.ProviderName + ")"));
            Console.WriteLine(new string('-', col1 + 4));

            foreach (var backend in provider.ListBackends()) {
                Console.WriteLine(backend);
            }

            Console.WriteLine();
        }
        return Status.Success;
    }

}

}