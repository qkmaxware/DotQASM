namespace DotQasm {

/// <summary>
/// Generic gate exception
/// </summary>
public class GateException : System.Exception {
    public GateException(string message): base(message) {}
}

/// <summary>
/// Invalid matrix representation exception
/// </summary>
public class MatrixRepresentationException : GateException {
    public MatrixRepresentationException(string msg) : base(msg) {}
}

}