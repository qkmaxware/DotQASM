using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

public class IBMSimulator : IBMBackend {

    public override string BackendName => "ibmq_qasm_simulator";

    public override int QubitCount => 32;

    private static string[] _supportedGates = new string[]{
        "id",
        "u1",
        "u2",
        "u3",
        "cx",
        "cz",
        "x",
        "y",
        "z",
        "h",
        "s",
        "sdg",
        "t",
        "tdg",
        "ccx",
        "swap",
        "unitary",
        "initialize",
        "kraus"
    };
    public override IEnumerable<string> SupportedGates => Array.AsReadOnly(_supportedGates);

    public IBMSimulator(string key): base(key) {}

}

}