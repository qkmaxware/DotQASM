using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

    public class IBMBackendProvider : IBackendProvider {
        public IEnumerable<IBackend<IBackendResult>> GetBackends(string key) {
            return new IBackend<IBackendResult>[]{
                (IBackend<IBackendResult>)new IBMArmonk(key),
                (IBackend<IBackendResult>)new IBMBurlington(key),
                (IBackend<IBackendResult>)new IBMEssex(key),
                (IBackend<IBackendResult>)new IBMLondon(key),
                (IBackend<IBackendResult>)new IBMMelbourne(key),
                (IBackend<IBackendResult>)new IBMOurense(key),
                (IBackend<IBackendResult>)new IBMqx4(key),
                (IBackend<IBackendResult>)new IBMqx5(key),
                (IBackend<IBackendResult>)new IBMSimulator(key),
                (IBackend<IBackendResult>)new IBMVigo(key)
            };
        }
    }

}