---
title: Command Line Tooling
---
Verify if your quantum assembly files are syntactically and semantically valid by using the verify command

```sh
> qasm verify ./teleport.qasm
'./teleport.qasm' is syntactically and semantically valid
```

Optimize open quantum assembly files using a variety of optimization strategies and output the optimized code to a new file by using the optimize command

```sh
> qasm optimize -o "commutativity_check" ./teleport.qasm ./teleport.optimized.qasm
```

Execute quantum programs on a variety of quantum hardware including hardware from IBM's quantum experience as well as on a variety of simulators.

```sh
> qasm run --backend IBMYorktown --token "..." ./teleport.qasm
```

Execute quantum instructions in a read-print-evaluate-loop (repl) against a locally simulated quantum machine and view ths results after each instruction.

```sh
> qasm repl --qubits 10
OpenQASM interactive console
type 'exit' to quit, 'help' for information, 'print' for state info

|0> qreg q[2];
```