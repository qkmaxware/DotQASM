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

    private static KeyValuePair<int, int>[] _connectivity = new KeyValuePair<int, int>[]{
        new KeyValuePair<int, int>(0, 1),
        new KeyValuePair<int, int>(1, 0),
        new KeyValuePair<int, int>(1, 2),
        new KeyValuePair<int, int>(2, 1),
        new KeyValuePair<int, int>(2, 3),
        new KeyValuePair<int, int>(3, 2),
        new KeyValuePair<int, int>(3, 4),
        new KeyValuePair<int, int>(4, 3),
        new KeyValuePair<int, int>(4, 5),
        new KeyValuePair<int, int>(5, 4),
        new KeyValuePair<int, int>(5, 6),
        new KeyValuePair<int, int>(6, 5),
        new KeyValuePair<int, int>(6, 8),
        new KeyValuePair<int, int>(8, 6),
        new KeyValuePair<int, int>(8, 7),
        new KeyValuePair<int, int>(7, 8),
        new KeyValuePair<int, int>(8, 9),
        new KeyValuePair<int, int>(9, 8),
        new KeyValuePair<int, int>(9, 10),
        new KeyValuePair<int, int>(10, 9),
        new KeyValuePair<int, int>(10, 11),
        new KeyValuePair<int, int>(11, 10),
        new KeyValuePair<int, int>(11, 12),
        new KeyValuePair<int, int>(12, 11),
        new KeyValuePair<int, int>(12, 13),
        new KeyValuePair<int, int>(13, 14),
        new KeyValuePair<int, int>(14, 13),
        new KeyValuePair<int, int>(14, 0),
        new KeyValuePair<int, int>(0, 14),
        new KeyValuePair<int, int>(13, 1),
        new KeyValuePair<int, int>(1, 13),
        new KeyValuePair<int, int>(12, 2),
        new KeyValuePair<int, int>(2, 12),
        new KeyValuePair<int, int>(11, 3),
        new KeyValuePair<int, int>(3, 11),
        new KeyValuePair<int, int>(10, 4),
        new KeyValuePair<int, int>(4, 10),
        new KeyValuePair<int, int>(9, 5),
        new KeyValuePair<int, int>(5, 9),
    };
    public override IEnumerable<KeyValuePair<int, int>> QubitConnectivity => Array.AsReadOnly(_connectivity);

    public IBMMelbourne(string key): base(key) {}

}

}