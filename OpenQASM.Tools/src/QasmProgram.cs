using System;
using CommandLine;
using DotQasm.Tools.Commands;

namespace DotQasm.Tools {

    class QasmProgram {
        static int Main(string[] args) {
            Parser parser = new Parser(config => {
                config.HelpWriter = Console.Error;
            });
            
            return (int)parser
            .ParseArguments<Verify, Stat, Repl, Run, Transpile, BackendLs>(args)
            .MapResult(
                (Verify opts)       =>  opts.Exec(),
                (Stat opts)         =>  opts.Exec(),
                (Repl opts)         =>  opts.Exec(),
                (Run opts)          =>  opts.Exec(),
                (Transpile opts)    =>  opts.Exec(),
                (Optimize opts)     =>  opts.Exec(),
                (BackendLs opts)    =>  opts.Exec(), 
                errs                =>  Status.Failure
            );
        }
    }

}
