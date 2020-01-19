using System.Collections.Generic;

namespace DotQasm.Backend {

public interface IBackendProvider {
    string ProviderName {get;}
    string ProviderAbbreviation {get;}

    IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey);
}

}