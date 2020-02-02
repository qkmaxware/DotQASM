using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using CommandLine;
using DotQasm;
using DotQasm.Backend.Local;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("optimizations", HelpText="List all available optimization strategies")]
public class OptimizationsLs : ICommand {

    public Status Exec() {
        int col1 = 64;
        var fmt = "{0,-"+(col1 >> 1)+"} {1}";
        
        Console.WriteLine(new string('-', col1 + 4));
        Console.WriteLine(string.Format("| {0,-"+col1+"} |", "Optimization Strategies"));
        Console.WriteLine(new string('-', col1 + 4));

        foreach (var optimization in Optimize.AvailableOptimizations) {
            Console.WriteLine(string.Format(fmt, optimization.Name, optimization.Description));
        }

        Console.WriteLine();
        
        return Status.Success;
    }

}

}