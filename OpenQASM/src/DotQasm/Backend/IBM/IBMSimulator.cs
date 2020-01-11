namespace DotQasm.Backend.IBM {

public class IBMSimulator : IBMBackend {

    public override string BackendName => "ibmq_qasm_simulator";

    public IBMSimulator(string key): base(key) {}

}

}