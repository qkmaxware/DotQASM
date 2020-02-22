using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM {

/// <summary>
/// Backend representing the 5-qubit ibm quantum experience quantum simulator
/// </summary>
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

    private static KeyValuePair<int, int>[] _connectivity;
    public override IEnumerable<KeyValuePair<int, int>> QubitConnectivity => Array.AsReadOnly(_connectivity);

    static IBMSimulator() {
        _connectivity = new KeyValuePair<int, int>[32 * 32];
        int k = 0;
        for(int i = 0; i < 32; i++) {
            for(int j = 0; j < 32; j++) {
                _connectivity[k++] = new KeyValuePair<int, int>(i, j);
            } 
        }   
    }

    public IBMSimulator(string key): base(key) {}

}

}