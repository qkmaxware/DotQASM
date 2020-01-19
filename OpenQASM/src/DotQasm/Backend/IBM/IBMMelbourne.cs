using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

public class IBMMelbourne : IBMBackend {

    public override string BackendName => "ibmq_16_melbourne";
    public override int QubitCount => 15;

    private static string[] _supportedGates = new string[]{
        "id",
        "u1",
        "u2",
        "u3",
        "cx"
    };
    public override IEnumerable<string> SupportedGates => Array.AsReadOnly(_supportedGates);

    public IBMMelbourne(string key): base(key) {}

}

}