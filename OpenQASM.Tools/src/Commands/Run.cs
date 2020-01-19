using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using DotQasm.Backend;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools.Commands {

[Verb("run", HelpText="Execute a quantum assembly file")]
public class Run : ICommand {
    [Option('p', "provider", Default="local", HelpText="Backend provider identifier string")]
    public string Provider {get; set;}
    
    [Option('b', "backend", Default="simulator", HelpText="Backend identifier string")]
    public string Backend {get; set;}

    
    [Option('k', "api-key", Default="", HelpText="Api key for backends requiring one")]
    public string ApiKey {get; set;}

    [Value(0, MetaName="file", HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    private static List<IBackendProvider> providers = new List<IBackendProvider>(){
        new Backend.Local.LocalBackendProvider(),
        new Backend.IBM.IBMBackendProvider()
    };

    public Status Exec(){
        string lowerProvider = Provider.ToLower();
        string lowerBackend = Backend.ToLower();

        // Read source file
        if (!File.Exists(QasmFile)) {
            Console.WriteLine(string.Format("File `{0}` does not exist", QasmFile));
            return Status.Failure;

        }
        Circuit circuit = null;
        var source = File.ReadAllText(QasmFile);
        try {
            circuit = DotQasm.IO.OpenQasm.Parser.ParseCircuit(source, Path.GetDirectoryName(QasmFile));
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(QasmFile, source));
            return Status.Failure;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }
        circuit.Name = Path.GetFileNameWithoutExtension(QasmFile);

        // Get provider matching the name given
        var provider = providers.Where(
            (provider) => 
            provider.ProviderName.ToLower() == lowerProvider 
            || provider.ProviderAbbreviation.ToLower() == lowerProvider
        ).FirstOrDefault();
        if (provider == null) {
            Console.Error.WriteLine(string.Format("No provider '{0}'", Provider));
            return Status.Failure;
        }

        // Create backend instance
        var backend = provider.CreateBackendInterface(lowerBackend, circuit.QubitCount, ApiKey);
        if (backend == null) {
            Console.Error.WriteLine(string.Format("No backend '{0}' from provider '{1}'", Backend, Provider));
            return Status.Failure;
        }
        if (!backend.IsAvailable()) {
            Console.Error.WriteLine(string.Format("Backend '{0}' is not available at this time. Please verify the API key if applicable.", Backend));
            return Status.Failure;
        }

        // Execute and get results
        var task = backend.Exec(circuit);
        task.RunSynchronously();
        var results = task.Result;
        
        // Print results to console
        Console.WriteLine(results?.ToString() ?? string.Empty);

        return Status.Success;
    }
}

}