using System;

namespace DotQasm.Backend.IBM.Api {
/// <summary>
/// Device status as defined by IBM's API
/// </summary>
public class IBMDeviceStatus {
    public bool state {get; set;}
    public string message {get; set;}
    public string status {get; set;}
    public int lengthQueue {get; set;}
    public string backend_version {get; set;}

    public bool IsActive() {
        return (status?.ToLower() ?? string.Empty) == "active";
    }
}

}