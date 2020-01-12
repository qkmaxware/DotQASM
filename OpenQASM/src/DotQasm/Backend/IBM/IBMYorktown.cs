namespace DotQasm.Backend.IBM {

public class IBMYorktown : IBMBackend {

    public override string BackendName => "ibmqx2";
    public override int QubitCount => 5;
    
    public IBMYorktown(string key): base(key) {}

}

}