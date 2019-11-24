using System;
using System.Threading.Tasks;

namespace DotQasm.Backend {

public interface IBackend {
    bool IsAvailable();
    bool SupportsGate(Gate gate);
    Task Exec(Circuit circuit);
}

}