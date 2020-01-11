namespace DotQasm.Backend.IBM {

public class IBMqx5 : IBMBackend {

    public override string BackendName => "ibmqx5";

    public IBMqx5(string key): base(key) {}

}

}