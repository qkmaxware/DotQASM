using System;
using System.Linq;
using System.Collections.Generic;
using DotQasm.Backend;
using System.Numerics;
using System.Text;

namespace DotQasm.Compile.Generators {

/// <summary>
/// Generator for Shor's algorithm circuits. Derived from IBM's QIS kit implementation found at
/// https://qiskit.org/documentation/_modules/qiskit/aqua/algorithms/factorizers/shor.html#Shor
/// </summary>
public class ShorsGenerator : ICircuitGenerator<(int a, int N)>{

    private static Operators.Qft qft = new Operators.Qft();
    private static Operators.QftDg iqft = new Operators.QftDg();
    public static Operators.ControlledSwap cswap = new Operators.ControlledSwap();
    private static Random random = new Random();

    private static int gcd(int a, int b) {
        while (b != 0) {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public (int, int)? Factor(int N, IBackend backend) {
        // Step 1 choose a random number between 1 and N - 1
        var a = random.Next(2, N);

        // Step 2 check that there isn't already a non-trivial factor of N
        var factor = gcd(a, N);
        if (factor > 1) {
            return (factor, N / factor);
        }

        var attempts = 50; // Hard coded max attempt count
        for (var i = 0; i < attempts; i++) {
            // Step 3 run quantum kernel
            var kernel = Generate((a: a, N: N)); 
            var counting_qubits = (int)Math.Log(a, 2) + 1;
            kernel.Name += $" attempt {i}";
            var task = backend.Exec(kernel);
            task.RunSynchronously();
            var reading = task.Result.State >> 4; // There are 4 ancilla to remove
            var phase = reading / Math.Pow(2, counting_qubits); // s/r
            
            if (phase != 0) {
                // Step 4 guess 'r'
                int r = Math.Min(new Fraction(phase).Reduce().Denominator, N); // 'r' can't be bigger than what we are searching for

                // Step 5 use 'r' to find a potential factor
                var _aPowR2 = (int)Math.Pow(a, r/2);
                var guesses = (gcd(_aPowR2 - 1, N), gcd(_aPowR2 + 1, N));
                if (guesses.Item1 != 1 && (15 % guesses.Item1) == 0) {
                    return (guesses.Item1, N / guesses.Item1);
                } else if (guesses.Item2 != 1 && (15 % guesses.Item2) == 0) {
                    return (guesses.Item2, N / guesses.Item2);
                }
            }
        }

        // Could not find a factor
        return null;
    }

    public Circuit Generate((int a, int N) args) {
        // https://qiskit.org/documentation/_modules/qiskit/aqua/algorithms/factorizers/shor.html#Shor
        var circ = new Circuit("Shors Quantum Kernel");

        // Compute number of qubits 
        var n = (int)Math.Ceiling(Math.Log(args.N, 2));
        var qft = n + 1;
        var iqft = n + 1;

        // Setup registers
        var up_qreg     = circ.AllocateQubits(n * 2 );
        var up_creg     = circ.AllocateCbits (n * 2 );
        var down_qreg   = circ.AllocateQubits(n     );
        var aux_qreg    = circ.AllocateQubits(n + 2 );

        // Initialize up register to |+>
        foreach (var qubit in up_qreg) {
            qubit.H();
        }

        // Initialize down to |1>
        down_qreg.First().X();

        // Apply multiplication gates
        for (var i = 0; i < n * 2; i++) {
            var exact = BigInteger.Pow(new BigInteger(args.a), (int)Math.Pow(2, i));
            controlledMultipleModN (
                up_qreg[i],
                down_qreg,
                aux_qreg,
                exact,
                n,
                args.N
            );
        }

        // Do an inverse QFT
        ShorsGenerator.iqft.Invoke(up_qreg);

        // Measure
        foreach (var (qubit, cbit) in up_qreg.Zip(up_creg, (q,c) => (q,c))) {
            qubit.Measure(cbit);
        }

        return circ;
    }

    private static (BigInteger, BigInteger, BigInteger) egcd(BigInteger a, BigInteger b) {
        if (a == 0) {
            return (b, 0, 1);
        } else {
            var (g, y, x) = egcd(b % a, a);
            return (g, x - (b / a) * y, y);
        }
    }

    private static BigInteger modinv(BigInteger a, long m) {
        var (g, x, _) = egcd(a, m);
        if (g != 1) {
            throw new ArgumentException("Modular inverse does not exist");
        } else {
            return x % m;
        }
    }

    private void controlledMultipleModN(Qubit ctrl, Register<Qubit> qreg, Register<Qubit> aux, BigInteger a, int n, int N) {
        var qubits = aux.Take(n + 1).Reverse();

        qft.Invoke(qubits);
        for (var i = 0; i < n; i++) {
            controlledControlledPhiAddModN(
                aux,
                qreg[i],
                ctrl,
                aux[n + 1],
                (long)Math.Pow(2, i) * a % N,
                n,
                N
            );
        }
        iqft.Invoke(qubits);

        for (var i = 0; i < n; i++) {
            cswap.Invoke((ctrl, (qreg[i], aux[i])));
        }

        var aInv = modinv(a, N);
        qft.Invoke(qubits);

        for (var i = n - 1; i >= 0; i--) {
            controlledControlledPhiAddModNInv (
                aux,
                qreg[i],
                ctrl,
                aux[n + 1],
                (long)Math.Pow(2, i) * aInv % N,
                n,
                N
            );
        } 

        iqft.Invoke(qubits);
    }

    private static void controlledControlledPhiAddModN(Register<Qubit> q, Qubit ctrl1, Qubit ctrl2, Qubit aux, BigInteger a, int n, int N) {
        var qubits = q.Take(n + 1).Reverse();
        
        controlledControlledPhiAdd(q, ctrl1, ctrl2, a, n, N);
        phiAdd(q, n, N);

        iqft.Invoke(qubits);
        q[n].CX(aux);
        qft.Invoke(qubits);
        controlledPhiAdd(q, aux, n, N);

        controlledControlledPhiAdd(q, ctrl1, ctrl2, a, n, N, inverse: true);
        iqft.Invoke(qubits);
        q[n].X();
        q[n].CX(aux);
        q[n].X();
        qft.Invoke(qubits);
        controlledControlledPhiAdd(q, ctrl1, ctrl2, a, n, N);
    }

    private static void controlledControlledPhiAddModNInv(Register<Qubit> q, Qubit ctrl1, Qubit ctrl2, Qubit aux, BigInteger a, int n, int N) {
        var qubits = q.Take(n + 1).Reverse();
        
        controlledControlledPhiAdd(q, ctrl1, ctrl2, a, n, N, inverse: true);
        iqft.Invoke(qubits);
        q[n].X();
        q[n].CX(aux);
        q[n].X();
        qft.Invoke(qubits);

        controlledControlledPhiAdd(q, ctrl1, ctrl1, a, n, N);
        controlledPhiAdd(q, aux, n, N, inverse: true);

        iqft.Invoke(qubits);
        q[n].CX(aux);
        qft.Invoke(qubits);

        phiAdd(q, n, N);
        controlledControlledPhiAdd(q, ctrl1, ctrl2, a, n, N, inverse: true);
    }

    private static void controlledControlledPhiAdd (Register<Qubit> q, Qubit ctrl1, Qubit ctrl2, BigInteger a, int n, int N, bool inverse = false) {
        var angles = getAngles(a, n);
        foreach (var i in Range(n + 1)) {
            var angle = inverse ? -angles[i] : angles[i];
            var gate = new Operators.MCU1 (angle);
            gate.Invoke((new Qubit[]{ ctrl1, ctrl2 }, q[i]));
        }
    }

    private static void controlledPhiAdd (Register<Qubit> q, Qubit ctrl, int n, int N, bool inverse = false) {
        var angles = getAngles(N, n);
        foreach (var i in Range(0, n + 1)) {
            var angle = (inverse ? -angles[i] : angles[i]) / 2;

            ctrl.U1(angle);
            ctrl.CX(q[i]);
            q[i].U1(-angle);
            ctrl.CX(q[i]);
            q[i].U1(angle);
        }
    }

    private static void phiAdd(Register<Qubit> q, int n, int N, bool inverse = false) {
        var angles = getAngles(N, n);
        foreach (var i in Range(0, n + 1)) {
            q[i].U1(inverse ? -angles[i] : angles[i]);
        }
    }

    private static string ToNBaseString(BigInteger a, int n) {
        StringBuilder sb = new StringBuilder();
        while (a > 0) {
            sb.Insert(0,a % n);
            a /= n;
        }
        return sb.ToString();
    }

    private static double[] getAngles(BigInteger a, int n) {
        var s =  ToNBaseString(a, 2).PadLeft(n + 1);
        var angles = new double[n + 1];
        foreach (var i in Range(0, n + 1)) {
            foreach (var j in Range(i, n + 1)) {
                if (s[j] == '1') {
                    angles[n - i] += Math.Pow(2, -(j - i));
                } 
            }
            angles[n - i] *= Math.PI;
        }
        return angles;
    }

    private static IEnumerable<int> Range(int max) {
        for (var i = 0; i < max; i++) {
            yield return i;
        }
    }

    private static IEnumerable<int> Range(int min, int max) {
        for (var i = min; i < max; i++) {
            yield return i;
        }
    }

}

}