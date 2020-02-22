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

    /// <summary>
    /// The http client used to interface with the backend
    /// </summary>
    private HttpClient client;

    /// <summary>
    /// Url to the IBM quantum experience api
    /// </summary>
    public string RootApiUrl => @"https://api.quantum-computing.ibm.com/api";

    /// <summary>
    /// Url to the login endpoint
    /// </summary>
    public string LoginApiUrl => RootApiUrl + @"/users/login";
    /// <summary>
    /// Url to the login with api token endpoint
    /// </summary>
    public string TokenLoginApiUrl => RootApiUrl + @"/users/loginWithToken";

    /// <summary>
    /// Url to the jobs endpoint
    /// </summary>
    public string JobsApiUrl => RootApiUrl + @"/jobs/";
    /// <summary>
    /// Url to the job upload endpoint
    /// </summary>
    public string GetJobUploadUrlUpload => JobsApiUrl + @"{0}/jobUploadUrl";

    /// <summary>
    /// Url to the network endpoint
    /// </summary>
    public string NetworkApiUrl => RootApiUrl + @"/network";

    /// <summary>
    /// Url to the devices endpoint
    /// </summary>
    public string DeviceApiUrl => RootApiUrl + @"/backends/{0}";
    /// <summary>
    /// Url to the device defaults endpoint
    /// </summary>
    public string DeviceDefaultsApiUrl => DeviceApiUrl + @"/defaults";
    /// <summary>
    /// Url to the device status endpoint
    /// </summary>
    public string DeviceStatusApiUrl => DeviceApiUrl + "/queue/status";

    /// <summary>
    /// Session representing the current user logged into the api
    /// </summary>
    private IBMSession session = null;
    /// <summary>
    /// Check if the user has authenticated with the API
    /// </summary>
    public bool IsAuthenticated => session != null && session.WasRequestSuccessful;

    /// <summary>
    /// Settings for serializing and deserializing json from the IBM api
    /// </summary>
    private JsonSerializerOptions json_settings;

    /// <summary>
    /// Create a new un authenticated instance to the IBM API
    /// </summary>
    public IBMApi() {
        this.client = new HttpClient();

        this.json_settings = new JsonSerializerOptions();
        this.json_settings.Converters.Add(new JsonStringEnumConverter());
        this.json_settings.IgnoreNullValues = true;
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

    /// <summary>
    /// Send a get request
    /// </summary>
    /// <param name="url">endpoint</param>
    /// <returns>response string</returns>
    private string Get(string url) {
        var task = client.GetAsync(url);
        task.Wait();

        var content = task.Result.Content.ReadAsStringAsync();
        content.Wait();

        return (content.Result);
    }

    /// <summary>
    /// Send a get request with content
    /// </summary>
    /// <param name="url">endpoint</param>
    /// <param name="body">content</param>
    /// <returns>response string</returns>
    private string Get(string url, HttpContent body) {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Content = body;

        var task = client.SendAsync(req);
        task.Wait();

        var content = task.Result.Content.ReadAsStringAsync();
        content.Wait();

        return (content.Result);
    }

    /// <summary>
    /// Send a post request with content
    /// </summary>
    /// <param name="url">endpoint</param>
    /// <param name="body">content</param>
    /// <returns>response string</returns>
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
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(TokenLoginApiUrl, json);
        session = JsonSerializer.Deserialize<IBMSession>(response, json_settings);
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
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(TokenLoginApiUrl, json);
        session = JsonSerializer.Deserialize<IBMSession>(response, json_settings);
    }

    /// <summary>
    /// Check if authenticated and if not, throw an access violation exception
    /// </summary>
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
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(string.Format(TokenLoginApiUrl, deviceName), json);
        return JsonSerializer.Deserialize<IBMDevice>(response, json_settings);
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
        var response = Get(string.Format(DeviceStatusApiUrl, deviceName));
        return JsonSerializer.Deserialize<IBMDeviceStatus>(response, json_settings);
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
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(JobsApiUrl, json);
        return JsonSerializer.Deserialize<IBMApiJob[]>(response, json_settings);
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
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(JobsApiUrl + WebUtility.UrlEncode(jobId), json);
        return JsonSerializer.Deserialize<IBMApiJob>(response, json_settings);
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

    /// <summary>
    /// Cancel a running job
    /// </summary>
    /// <param name="jobId">unique job identifier</param>
    public void CancelJob(string jobId) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Submit a new quantum object for processing with the IBM quantum experience
    /// </summary>
    /// <param name="backendName">backend to run on</param>
    /// <param name="jobName">name of the job</param>
    /// <param name="obj">the quantum object</param>
    /// <param name="shots">the number of times to repeat the experiment</param>
    /// <returns>Reference to the submitted job</returns>
    public IBMApiJob SubmitJob (string backendName, string jobName, IBMQObj obj, int shots = 1024) {
        ForceAuth();
        
        // Create data-structure IBM expects
        var data = new {
            name = jobName,
            qObject = obj,
            backend = new {
                name = backendName
            },
            shots = shots,
            access_token = session.id
        };
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Post(JobsApiUrl, json);
        var job = JsonSerializer.Deserialize<IBMApiJob>(response, json_settings); // job.id;
        
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
        var json = new StringContent(JsonSerializer.Serialize(data, json_settings), Encoding.UTF8, "application/json");

        // Send and decode
        var response = Get(JobsApiUrl, json);
        return JsonSerializer.Deserialize<IBMNetwork[]>(response, json_settings);
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