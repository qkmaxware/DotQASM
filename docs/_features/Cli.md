---
title: Command Line Tooling
---
dotQASM comes with a fully featured command line utility for handing operations related to quantum assembly files. The command line utility includes functionality such as:

**1)** Create new OpenQASM projects from a given algorithm template
```sh
> qasm new . maxcut
Created: ./qelib1.inc
Created: ./main.qasm
```

**2)** Verify if your quantum assembly files are syntactically and semantically valid by using the describe command

```sh
> qasm describe ./teleport.qasm
---------------------------------------------------------------------
| Property               | Value                                    |
---------------------------------------------------------------------
QASM Statements          40
Quantum Bits             3
Classic Bits             3
Scheduled Events         11
First Event              DotQASM.Scheduling.GateEvent
Last Event               DotQASM.Scheduling.MeasurementEvent
Gate Uses                8
Measurements             3
Resets                   0
Barriers                 1
Conditionals             2
Est. Time.               ~5.00ms
```
**3)** Render quantum circuits to scalable vector graphics (svg) circuit diagrams
```sh
> qasm render ./qft.qasm
Rendered circuit to 'qft.svg'
```

**4)** Optimize open quantum assembly files using a variety of optimization strategies and output the optimized code to a new file by using the optimize command

```sh
> qasm optimizations
--------------------------------------------------------------------
| Optimization Strategies                                          |
--------------------------------------------------------------------
Combine                          Combine neighboring gates into a single gate
Hardware Scheduling              Schedule gates for a given hardware configuration

> qasm optimize ./teleport.qasm ./teleport.optimized.qasm -o "Combine"
```

**5)** Execute quantum programs on a variety of quantum hardware including hardware from IBM's quantum experience as well as on a variety of simulators.

```sh
> qasm backends
--------------------------------------------------------------------
| local (local)                                                    |
--------------------------------------------------------------------
simulator

--------------------------------------------------------------------
| IBM (IBM Quantum Experience)                                     |
--------------------------------------------------------------------
ibmq_armonk
ibmq_burlington
ibmq_essex
ibmq_london
ibmq_16_melbourne
ibmq_ourense
ibmq_qasm_simulator
ibmq_vigo
ibmqx2

> qasm run --provider ibm --backend ibmq_qasm_simulator --api-key $Env:IBM_KEY ./coinflip.qasm
--------------------------------------------------------------------------------------------------------------------
| Backend                          | Total Time                                         | Execution Time           |
--------------------------------------------------------------------------------------------------------------------
| DotQASM.Backend.IBM.IBMSimulator | 00:00:05.5699385                                   | 00:00:00.0022656         |

------------------------------------
| Histogram                        |
------------------------------------
0 |||||||||||||||||||||||||| 51.66 %
1 ||||||||||||||||||||||||| 48.34 %
```

**6)** Execute quantum instructions in a read-print-evaluate-loop (repl) against a locally simulated quantum machine and view ths results after each instruction.

```sh
> qasm repl --qubits 2
-----------------------------------------------------------------------
| OpenQASM interactive console                                        |
-----------------------------------------------------------------------
type 'exit' to quit, 'help' for information, 'print' for state info

|0> qreg q[2];
|0> h q[0];
|0> print
(0.707, 0)|00> + (0.707, 0)|10>
|0> exit
```