using System;
using System.Numerics;

namespace DotQasm {

public class Gate {

    public string Name {get; protected set;}
    public string Symbol {get; protected set;}
    public int QubitCount {get;}
    public Complex[,] Matrix {get; protected set;} 

    public static readonly Gate Identity = new Gate(
        "Identity",
        "I",
        1,
        new Complex[,]{
            {1, 0},
            {0, 1}
        }
    );

    public static readonly Gate Hadamard = new Gate(
        "Hadamard",
        "H",
        1,
        new Complex[,]{
            {0.707, 0.707},
            {0.707, -0.707}
        }
    );

    public static readonly Gate PauliX = new Gate(
        "Pauli-X",
        "X",
        1,
        new Complex[,]{
            {0, 1},
            {1, 0}
        }
    );

    public static readonly Gate PauliY = new Gate(
        "Pauli-Y",
        "Y",
        1,
        new Complex[,]{
            {0, -1.i()},
            {1.i(), 0}
        }
    );

    public static readonly Gate PauliZ = new Gate(
        "Pauli-Z",
        "Z",
        1,
        new Complex[,]{
            {1, 0},
            {0, -1}
        }
    );

    public static readonly Gate Swap = new Gate(
        "Swap",
        "SWAP",
        2,
        new Complex[,] {
            {1, 0, 0, 0},
            {0, 0, 1, 0},
            {0, 1, 0, 0},
            {0, 0, 0, 1}
        }
    );

    public static readonly Gate CNot = new Gate(
        "Controlled Not",
        "CX",
        2,
        new Complex[,] {
            {1, 0, 0, 0},
            {0, 1, 0, 0},
            {0, 0, 0, 1},
            {0, 0, 1, 0}
        }
    );

    public static Gate PhaseShift(double rotation) {
        return new Gate(
            "Phase Shift-" + rotation,
            "R" + rotation,
            1,
            new Complex[,] {
                {1, 0},
                {0, new Complex(Math.Cos(rotation), Math.Sin(rotation))} // Euler's formula cos(x) + isin(x) = e^ix
            }
        );
    }

    public static Gate U(double theta, double phi, double lambda) {
        // https://github.com/Qiskit/openqasm/blob/master/spec/qasm2.rst
        Gate u = new Gate();
        u.Name = "Parametric Rotation Gate (" + theta + "," + phi + "," + lambda + ")";
        u.Symbol = "U(" + theta + "," + phi + "," + lambda + ")";

        double t2 = theta/2;
        double st2 = Math.Sin(t2);
        double ct2 = Math.Cos(t2);
        double PpL2 = phi + lambda / 2;
        double PmL2 = phi - lambda / 2;

        u.Matrix = new Complex[,]{
            {Complex.Exp( (-PpL2).i() ) * ct2, -Complex.Exp( (-PmL2).i() ) * st2 },
            {Complex.Exp(   PmL2.i()  ) * st2,  Complex.Exp(   PpL2.i()  ) * ct2}
        };

        return u;
    }

    private static int IPow(int x, int pow) {
        int ret = 1;
        while ( pow != 0 ) {
            if ( (pow & 1) == 1 )
                ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }

    protected Gate() {}

    public Gate(string name, string symbol, int qubits, Complex[,] matrix) {
        this.Name = name;
        this.Symbol = symbol;
        this.QubitCount = qubits;
        this.Matrix = matrix;
        
        if (this.Matrix.GetLength(0) != this.Matrix.GetLength(1)) {
            throw new MatrixRepresentationException("Unitary matrix must be square");
        }
        if (this.Matrix.GetLength(0) != IPow(2, this.QubitCount)) {
            throw new MatrixRepresentationException("Size of unitary matrix must be 2^qubits");
        }
    }

}

}