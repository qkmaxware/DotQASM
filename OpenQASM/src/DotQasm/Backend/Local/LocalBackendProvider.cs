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