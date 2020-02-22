using System.Collections.Generic;

namespace DotQasm.Backend {

/// <summary>
/// Backend information
/// </summary>
public struct BackendInformation {
    /// <summary>
    /// Name of the backend
    /// </summary>
    public string Name;
    /// <summary>
    /// Description of the backend
    /// </summary>
    public string Description;
}

/// <summary>
/// Interface representing providers that can create backends
/// </summary>
public interface IBackendProvider {
    /// <summary>
    /// Provider name
    /// </summary>
    string ProviderName {get;}
    /// <summary>
    /// Provider abbreviation
    /// </summary>
    string ProviderAbbreviation {get;}
    /// <summary>
    /// Create a backend with the given restrictions
    /// </summary>
    /// <param name="deviceName">device name</param>
    /// <param name="minQubits">minimum number of qubits required</param>
    /// <param name="apikey">the api key</param>
    /// <returns>Backend</returns>
    IBackend CreateBackendInterface(string deviceName, int minQubits, string apikey);
    /// <summary>
    /// List all available backend information
    /// </summary>
    /// <returns>list of backends this provider provides</returns>
    IEnumerable<BackendInformation> ListBackends();
}

}