using System;
using CommandLine;
using DotQasm.Tools.Commands;

namespace DotQasm.Tools {

    class QasmProgram {
        static int Main(string[] args) {
            return (int)Parser
            .Default
            .ParseArguments<Verify, Repl, Run>(args)
            .MapResult(
                (Verify opts)   => opts.Exec(),
                (Repl opts)     => opts.Exec(),
                (Run opts)      => opts.Exec(),
                errs            => Status.Failure
            );
        }
    }

}
