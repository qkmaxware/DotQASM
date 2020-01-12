namespace DotQasm.Backend.IBM {

public class IBMOurense : IBMBackend {

    public override string BackendName => "ibmq_ourense";
    public override int QubitCount => 5;
    
    public IBMOurense(string key): base(key) {}

}

}