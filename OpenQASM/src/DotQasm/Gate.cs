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
        u.Parametres = U3Params(theta, phi, lambda);
        u.RecomputeMatrix(theta, phi, lambda);

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

    private void RecomputeMatrix(double theta, double phi, double lambda) {
        double t2 = theta/2;
        double st2 = Math.Sin(t2);
        double ct2 = Math.Cos(t2);
        double PpL2 = phi + lambda / 2;
        double PmL2 = phi - lambda / 2;

        this.Matrix = new Complex[,]{
            { Complex.Exp( (-PpL2).i() ) * ct2, -Complex.Exp( (-PmL2).i() ) * st2 },
            { Complex.Exp(   PmL2.i()  ) * st2,  Complex.Exp(   PpL2.i()  ) * ct2 }
        };
    }

    private bool CloseEnough(double a, double b, double epsilon = 0.0001) {
        double absA = Math.Abs (a);
        double absB = Math.Abs (b);
        double diff = Math.Abs (a - b);

        if (a == b) {
            return true;
        } else if (a == 0 || b == 0 || diff < double.Epsilon) {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < epsilon;
        } else { // use relative error
            return diff / (absA + absB) < epsilon;
        }
    }

    private bool CloseEnough(Complex a, Complex b, double epsilon = 0.0001) {
        return CloseEnough(a.Real, b.Real, epsilon) && CloseEnough(a.Imaginary, b.Imaginary, epsilon);
    }

    /// <summary>
    /// Check if this quantum gate commutes with another
    /// </summary>
    /// <param name="other">other quantum gate</param>
    public bool CommutesWith(Gate other) {
        // det[AB - BA] == 0 if gates commute. 
        var AB = this.Matrix.Multiply(other.Matrix);
        var BA = other.Matrix.Multiply(this.Matrix);
        var sub = AB.Subtract(BA);
        var det = sub[0,0] * sub[1,1] - sub[0,1] * sub[1,0];
        return CloseEnough(det, Complex.Zero);
    }

    /// <summary>
    /// Multiply two quantum operators to create a new operator
    /// </summary>
    /// <param name="other">gate to multiply with</param>
    /// <returns>New quantum gate</returns>
    public Gate Multiply(Gate other) {
        var lhs = this.Parametres;
        var rhs = other.Parametres;

        var pars = (lhs.Item1 + rhs.Item1, lhs.Item2 + rhs.Item2, lhs.Item3 + rhs.Item3); // Just add rotation angles?
        // Multiply(lhs, rhs);
        
        Gate u = new Gate();
        u.Name = "Parametric Rotation Gate (" + pars.Item1 + "," + pars.Item2 + "," + pars.Item3 + ")";
        u.Symbol = "U";

        u.Parametres = pars;
        u.RecomputeMatrix(pars.Item1, pars.Item2, pars.Item3);

        return u;
    }

    // override object.Equals
    public override bool Equals(object obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        var other = (Gate)obj;

        if (
            CloseEnough(this.Matrix[0,0], other.Matrix[0,0]) 
            && CloseEnough(this.Matrix[0,1], other.Matrix[0,1])
            && CloseEnough(this.Matrix[1,0], other.Matrix[1,0])
            && CloseEnough(this.Matrix[1,1], other.Matrix[1,1])
        ) {
            return true;
        } else {
            return false;
        }
    }
    
    // override object.GetHashCode
    public override int GetHashCode() {
        return this.Matrix[0,0].GetHashCode() ^ this.Matrix[0,1].GetHashCode() ^ this.Matrix[1,0].GetHashCode() ^ this.Matrix[1,1].GetHashCode();
    }

    /// <summary>
    /// Operator for quantum operator multiplication
    /// </summary>
    /// <param name="lhs">left hand operator</param>
    /// <param name="rhs">right hand operator</param>
    /// <returns>quantum operator</returns>
    public static Gate operator * (Gate lhs, Gate rhs) {
        return lhs.Multiply(rhs);
    }

}

}