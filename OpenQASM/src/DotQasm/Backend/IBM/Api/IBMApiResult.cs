namespace DotQasm.Backend.IBM.Api {

public class IBMApiError: System.Exception {
    // JSON fields
    public int statusCode {get; set;}
    public int code {get; set;}
    public string name {get; set;}
    public string message {get; set;}

    // Override c#'s Message with the JSON message
    public override string Message => message;
}

public class IBMApiResult {
    public IBMApiError error {get; set;}

    public bool WasRequestSuccessful => error == null;
}

}