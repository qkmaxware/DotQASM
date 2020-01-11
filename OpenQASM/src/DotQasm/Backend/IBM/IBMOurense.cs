namespace DotQasm.Backend.IBM {

public class IBMOurense : IBMBackend {

    public override string BackendName => "ibmq_ourense";

    public IBMOurense(string key): base(key) {}

}

}