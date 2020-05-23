using System;
using System.IO;
using DotQasm.IO;
using DotQasm.IO.OpenQasm;

namespace DotQasm.Tools {

public interface ICommand {
    Status Exec();
}

public abstract class BaseCommand : ICommand {
    public abstract Status Exec();
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