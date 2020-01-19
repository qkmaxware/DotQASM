using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

public class IBMArmonk : IBMBackend {

    public override string BackendName => "ibmq_armonk";
    public override int QubitCount => 1;

    private static string[] _supportedGates = new string[]{
        "id",
        "u1",
        "u2",
        "u3"
    };
    public override IEnumerable<string> SupportedGates => Array.AsReadOnly(_supportedGates);

    public IBMArmonk(string key): base(key) {}

}

}