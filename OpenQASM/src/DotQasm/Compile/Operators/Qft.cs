using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class Qft : BaseOperator<IEnumerable<Qubit>> {

    private static float RmTheta(int m) {
        return (float)(
            ( 2 * Math.PI ) 
            / Math.Pow(2, m)
        );
    }

    private static void cx(Qubit a, Qubit b) {
        a.CX(b);
    }

    private static void u1(float theta, Qubit a) {
        a.U1(theta);
    }

    private static void ControlledPhaseRotation(float theta, Qubit a, Qubit b) {
        /*
        gate cu1(lambda) a,b {
            u1(lambda/2) a;
            cx a,b;
            u1(-lambda/2) b;
            cx a,b;
            u1(lambda/2) b;
        }
        */
        var theta_2 = theta / 2;

        u1(theta_2, a);
        cx(a, b);
        u1(-theta_2, b);
        cx (a, b);
        u1(theta_2, b);
    }

    public override void Invoke(IEnumerable<Qubit> register) {
        // https://en.wikipedia.org/wiki/Quantum_Fourier_transform
        
        var qubits = register.ToList();
        for (var i = 0; i < qubits.Count; i++) {
            // Apply H
            var qubit = qubits[i];
            qubit.H();

            // Apply phase gates controlled by all the "next" qubits
            var m = 2;
            for (var j = i + 1; j < qubits.Count; j++) {
                var control = qubits[j];
                var target = qubit;
                ControlledPhaseRotation(RmTheta(m++), control, target);
            }
        }
    }
}

}