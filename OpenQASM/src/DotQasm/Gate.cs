using System;
using System.Numerics;

namespace DotQasm {

public class Gate {

    public string Name {get; protected set;}
    public string Symbol {get; protected set;}
    public Complex[,] Matrix {get; protected set;} 

    public (double, double, double) Parametres {get; private set;}

    public static readonly Gate Identity = new Gate(
        "Identity",
        "i",
        new Complex[,]{
            {1, 0},
            {0, 1}
        },
        (0, 0, 0)
    );

    public static readonly Gate Hadamard = new Gate(
        "Hadamard",
        "h",
        new Complex[,]{
            {0.707, 0.707},
            {0.707, -0.707}
        },
        U2Params(0, Math.PI)
    );

    public static readonly Gate PauliX = new Gate(
        "Pauli-X",
        "x",
        new Complex[,]{
            {0, 1},
            {1, 0}
        },
        U3Params(Math.PI, 0, Math.PI)
    );

    public static readonly Gate PauliY = new Gate(
        "Pauli-Y",
        "y",
        new Complex[,]{
            {0, -1.i()},
            {1.i(), 0}
        },
        U3Params(Math.PI, Math.PI/2, Math.PI/2)
    );

    public static readonly Gate PauliZ = new Gate(
        "Pauli-Z",
        "z",
        new Complex[,]{
            {1, 0},
            {0, -1}
        },
        U1Params(Math.PI)
    );

    /*
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
    );*/

    public static Gate Rx(double theta) {
        var gate = U3(theta, -Math.PI/2, Math.PI/2);
        gate.Name = "rx(" + theta + ")";
        gate.Symbol = "rx";
        return gate;
    }

    public static Gate Ry(double theta) {
        var gate = U3(theta,0,0);
        gate.Name = "ry(" + theta + ")";
        gate.Symbol = "ry";
        return gate;
    }

    public static Gate Rz(double phi) {
        var gate = U1(phi);
        gate.Name = "rz(" + phi + ")";
        gate.Symbol = "rz";
        return gate;
    }

    public static Gate U1(double lambda) {
        var g = U(0, 0, lambda);
        g.Symbol = "u1";
        return g;
    }

    private static (double, double, double) U1Params (double lambda) {
        return (0, 0, lambda);
    }

    public static Gate U2(double phi, double lambda) {
        var g = U(Math.PI / 2, phi, lambda);
        g.Symbol = "u2";
        return g;
    }

    private static (double, double, double) U2Params (double phi, double lambda) {
        return ((Math.PI / 2), phi, lambda);
    }

    public static Gate U3(double theta, double phi, double lambda) {
        var g = U(theta, phi, lambda);
        g.Symbol = "u3";
        return g;
    }

    private static (double, double, double) U3Params (double theta, double phi, double lambda) {
        return (theta, phi, lambda);
    }

    public static Gate U(double theta, double phi, double lambda) {
        // https://github.com/Qiskit/openqasm/blob/master/spec/qasm2.rst
        Gate u = new Gate();
        u.Name = "Parametric Rotation Gate (" + theta + "," + phi + "," + lambda + ")";
        u.Symbol = "U";

        double t2 = theta/2;
        double st2 = Math.Sin(t2);
        double ct2 = Math.Cos(t2);
        double PpL2 = phi + lambda / 2;
        double PmL2 = phi - lambda / 2;

        u.Matrix = new Complex[,]{
            {Complex.Exp( (-PpL2).i() ) * ct2, -Complex.Exp( (-PmL2).i() ) * st2 },
            {Complex.Exp(   PmL2.i()  ) * st2,  Complex.Exp(   PpL2.i()  ) * ct2}
        };
        u.Parametres = U3Params(theta, phi, lambda);

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

    protected Gate(string name, string symbol, Complex[,] matrix, (double, double, double) parametres) {
        this.Name = name;
        this.Symbol = symbol;
        this.Matrix = matrix;

        this.Parametres = parametres;
        
        if (this.Matrix.GetLength(0) != this.Matrix.GetLength(1)) {
            throw new MatrixRepresentationException("Unitary matrix must be square");
        }
        if (this.Matrix.GetLength(0) != 2) {
            throw new MatrixRepresentationException("Size of unitary matrix must be 2");
        }
    }

}

}