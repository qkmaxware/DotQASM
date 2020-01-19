using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

public class IBMBurlington : IBMBackend {

    public override string BackendName => "ibmq_burlington";
    public override int QubitCount => 5;

    private static string[] _supportedGates = new string[]{
        "id",
        "u1",
        "u2",
        "u3",
        "cx"
    };
    public override IEnumerable<string> SupportedGates => Array.AsReadOnly(_supportedGates);

    public IBMBurlington(string key): base(key) {}

}

}