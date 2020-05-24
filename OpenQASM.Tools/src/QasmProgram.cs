using System;
using CommandLine;
using DotQasm.Tools.Commands;

namespace DotQasm.Tools {

    /// <summary>
    /// CLI tooling 
    /// </summary>
    class QasmProgram {
        static int Main(string[] args) {
            Parser parser = new Parser(config => {
                config.HelpWriter = Console.Error;
            });
            
            return (int)parser
            .ParseArguments<New, Stat, Repl, Run, Transpile, Optimize, Render>(args)
            .MapResult(
                (New opts)              =>  opts.Exec(),
                (Stat opts)             =>  opts.Exec(),
                (Repl opts)             =>  opts.Exec(),
                (Run opts)              =>  opts.Exec(),
                (Transpile opts)        =>  opts.Exec(),
                (Optimize opts)         =>  opts.Exec(),
                (Render opts)           =>  opts.Exec(),
                errs                    =>  Status.Failure
            );
        }
    }

}
