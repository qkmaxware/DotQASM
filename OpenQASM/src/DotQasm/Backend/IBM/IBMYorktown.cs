using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

/// <summary>
/// Backend representing the 5-qubit ibm quantum experience Yorktown device
/// </summary>
public class IBMYorktown : IBMBackend {

    public override string BackendName => "ibmqx2";
    public override int QubitCount => 5;
    
    private static string[] _supportedGates = new string[]{
        "id",
        "u1",
        "u2",
        "u3",
        "cx"
    };
    public override IEnumerable<string> SupportedGates => Array.AsReadOnly(_supportedGates);

    private static KeyValuePair<int, int>[] _connectivity = new KeyValuePair<int, int>[]{
        new KeyValuePair<int, int>(0, 1),
        new KeyValuePair<int, int>(1, 0),
        new KeyValuePair<int, int>(1, 2),
        new KeyValuePair<int, int>(2, 1),
        new KeyValuePair<int, int>(0, 2),
        new KeyValuePair<int, int>(2, 0),
        new KeyValuePair<int, int>(2, 3),
        new KeyValuePair<int, int>(3, 2),
        new KeyValuePair<int, int>(2, 4),
        new KeyValuePair<int, int>(4, 2),
        new KeyValuePair<int, int>(3, 4),
        new KeyValuePair<int, int>(4, 3),
    };
    public override IEnumerable<KeyValuePair<int, int>> QubitConnectivity => Array.AsReadOnly(_connectivity);

    public IBMYorktown(string key): base(key) {}

}

}