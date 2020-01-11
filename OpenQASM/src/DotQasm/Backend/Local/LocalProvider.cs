using System.Collections.Generic;

namespace DotQasm.Backend.Local {

    public class LocalProvider : IBackendProvider {
        public IEnumerable<IBackend<IBackendResult>> GetBackends(string key) {
            return new IBackend<IBackendResult>[]{
                (IBackend<IBackendResult>)new Simulator()
            };
        }
    }

}