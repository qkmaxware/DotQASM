using System.Diagnostics;
using System.Threading.Tasks;

namespace DotQasm.Backend.QX {

public class QXSimulatorBackend : IBackend<IBackendResult> {
    public bool IsAvailable() {
        Process p = new Process();
        p.StartInfo.FileName = "qx_simulator --version";
        p.Start();
        p.WaitForExit();

        return p.ExitCode == 0;
    }

    public bool SupportsGate(Gate gate) {
        return true;
    }

    public Task<IBackendResult> Exec(Circuit circuit) {
        throw new System.NotImplementedException();
    }
}

}