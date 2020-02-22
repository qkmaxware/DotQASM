using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

/// <summary>
/// Backend representing the 1-qubit ibm quantum experience Armonk device
/// </summary>
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

    private static KeyValuePair<int, int>[] _connectivity = new KeyValuePair<int, int>[]{

    };
    public override IEnumerable<KeyValuePair<int, int>> QubitConnectivity => Array.AsReadOnly(_connectivity);

    public IBMArmonk(string key): base(key) {}

}

}