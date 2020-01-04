using System;
using System.Threading.Tasks;

namespace DotQasm.Backend {

public interface IBackend<T> where T:IBackendResult {
    bool IsAvailable();
    bool SupportsGate(Gate gate);
    Task<T> Exec(Circuit circuit);
}

}