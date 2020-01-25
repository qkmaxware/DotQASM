---
title: Read & Write Open Quantum Assembly
---
The dotQASM libraries allow users to load quantum assembly files from disk, manipulate quantum circuits, and save the resultant quantum assembly back to disk.  
```cs
// Read OpenQASM files
var circuit = DotQASM.IO.OpenQasm.Parser.ParseCircuit(qasm);

// Write OpenQASM files
using (var writer = new StreamWriter("myCircuit.qasm")) {
    DotQASM.IO.OpenQasm.Emitter.EmitCircuit(circuit, writer);
}
```