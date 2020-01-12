using System;

namespace DotQasm.Backend.IBM.Api {

/// <summary>
/// An object for storing an API session
/// </summary>
public class IBMSession: IBMApiResult {
    /// <summary>
    /// Session id or access_token
    /// </summary>
    public string id {get; set;}
    /// <summary>
    /// Time to logout
    /// </summary>
    public int ttl {get; set;}
    /// <summary>
    /// Time session was created
    /// </summary>
    public DateTime created {get; set;}
    /// <summary>
    /// Logged in user id
    /// </summary>
    public string userId {get; set;}
}

}