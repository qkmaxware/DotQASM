using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

public class IBMLondon : IBMBackend {

    public override string BackendName => "ibmq_london";
    public override int QubitCount => 5;

    private static string[] _supportedGates = new string[]{
        "id",
        "u1",
        "u2",
        "u3",
        "cx"
    };
    public override IEnumerable<string> SupportedGates => Array.AsReadOnly(_supportedGates);

    public IBMLondon(string key): base(key) {}

}

}