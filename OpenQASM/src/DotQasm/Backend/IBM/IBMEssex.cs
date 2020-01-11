namespace DotQasm.Backend.IBM {

public class IBMEssex : IBMBackend {

    public override string BackendName => "ibmq_essex";

    public IBMEssex(string key): base(key) {}

}

}