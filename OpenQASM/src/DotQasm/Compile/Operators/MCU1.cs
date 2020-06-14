using System.Linq;
using System.Collections.Generic;
using System;

namespace DotQasm.Compile.Operators {

/// <summary>
/// Multi-controlled U1 gate
/// </summary>
public class MCU1 : BaseOperator<(IEnumerable<Qubit> controls, Qubit target)> {
    public double Angle {get; private set;}

    public MCU1 (double angle) {
        this.Angle = angle;
    }

    private static IEnumerable<uint> generateGreyCodes(int bits) {
        // https://www.geeksforgeeks.org/generate-n-bit-gray-codes-set-2/
        if (bits <= 0) {
            yield break;
        }

        // start with 1bit pattern
        int N = 1 << bits;
        for (uint i = 0; i < N; i++) {
            // Generate gray code of 
            // corresponding binary 
            // number of integer i. 
            yield return i ^ (i >> 1);
        }
    }

    public override void Invoke((IEnumerable<Qubit> controls, Qubit target) value) {
        // https://qiskit.org/documentation/locale/de_DE/_modules/qiskit/aqua/circuits/gates/multi_control_u1_gate.html
        // _apply_mcu1
        var controls = value.controls.ToList();
        var target = value.target;

        var n = controls.Count;
        var grey_codes = generateGreyCodes(n).Select(code => Convert.ToString(code, 2));
        var angle = Angle * (1 / (Math.Pow(2, n - 1)));
        var gate = Gate.U1(angle);
        var inverseGate = Gate.U1(-angle);

        string last_pattern = null;
        foreach (var pattern in grey_codes) {
            // Pattern does not contain '1'
            if (!pattern.Contains('1')) 
                continue;
            if (last_pattern == null)
                last_pattern = pattern;

            // Find left most set bit
            var lm_pos = pattern.IndexOf('1');

            // Find changed bit
            var pos = pattern.Zip(last_pattern, (i,j) => (i,j)).Select((zip) => zip.i != zip.j).IndexOf(true);
            if (pos >= 0) {
                if (pos != lm_pos) {
                    controls[pos].CX(controls[lm_pos]);
                } else {
                    var indices = pattern.Select((c, i) => c == '1' ? i : -1).Where((i) => i >= 0); // All indices where the character is '1'
                    foreach (var index in indices) {
                        controls[index].CX(controls[lm_pos]);
                    }
                }
            }

            // Check parity
            var count = pattern.Count(c => c == '1');
            if (count % 2 == 0) {
                // Inverse
                controls[lm_pos].ControlledApply(target, inverseGate);
                /*apply_cu1(circuit, -lam_angle, ctls[lm_pos], tgt)
                if global_phase:
                    circuit.u1(-gp_angle, ctls[lm_pos])*/
            } else {
                controls[lm_pos].ControlledApply(target, gate);
                /*apply_cu1(circuit, lam_angle, ctls[lm_pos], tgt)
                if global_phase:
                    circuit.u1(gp_angle, ctls[lm_pos])*/
            }

            last_pattern = pattern;
        }
    }
}

}