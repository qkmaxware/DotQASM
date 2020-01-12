namespace DotQasm.Backend.IBM {

public class IBMBurlington : IBMBackend {

    public override string BackendName => "ibmq_burlington";
    public override int QubitCount => 5;

    public IBMBurlington(string key): base(key) {}

}

}