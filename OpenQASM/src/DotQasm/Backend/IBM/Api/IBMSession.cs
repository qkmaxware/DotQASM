using System;

namespace DotQasm.Backend.IBM.Api {

public class IBMSession: IBMApiResult {
    public string id {get; set;}
    public int ttl {get; set;}
    public DateTime created {get; set;}
    public string userId {get; set;}
}

}