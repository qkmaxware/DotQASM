namespace DotQasm.Backend.IBM {

public class IBMArmonk : IBMBackend {

    public override string BackendName => "ibmq_armonk";
    public override int QubitCount => 1;

    public IBMArmonk(string key): base(key) {}

}

}