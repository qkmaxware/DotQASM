namespace DotQasm.Backend.IBM {

public class IBMqx4 : IBMBackend {

    public override string BackendName => "ibmqx4";

    public IBMqx4(string key): base(key) {}

}

}