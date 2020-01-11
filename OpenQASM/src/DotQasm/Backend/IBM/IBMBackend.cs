using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace DotQasm.Backend.IBM {

public abstract class IBMBackend : IBackend<IBMJobResults> {

    private IBM.Api.IBMApi api;

    public abstract string BackendName {get;}

    public IBMBackend(string key) {
        this.api = new IBM.Api.IBMApi(key);
    }

    public bool IsAvailable() {
        return (api.GetDeviceStatus(BackendName).status?.ToLower() ?? string.Empty) == "active";
    }

    public bool SupportsGate(Gate gate) {
        return gate.Symbol == "CX" || gate.Symbol.StartsWith("U("); // CX and parametric U gate only
    }

    public Task<IBMJobResults> Exec(Circuit circuit) {
        return null;
    }
}

}