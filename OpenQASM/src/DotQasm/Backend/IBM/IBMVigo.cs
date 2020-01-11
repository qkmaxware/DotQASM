namespace DotQasm.Backend.IBM {

public class IBMVigo : IBMBackend {

    public override string BackendName => "ibmq_vigo";

    public IBMVigo(string key): base(key) {}

}

}