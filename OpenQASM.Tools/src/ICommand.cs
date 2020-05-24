using System;
using System.IO;
using DotQasm.IO;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools {

/// <summary>
/// Generic CLI command interface
/// </summary>
public interface ICommand {
    /// <summary>
    /// Execute CLI command and return a status code
    /// </summary>
    Status Exec();
}

/// <summary>
/// Base CLI command class with shared functionality
/// </summary>
public abstract class BaseCommand : ICommand {
    public abstract Status Exec();
    
    /// <summary>
    /// Read the given file as a quantum circuit
    /// </summary>
    /// <param name="QasmFile">path to qasm file</param>
    /// <returns>parsed circuit</returns>
    public Circuit ReadFileAsCircuit(string QasmFile) {
        Circuit circuit = null;
        var source = File.ReadAllText(QasmFile);
        try {
            circuit = DotQasm.IO.OpenQasm.Parser.ParseCircuit(source, new PhysicalDirectory(Path.GetDirectoryName(QasmFile)));
        } catch (OpenQasmException ex) {
            Console.WriteLine(ex.Format(QasmFile, source));
            throw;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            throw;
        }
        return circuit;
    }
}

}