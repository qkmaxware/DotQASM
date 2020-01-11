namespace DotQasm.Backend.IBM {

public class IBMBurlington : IBMBackend {

    public override string BackendName => "ibmq_burlington";

    public IBMBurlington(string key): base(key) {}

}

}