using System.Collections.Generic;

namespace DotQasm.Backend.Local {

    public class LocalBackendProvider : IBackendProvider {

        public string ProviderName => "local";
        public string ProviderAbbreviation => "local";

        public IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey) {
            return (deviceName.ToLower()) switch {
                "simulator" => (IBackend)new Simulator(minQubits),
                "qx" => (IBackend)new QXSimulatorBackend(),
                _ => null
            };
        }

        public IEnumerable<string> ListBackends() {
            return new string[]{
                "simulator",
                "qx"
            };
        }
    }

}