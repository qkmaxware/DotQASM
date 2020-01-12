using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

using DotQasm.Backend.IBM.Api;

namespace DotQasm.Backend.IBM {

public abstract class IBMBackend : IBackend<IBMJobResults> {

    private IBM.Api.IBMApi api;

    public abstract string BackendName {get;}
    public abstract int QubitCount {get;}

    public IBMBackend(string key) {
        this.api = new IBM.Api.IBMApi(key);
    }

    public bool IsAvailable() {
        return api.GetDeviceStatus(BackendName).IsActive();
    }

    public bool SupportsGate(Gate gate) {
        return gate.Symbol == "CX" || gate.Symbol.StartsWith("U("); // CX and parametric U gate only
    }

    private IBMQObj Convert(Circuit circuit) {
        throw new NotImplementedException();
    }

    public Task<IBMJobResults> Exec(Circuit circuit) {
        return new Task<IBMJobResults>(() => {
            var total = System.Diagnostics.Stopwatch.StartNew();

            var qobj = Convert(circuit);
            //this.api.SubmitQasm(this.BackendName, , qobj, );

            return null;
        });
    }
}

}