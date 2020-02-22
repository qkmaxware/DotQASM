namespace DotQasm.Backend {

/// <summary>
/// Class representing an error when an unsupported gate is used 
/// </summary>
public class UnsupportedGateException: System.Exception {

    public UnsupportedGateException(Gate gate): base(
        string.Format(
            "gate '{0} ({1})' is not supported on the selected backend",
            gate.Name,
            gate.Symbol
        )
    ) {}

}

}