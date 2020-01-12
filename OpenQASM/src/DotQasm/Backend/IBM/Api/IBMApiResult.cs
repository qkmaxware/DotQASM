namespace DotQasm.Backend.IBM.Api {

/// <summary>
/// An error as defined by IBM's API
/// </summary>
public class IBMApiError: System.Exception {
    // JSON fields
    public int statusCode {get; set;}
    public int code {get; set;}
    public string name {get; set;}
    public string message {get; set;}

    // Override c#'s Message with the JSON message
    public override string Message => message;
}

/// <summary>
/// Generic base class for all API results, allows us to check for the existence of errors
/// </summary>
public class IBMApiResult {
    /// <summary>
    /// The error if the api failed
    /// </summary>
    public IBMApiError error {get; set;}

    /// <summary>
    /// True if no error exists
    /// </summary>
    public bool WasRequestSuccessful => error == null;
}

}