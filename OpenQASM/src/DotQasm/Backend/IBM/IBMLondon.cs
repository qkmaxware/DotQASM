namespace DotQasm.Backend.IBM {

public class IBMLondon : IBMBackend {

    public override string BackendName => "ibmq_london";

    public IBMLondon(string key): base(key) {}

}

}