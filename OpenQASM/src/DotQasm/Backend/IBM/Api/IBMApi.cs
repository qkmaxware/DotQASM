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

/// <summary>
/// Interface with the IBM Q-Experience API
/// </summary>
public class IBMApi {

    private HttpClient client;

    public string RootApiUrl => @"https://api.quantum-computing.ibm.com/api";

    public string LoginApiUrl => RootApiUrl + @"/users/login";
    public string TokenLoginApiUrl => RootApiUrl + @"/users/loginWithToken";

    public string JobsApiUrl => RootApiUrl + @"/jobs/";
    public string GetJobUploadUrlUpload => JobsApiUrl + @"{0}/jobUploadUrl";

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

    /// <summary>
    /// Authenticate with the API using a login token
    /// </summary>
    /// <param name="apiToken">login token</param>
    public void AuthenticateWithToken(string apiToken) {
        // Create data-structure IBM expects
        var data = new {
            apiToken = apiToken
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(TokenLoginApiUrl, json);
        session = JsonSerializer.Deserialize<IBMSession>(response);
    }

    /// <summary>
    /// Authenticate with the API using an email and password
    /// </summary>
    /// <param name="email">login email</param>
    /// <param name="password">login password</param>
    public void AuthenticateWithUsername(string email, string password) { 
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
    /// <summary>
    /// Get the information for a given quantum device
    /// </summary>
    /// <param name="deviceName">name of backend</param>
    /// <returns>Device information structure</returns>
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

    /// <summary>
    /// Get the information for a given quantum device
    /// </summary>
    /// <param name="device">project link to device</param>
    /// <returns>Device information structure</returns>
    public IBMDevice GetDeviceInfo (IBMNetwork.ProjectDevice device) {
        return GetDeviceInfo(device.name);
    }

    // TODO GetDeviceDefaults

    /// <summary>
    /// Get the status of a particular quantum device
    /// </summary>
    /// <param name="deviceName">name of device</param>
    /// <returns>Device status structure</returns>
    public IBMDeviceStatus GetDeviceStatus (string deviceName) {
        // Send and decode
        var response = Get(string.Format(TokenLoginApiUrl, deviceName));
        return JsonSerializer.Deserialize<IBMDeviceStatus>(response);
    }

    /// <summary>
    /// Get the status of a particular quantum device
    /// </summary>
    /// <param name="device">IBM device info</param>
    /// <returns>Device status structure</returns>
    public IBMDeviceStatus GetDeviceStatus (IBMDevice device) {
        return GetDeviceStatus(device.backend_name);
    }

    /// <summary>
    /// Get the status of a particular quantum device
    /// </summary>
    /// <param name="device">project link to device</param>
    /// <returns>Device status structure</returns>
    public IBMDeviceStatus GetDeviceStatus (IBMNetwork.ProjectDevice device) {
        return GetDeviceStatus(device.name);    
    } 

    // ---------------------------------------------------------------------------------------
    // Jobs 
    // ---------------------------------------------------------------------------------------
    /// <summary>
    /// Get all jobs submitted by the logged in user
    /// </summary>
    /// <returns>List of jobs</returns>
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

    /// <summary>
    /// Get the info for a particular job by the job id
    /// </summary>
    /// <param name="jobId">id of the job</param>
    /// <returns>job information</returns>
    public IBMApiJob GetJobInfo(string jobId) {
        ForceAuth();

        // Create data-structure IBM expects
        var data = new {
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(JobsApiUrl + WebUtility.UrlEncode(jobId), json);
        return JsonSerializer.Deserialize<IBMApiJob>(response);
    }

    /// <summary>
    /// Get the updated info for a particular job reference to the old job info
    /// </summary>
    /// <param name="job">the old job strcuture</param>
    /// <returns>job information</returns>
    public IBMApiJob GetJobInfo (IBMApiJob job) {
        return GetJobInfo(job.id);
    }

    /// <summary>
    /// Get the status of a job by the job id
    /// </summary>
    /// <param name="jobId">id of the job</param>
    /// <returns>job information</returns>
    public string GetJobStatus(string jobId) {
        return GetJobInfo(jobId).status;
    }

    public void CancelJob(string jobId) {
        throw new NotImplementedException();
    }

    public IBMApiJob SubmitJob (string backendName, string jobName, IBMQObj obj) {
        ForceAuth();
        
        // Create data-structure IBM expects
        var data = new {
            name = jobName,
            qObject = obj,
            backend = new {
                name = backendName
            },
            allowObjectStorage = true,
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(JobsApiUrl, json);
        var job = JsonSerializer.Deserialize<IBMApiJob>(response); // job.id;
        
        return job;
    }

    // ---------------------------------------------------------------------------------------
    // Networks 
    // ---------------------------------------------------------------------------------------
    /// <summary>
    /// Get all the networks which a user belongs to
    /// </summary>
    /// <returns>list of networks</returns>
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
    /// <summary>
    /// Get all the groups within each network that the user belongs to
    /// </summary>
    /// <returns>list of groups</returns>
    public IEnumerable<IBMNetwork.Group> GetAllGroups() {
        return GetAllNetworks().SelectMany(n => n.groups.Values);
    }
    /// <summary>
    /// Get all the projects within each group that the user belongs to
    /// </summary>
    /// <returns>list of projects</returns>
    public IEnumerable<IBMNetwork.Project> GetAllProjects() {
        return GetAllGroups().SelectMany(g => g.projects.Values);
    }
    /// <summary>
    /// Get all the authorized devices for each project that the user has access to
    /// </summary>
    /// <returns>list of project devices</returns>
    public IEnumerable<IBMNetwork.ProjectDevice> GetAllProjectDevices() {
        return GetAllProjects().SelectMany(p => p.devices.Values);
    }
    /// <summary>
    /// Get all authorized devices for the given user
    /// </summary>
    /// <returns>list of devices</returns>
    public IEnumerable<IBMDevice> GetDevices() {
        return GetAllProjectDevices().Select(pd => GetDeviceInfo(pd));
    }
}

}