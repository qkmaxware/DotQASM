using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using DotQasm.IO;
using DotQasm.Backend;
using DotQasm.IO.OpenQasm;
using CommandLine.Text;

namespace DotQasm.Tools.Commands {

[Verb("run", HelpText="Execute a quantum assembly file")]
public class Run : ICommand {
    [Option('p', "provider", Default="local", HelpText="Backend provider identifier string")]
    public string Provider {get; set;}
    
    [Option('b', "backend", Default="simulator", HelpText="Backend identifier string")]
    public string Backend {get; set;}

    
    [Option('k', "api-key", Default="", HelpText="Api key for backends requiring one")]
    public string ApiKey {get; set;}

    [Value(0, MetaName="file", Required=true, HelpText="OpenQASM file path")]
    public string QasmFile {get; set;}

    private static List<IBackendProvider> providers = new List<IBackendProvider>(){
        new Backend.Local.LocalBackendProvider(),
        new Backend.IBM.IBMBackendProvider()
    };

    public static IEnumerable<IBackendProvider> Providers => providers.AsReadOnly();

    [Usage()]
    public static IEnumerable<Example> Examples {
        get {
            foreach (var provider in Run.Providers) {
                foreach (var backend in provider.ListBackends()) {
                    yield return new Example(
                        "Execute algorithm on the " + provider.ProviderAbbreviation + " " + backend.Name  + " device", 
                        new Run{ 
                            QasmFile = "./file.qasm", 
                            Provider = provider.ProviderAbbreviation,
                            Backend = backend.Name 
                        }
                    );
                }
            }
        }
    }

    public static object GetBackend(string providerName, string backendName, string apikey, int qubits) {
        var lowerProvider = providerName.ToLower();
        var lowerBackend = backendName.ToLower();

        // Get provider matching the name given
        var provider = providers.Where(
            (provider) => 
            provider.ProviderName.ToLower() == lowerProvider 
            || provider.ProviderAbbreviation.ToLower() == lowerProvider
        ).FirstOrDefault();
        if (provider == null) {
            return new Exception(string.Format("No provider '{0}'", providerName));
        }

        // Create backend instance
        var backend = provider.CreateBackendInterface(lowerBackend, qubits, apikey);
        if (backend == null) {
            return new Exception(string.Format("No backend '{0}' from provider '{1}'", backendName, providerName));
        }

        return backend;
    }

    public Status Exec(){
        // Read source file
        if (!File.Exists(QasmFile)) {
            Console.WriteLine(string.Format("File `{0}` does not exist", QasmFile));
            return Status.Failure;

        }
        Circuit circuit = null;
        var source = File.ReadAllText(QasmFile);
        try {
            circuit = DotQasm.IO.OpenQasm.Parser.ParseCircuit(source, new PhysicalDirectory(Path.GetDirectoryName(QasmFile)));
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(QasmFile, source));
            return Status.Failure;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Status.Failure;
        }
        circuit.Name = Path.GetFileNameWithoutExtension(QasmFile);

        var backendOrError = GetBackend(Provider, Backend, ApiKey, circuit.QubitCount);
        if (backendOrError is Exception) {
            Console.Error.WriteLine(((Exception)backendOrError).Message);
            return Status.Failure;
        }
        
        var backend = (IBackend)backendOrError;
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