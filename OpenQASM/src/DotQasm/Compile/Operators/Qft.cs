using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Operators {

public class Qft : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>> {

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

    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return new QftDg();
    }
}

public class QftDg : BaseOperator<IEnumerable<Qubit>>, IAdjoint<IEnumerable<Qubit>> {
    public override void Invoke(IEnumerable<Qubit> register) {
        var qubits = register.ToList();

        for (var i = 0; i < qubits.Count/2; i++) {
            // Half of the qubits
            var qubit = qubits[i];
            qubit.Swap(qubits[qubits.Count - i - 1]);
        }
        for (var j = 0; j < qubits.Count; j++) {
            for (var m = 0; m < j; m++) {
                var gate = Gate.U1(-Math.PI / Math.Pow(2, j - m));
                qubits[m].ControlledApply(qubits[j], gate);
            }
            qubits[j].H();
        }
    }

    public IOperator<IEnumerable<Qubit>> Adjoint() {
        return new Qft();
    }
}

}