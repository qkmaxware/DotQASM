namespace DotQasm.Backend.IBM {

public class IBMLondon : IBMBackend {

    public override string BackendName => "ibmq_london";
    public override int QubitCount => 5;

    public IBMLondon(string key): base(key) {}

}

}