using System.Threading.Tasks;

namespace DotQasm.Backend {

public class IBMqx4 : IBMBackend {

    public override string BackendName => "ibmqx4";

    public IBMqx4(string key): base(key) {}

}

}