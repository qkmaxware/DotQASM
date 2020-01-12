namespace DotQasm.Backend.IBM {

public class IBMSimulator : IBMBackend {

    public override string BackendName => "ibmq_qasm_simulator";

    public override int QubitCount => 32;

    public IBMSimulator(string key): base(key) {}

}

}