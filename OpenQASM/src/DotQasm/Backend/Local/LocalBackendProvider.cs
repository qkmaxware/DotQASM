using System.Collections.Generic;

namespace DotQasm.Backend.Local {

    /// <summary>
    /// Provider for all local machine backends
    /// </summary>
    public class LocalBackendProvider : IBackendProvider {
        /// <summary>
        /// Name of the provider
        /// </summary>
        public string ProviderName => "local";
        /// <summary>
        /// Short abbreviation for the provider
        /// </summary>
        public string ProviderAbbreviation => "local";
        /// <summary>
        /// Create a backend with the given restrictions
        /// </summary>
        /// <param name="deviceName">device name</param>
        /// <param name="minQubits">minimum number of qubits required</param>
        /// <param name="apikey">the api key</param>
        /// <returns>Backend</returns>
        public IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey) {
            return (deviceName.ToLower()) switch {
                "simulator" => (IBackend)new Simulator(minQubits),
                "qx" => (IBackend)new QXSimulatorBackend(),
                _ => null
            };
        }
        /// <summary>
        /// List all available backend information
        /// </summary>
        /// <returns>list of backends this provider provides</returns>
        public IEnumerable<BackendInformation> ListBackends() {
            return new BackendInformation[]{
                new BackendInformation(){
                    Name = "simulator",
                    Description = "Fully connected quantum computer simulator"
                },
                new BackendInformation(){
                    Name = "qx",
                    Description = "Local install of QX quantum computer simulator"
                }
            };
        }
    }

}