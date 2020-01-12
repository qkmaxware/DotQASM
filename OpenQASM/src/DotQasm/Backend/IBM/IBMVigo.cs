namespace DotQasm.Backend.IBM {

public class IBMVigo : IBMBackend {

    public override string BackendName => "ibmq_vigo";
    public override int QubitCount => 5;
    
    public IBMVigo(string key): base(key) {}

}

}