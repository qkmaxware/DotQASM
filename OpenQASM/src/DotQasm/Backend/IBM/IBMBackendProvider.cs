using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

    public class IBMBackendProvider : IBackendProvider {
        public string ProviderName => "IBM Quantum Experience";
        public string ProviderAbbreviation => "IBM";

        public IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey){
            var deviceLower = deviceName.ToLower();
            return (IBackend)GetBackends(apikey).Where((backend) => backend.BackendName.ToLower() == deviceLower && minQubits <= backend.QubitCount).FirstOrDefault();
        }

        public IEnumerable<string> ListBackends() {
            return GetBackends(string.Empty).Select(backend => backend.BackendName);
        }

        private IEnumerable<IBMBackend> GetBackends(string key) {
            return new IBMBackend[]{
                new IBMArmonk(key),
                new IBMBurlington(key),
                new IBMEssex(key),
                new IBMLondon(key),
                new IBMMelbourne(key),
                new IBMOurense(key),
                new IBMSimulator(key),
                new IBMVigo(key),
                new IBMYorktown(key),
            };
        }
    }

}