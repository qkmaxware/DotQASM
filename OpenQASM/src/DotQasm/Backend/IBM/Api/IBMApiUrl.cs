using System;

namespace DotQasm.Backend.IBM.Api {

/// <summary>
/// Returned URLs as defined by IBM's API
/// </summary>
public class IBMApiUrl {
    public string url;
    public Uri GetUri() {
        return new Uri(url);
    }
}

}