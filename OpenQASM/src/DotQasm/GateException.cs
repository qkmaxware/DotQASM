namespace DotQasm {

public class GateException : System.Exception {
    public GateException(string message): base(message) {}
}

public class MatrixRepresentationException : GateException {
    public MatrixRepresentationException(string msg) : base(msg) {}
}

}