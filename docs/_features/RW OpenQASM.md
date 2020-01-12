---
title: Read & Write Open Quantum Assembly
---
```cs
// Read OpenQASM files
var circuit = DotQasm.IO.OpenQasm.Parser.ParseCircuit(qasm);

// Write OpenQASM files
using (var writer = new StreamWriter("myCircuit.qasm")) {
    DotQasm.IO.OpenQasm.Emitter.EmitCircuit(circuit, writer);
}
```