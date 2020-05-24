using System;
using System.Numerics;

namespace DotQasm {

/// <summary>
/// Single qubit quantum gate
/// </summary>
public class Gate {
    /// <summary>
    /// Quantum gate name
    /// </summary>
    public string Name {get; protected set;}
    /// <summary>
    /// Quantum gate abbreviated symbol
    /// </summary>
    public string Symbol {get; protected set;}
    /// <summary>
    /// Operation's 2x2 matrix representation
    /// </summary>
    public Complex[,] Matrix {get; protected set;} 
    /// <summary>
    /// Classical rotation parametres 
    /// </summary>
    public (double, double, double) Parametres {get; private set;}

    /// <summary>
    /// The identity gate
    /// </summary>
    public static readonly Gate Identity = new Gate(
        "Identity",
        "i",
        new Complex[,]{
            {1, 0},
            {0, 1}
        },
        (0, 0, 0)
    );

    /// <summary>
    /// The hadamard gate
    /// </summary>  
    public static readonly Gate Hadamard = new Gate(
        "Hadamard",
        "h",
        new Complex[,]{
            {0.707, 0.707},
            {0.707, -0.707}
        },
        U2Params(0, Math.PI)
    );

    /// <summary>
    /// The Pauli X rotation gate
    /// </summary>
    public static readonly Gate PauliX = new Gate(
        "Pauli-X",
        "x",
        new Complex[,]{
            {0, 1},
            {1, 0}
        },
        U3Params(Math.PI, 0, Math.PI)
    );

    /// <summary>
    /// The Pauli Y rotation gate
    /// </summary>
    public static readonly Gate PauliY = new Gate(
        "Pauli-Y",
        "y",
        new Complex[,]{
            {0, -1.i()},
            {1.i(), 0}
        },
        U3Params(Math.PI, Math.PI/2, Math.PI/2)
    );

    /// <summary>
    /// The Pauli Z rotation gate
    /// </summary>
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

    /// <summary>
    /// Apply a phase rotation of the given angle
    /// </summary>
    /// <param name="theta">phase angle</param>
    public static Gate PhaseShift(float theta) {
        var gate = U1(theta);
        gate.Name = "rφ(" + theta + ")";
        gate.Symbol = "rφ";
        return gate;
    }

    /// <summary>
    /// Create an X-Axis rotation gate
    /// </summary>
    /// <param name="theta">rotation angle</param>
    /// <returns>quantum gate</returns>
    public static Gate Rx(double theta) {
        var gate = U3(theta, -Math.PI/2, Math.PI/2);
        gate.Name = "rx(" + theta + ")";
        gate.Symbol = "rx";
        return gate;
    }

    /// <summary>
    /// Create an Y-Axis rotation gate
    /// </summary>
    /// <param name="theta">rotation angle</param>
    /// <returns>quantum gate</returns>
    public static Gate Ry(double theta) {
        var gate = U3(theta,0,0);
        gate.Name = "ry(" + theta + ")";
        gate.Symbol = "ry";
        return gate;
    }

    /// <summary>
    /// Create an Z-Axis rotation gate
    /// </summary>
    /// <param name="theta">rotation angle</param>
    /// <returns>quantum gate</returns>
    public static Gate Rz(double phi) {
        var gate = U1(phi);
        gate.Name = "rz(" + phi + ")";
        gate.Symbol = "rz";
        return gate;
    }

    /// <summary>
    /// 1-parameter 0-pulse single qubit gate
    /// </summary>
    /// <param name="lambda">rotation</param>
    /// <returns>quantum gate</returns>
    public static Gate U1(double lambda) {
        var g = U(0, 0, lambda);
        g.Symbol = "u1";
        return g;
    }

    private static (double, double, double) U1Params (double lambda) {
        return (0, 0, lambda);
    }

    /// <summary>
    /// 2-parameter 1-pulse single qubit gate
    /// </summary>
    /// <param name="lambda">rotation</param>
    /// <returns>quantum gate</returns>
    public static Gate U2(double phi, double lambda) {
        var g = U(Math.PI / 2, phi, lambda);
        g.Symbol = "u2";
        return g;
    }

    private static (double, double, double) U2Params (double phi, double lambda) {
        return ((Math.PI / 2), phi, lambda);
    }

    /// <summary>
    /// 3-parameter 2-pulse single qubit gate
    /// </summary>
    /// <param name="lambda">rotation</param>
    /// <returns>quantum gate</returns>
    public static Gate U3(double theta, double phi, double lambda) {
        var g = U(theta, phi, lambda);
        g.Symbol = "u3";
        return g;
    }

    private static (double, double, double) U3Params (double theta, double phi, double lambda) {
        return (theta, phi, lambda);
    }

    /// <summary>
    /// 3-parameter 2-pulse single qubit gate
    /// </summary>
    /// <param name="lambda">rotation</param>
    /// <returns>quantum gate</returns>
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
        if (a != b) {
            return Math.Abs(a - b) < epsilon;
        }

        return true;
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