namespace DotQasm.Backend.IBM {

public class IBMMelbourne : IBMBackend {

    public override string BackendName => "ibmq_16_melbourne";
    public override int QubitCount => 15;

    public IBMMelbourne(string key): base(key) {}

}

}