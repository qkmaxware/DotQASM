using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;

using DotQasm;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("verify", HelpText="Validate the syntax and semantics of an OpenQASM program file")]
public class Verify : ICommand {
    [Value(0, MetaName="file", HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    public Status Exec() {
        string contents = null;
        string filename = null;
        string directory = null;
        string ext = null;
        try {
            contents = File.ReadAllText(QasmFile);
            filename = Path.GetFileName(QasmFile);
            directory = Path.GetDirectoryName(QasmFile);
            ext = Path.GetExtension(QasmFile);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }

        List<Token> tokens;
        try {
            // Verify lexical analysis
            using (StringReader reader = new StringReader(contents)) {
                tokens = Lexer.Tokenize(reader);
            }

            // Verify syntatic analysis
            IO.OpenQasm.Parser parser = new IO.OpenQasm.Parser(tokens);
            parser.IncludeSearchPath = directory;
            var program = ext switch {
                "qasm" => parser.ParseFile(),   // QASM files must start with QASM
                _ => parser.ParseProgram()      // Non QASM files are treated as *.inc files
            };

            // Verify compatibility with 'Circuit' object

        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(filename, contents));
            return Status.Failure;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }

        return Status.Success;
    }
}

}