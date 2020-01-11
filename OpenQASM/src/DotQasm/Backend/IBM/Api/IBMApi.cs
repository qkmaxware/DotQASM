using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotQasm.Backend.IBM.Api {

public class IBMApi {

    private HttpClient client;

    public string RootApiUrl => @"https://api.quantum-computing.ibm.com/api";

    public string LoginApiUrl => RootApiUrl + @"/users/login";
    public string TokenLoginApiUrl => RootApiUrl + @"/users/loginWithToken";

    public string JobsApiUrl => RootApiUrl + @"/jobs/";

    public string NetworkApiUrl => RootApiUrl + @"/network";

    public string DeviceApiUrl => RootApiUrl + @"/backends/{0}";
    public string DeviceDefaultsApiUrl => DeviceApiUrl + @"/defaults";
    public string DeviceStatusApiUrl => DeviceApiUrl + "/queue/status";

    private IBMSession session = null;
    public bool IsAuthenticated => session != null && session.WasRequestSuccessful;

    /// <summary>
    /// Create a new un authenticated instance to the IBM API
    /// </summary>
    public IBMApi() {
        client = new HttpClient();
    }

    /// <summary>
    /// Create a new authenticated instance to the IBM API using an api token
    /// </summary>
    /// <param name="token">IBM api token</param>
    public IBMApi(string token): this() {
        AuthenticateWithToken(token);
    }

    /// <summary>
    /// Create a new authenticated instance to the IBM API using login credentials
    /// </summary>
    /// <param name="email">IBM login email</param>
    /// <param name="password">IBM login password</param>
    public IBMApi(string email, string password): this(){
        AuthenticateWithUsername(email, password);
    }

    private string Get(string url) {
        var task = client.GetAsync(url);
        task.Wait();

        var content = task.Result.Content.ReadAsStringAsync();
        content.Wait();

        return (content.Result);
    }

    private string Get(string url, HttpContent body) {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Content = body;

        var task = client.SendAsync(req);
        task.Wait();

        var content = task.Result.Content.ReadAsStringAsync();
        content.Wait();

        return (content.Result);
    }

    private string Post(string url, HttpContent body) {
        var task = client.PostAsync(url, body);
        task.Wait();

        var content = task.Result.Content.ReadAsStringAsync();
        content.Wait();

        return (content.Result);
    }

    // ---------------------------------------------------------------------------------------
    // Authentication 
    // ---------------------------------------------------------------------------------------

    private void AuthenticateWithToken(string apiToken) {
        // Create data-structure IBM expects
        var data = new {
            apiToken = apiToken
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(TokenLoginApiUrl, json);
        session = JsonSerializer.Deserialize<IBMSession>(response);
    }

    private void AuthenticateWithUsername(string email, string password) { 
        // Create data-structure IBM expects
        var data = new {
            email = email,
            password = password
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(TokenLoginApiUrl, json);
        session = JsonSerializer.Deserialize<IBMSession>(response);
    }

    private void ForceAuth() {
        if (!IsAuthenticated)
            throw new AccessViolationException("Not logged in");
    }

    // ---------------------------------------------------------------------------------------
    // Backends 
    // ---------------------------------------------------------------------------------------

    public IBMDevice GetDeviceInfo (string deviceName) {
        ForceAuth();

        // Create data-structure IBM expects
        var data = new {
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(string.Format(TokenLoginApiUrl, deviceName), json);
        return JsonSerializer.Deserialize<IBMDevice>(response);
    }

    public IBMDevice GetDeviceInfo (IBMNetwork.ProjectDevice device) {
        return GetDeviceInfo(device.name);
    }

    // TODO GetDeviceDefaults

    public IBMDeviceStatus GetDeviceStatus (string deviceName) {
        // Send and decode
        var response = Get(string.Format(TokenLoginApiUrl, deviceName));
        return JsonSerializer.Deserialize<IBMDeviceStatus>(response);
    }

    public IBMDeviceStatus GetDeviceStatus (IBMDevice device) {
        return GetDeviceStatus(device.backend_name);
    }

    public IBMDeviceStatus GetDeviceStatus (IBMNetwork.ProjectDevice device) {
        return GetDeviceStatus(device.name);    
    } 

    // ---------------------------------------------------------------------------------------
    // Jobs 
    // ---------------------------------------------------------------------------------------

    public IBMApiJob[] GetSubmittedJobs() {
        ForceAuth();

        // Create data-structure IBM expects
        var data = new {
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(JobsApiUrl, json);
        return JsonSerializer.Deserialize<IBMApiJob[]>(response);
    }

    public void SubmitQasm (string backendName, string jobName, string qasm, int shots=1024) {
        ForceAuth();

        // Create data-structure IBM expects
        var data = new {
            name = jobName,
            qObject = new {

            },
            backend = new {
                name = backendName
            },
            shots = shots,
            allowObjectStorage = true,
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(JobsApiUrl, json);
    }

    // ---------------------------------------------------------------------------------------
    // Networks 
    // ---------------------------------------------------------------------------------------

    public IEnumerable<IBMNetwork> GetAllNetworks() {
        ForceAuth();

        // Create data-structure IBM expects
        var data = new {
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(JobsApiUrl, json);
        return JsonSerializer.Deserialize<IBMNetwork[]>(response);
    }

    public IEnumerable<IBMNetwork.Group> GetAllGroups() {
        return GetAllNetworks().SelectMany(n => n.groups.Values);
    }

    public IEnumerable<IBMNetwork.Project> GetAllProjects() {
        return GetAllGroups().SelectMany(g => g.projects.Values);
    }

    public IEnumerable<IBMNetwork.ProjectDevice> GetAllProjectDevices() {
        return GetAllProjects().SelectMany(p => p.devices.Values);
    }

    public IEnumerable<IBMDevice> GetDevices() {
        return GetAllProjectDevices().Select(pd => GetDeviceInfo(pd));
    }
}

}