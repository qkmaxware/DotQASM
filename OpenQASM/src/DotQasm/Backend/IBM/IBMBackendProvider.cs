using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

    /// <summary>
    /// Provider which is able to create IBM bcakends
    /// </summary>
    public class IBMBackendProvider : IBackendProvider {
        /// <summary>
        /// Name of the provider
        /// </summary>
        public string ProviderName => "IBM Quantum Experience";
        /// <summary>
        /// Short abbreviation for the provider
        /// </summary>
        public string ProviderAbbreviation => "IBM";

        /// <summary>
        /// Create a backend with the given restrictions
        /// </summary>
        /// <param name="deviceName">device name</param>
        /// <param name="minQubits">minimum number of qubits required</param>
        /// <param name="apikey">the api key</param>
        /// <returns>Backend</returns>
        public IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey){
            var deviceLower = deviceName?.ToLower() ?? string.Empty;
            return (IBackend)GetBackends(apikey).Where((backend) => backend.BackendName.ToLower() == deviceLower && minQubits <= backend.QubitCount).FirstOrDefault();
        }

        /// <summary>
        /// List all available backend information
        /// </summary>
        /// <returns>list of backends this provider provides</returns>
        public IEnumerable<BackendInformation> ListBackends() {
            return GetBackends(string.Empty)
            .Select(backend => backend.Information);
        }

        /// <summary>
        /// List all available backends
        /// </summary>
        /// <param name="key">the api key if applicable</param>
        /// <returns>list of backends</returns>
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
                new IBMRome(key),
            };
        }
    }

}