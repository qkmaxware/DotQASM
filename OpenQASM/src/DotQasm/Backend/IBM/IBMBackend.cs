using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace DotQasm.Backend {

public abstract class IBMBackend : IBackend<IBMJobResults> {

    public string ApiUrl => @"https://quantumexperience.ng.bluemix.net/api/";
    public string JobsApiUrl => ApiUrl + "Jobs/";
    private string apiKey;
    public int MaxRetries {get; set;}

    public abstract string BackendName {get;}

    public HttpClient WebClient {get; private set;}

    private class JobId {
        public string execution_id {get; set;}
    }

    private class Job {
        public struct JobParametres {
            public string access_token {get; set;}
            public string deviceRunType {get; set;}
            public bool fromCache {get; set;}
            public int shots {get; set;}
        }

        private string qasm; // not serializable
        private object json; // not serializable
        public object data {
            get {
                return qasm != null ? qasm : json;
            } 
            set {
                if (value is string) {
                    qasm = (string)value;
                    json = null;
                } else {
                    qasm = null;
                    json = value;
                }
            }
        }
        public JobParametres @params {get; set;} 
    }

    public IBMBackend(string apiKey) {
        this.apiKey = apiKey;
        this.MaxRetries = 20;

        this.WebClient = new HttpClient();
    }

    public bool IsAvailable() {
        return true;
    }

    public bool SupportsGate(Gate gate) {
        return gate.Symbol == "CX" || gate.Symbol.StartsWith("U("); // CX and parametric U gate only
    }

    public Task<IBMJobResults> Exec(Circuit circuit) {
        Job j = new Job();
        Job.JobParametres par = new Job.JobParametres();
        par.access_token = this.apiKey;
        par.deviceRunType = this.BackendName;
        par.fromCache = false;
        par.shots = 1024;
        j.@params = par;

        var json = JsonSerializer.Serialize<Job>(j);

        return new Task<IBMJobResults>(() => {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // https://quantumcomputing.stackexchange.com/questions/5173/submitting-jobs-to-ibm-devices-without-python
            // Await the submission of the task
            var submitTask = WebClient.PostAsync(JobsApiUrl, new StringContent(json));
            submitTask.Wait();

            // Get the response id
            var submitContent = submitTask.Result.Content.ReadAsStringAsync();
            submitContent.Wait();
            var jobid = JsonSerializer.Deserialize<JobId>(submitContent.Result);

            // Create a url to view the job status
            var joburl = JobsApiUrl + jobid.execution_id;

            // Await the job to be done (or timeout)
            Func<object> checkStatus = () => {
                //var statusTask = WebClient.GetAsync(joburl);
                //statusTask.Wait();
                return null;
            };

            var retries = MaxRetries;
            var status = checkStatus();
            while (status == null && retries-- > 0) {
                Task.Delay(TimeSpan.FromSeconds(5));
                status = checkStatus();
            }

            // Return the results object or an error
            if (status == null) {
                throw new  System.OperationCanceledException("Operation cancelled as the number of retries was exceeded");
            }
            TimeSpan codeTime = new TimeSpan();
            object data = null;

            return new IBMJobResults(jobid.execution_id, watch.Elapsed, codeTime, data);
        });
    }
}

}