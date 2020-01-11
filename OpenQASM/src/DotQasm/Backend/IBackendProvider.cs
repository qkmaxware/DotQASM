using System.Collections.Generic;

namespace DotQasm.Backend {

public interface IBackendProvider {
    IEnumerable<IBackend<IBackendResult>> GetBackends(string key);
}

}