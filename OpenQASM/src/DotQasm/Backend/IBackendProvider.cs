using System.Collections.Generic;

namespace DotQasm.Backend {

public struct BackendInformation {
    public string Name;
    public string Description;
}

public interface IBackendProvider {
    string ProviderName {get;}
    string ProviderAbbreviation {get;}

    IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey);
    IEnumerable<BackendInformation> ListBackends();
}

}