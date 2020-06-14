using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

/// <summary>
/// The Grover Diffusion operator
/// </summary>
public class Diffusion : BaseOperator<IEnumerable<Qubit>> {

    /// <summary>
    /// Invoke the operator 2|s⟩⟨s|−1
    /// </summary>
    /// <param name="qreg">quantum register</param>
    public override void Invoke(IEnumerable<Qubit> qreg) {
        foreach (var q in qreg) {
            q.H();
            q.X();
        }

        // 2|0⟩⟨0|−1, C^n-1 Phase Oracle (C^-1 CNOT gate + 2 hadmard gates)
        qreg.Last().H();
        new MCT().Invoke((qreg.Take(qreg.Count() - 1), qreg.Last()));
        qreg.Last().H();

        foreach (var q in qreg) {
            q.X();
            q.H();
        }
    }
}

}