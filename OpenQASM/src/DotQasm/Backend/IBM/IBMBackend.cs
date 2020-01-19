using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using DotQasm.Backend.IBM.Api;

namespace DotQasm.Backend.IBM {

public abstract class IBMBackend : IBackend {

    private IBM.Api.IBMApi api;

    public abstract string BackendName {get;}
    public abstract int QubitCount {get;}

    public TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
    public int Retries = 60;

    public abstract IEnumerable<string> SupportedGates {get;}

    public IBMBackend(string key) {
        this.api = new IBM.Api.IBMApi(key);
    }

    public bool IsAvailable() {
        return api.GetDeviceStatus(BackendName).IsActive();
    }

    public bool SupportsGate(Gate gate) {
        return SupportedGates.Contains(gate.Symbol.ToLower());
    }

    public Task<BackendResult> Exec(Circuit circuit) {
        return new Task<BackendResult>(() => {
            var total = System.Diagnostics.Stopwatch.StartNew();

            // Convert circuit to quantum object
            var qobj = Convert(circuit);
            
            // Submit the job
            var job = this.api.SubmitJob(this.BackendName, circuit.Name, qobj);

            // wait for job to complete
            var retriesleft = Retries;
            while (retriesleft > 0 && !job.IsDone()) {
                Thread.Sleep(RetryDelay);
                retriesleft--;
                job = this.api.GetJobInfo(job.id); // Keep fetching
            }

            // process results
            return new IBMJobResults(this, job.id, total.Elapsed, job);
        });
    }

    private IBMQObj Convert(Circuit circuit) {
        throw new NotImplementedException();
    }
}

}