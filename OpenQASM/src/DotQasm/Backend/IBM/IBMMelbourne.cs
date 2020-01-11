namespace DotQasm.Backend.IBM {

public class IBMMelbourne : IBMBackend {

    public override string BackendName => "ibmq_16_melbourne";

    public IBMMelbourne(string key): base(key) {}

}

}