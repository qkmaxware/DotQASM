using System.Threading.Tasks;

namespace DotQasm.Backend {

public class IBMqx2 : IBackend {
    public bool IsAvailable() {
        // http hit is yes
    }

    public bool SupportsGate(Gate gate) {
        // u1, u2, u3, cx, id
    }

    public Task Exec(Circuit circuit) {
        // compile and execute
    }
}

}