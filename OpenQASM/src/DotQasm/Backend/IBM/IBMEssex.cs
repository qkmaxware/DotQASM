namespace DotQasm.Backend.IBM {

public class IBMEssex : IBMBackend {

    public override string BackendName => "ibmq_essex";
    public override int QubitCount => 5;

    public IBMEssex(string key): base(key) {}

}

}