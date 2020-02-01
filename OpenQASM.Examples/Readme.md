# Example Quantum Kernel Implementations
This directory contains example quantum algorithms which can be handled by dotQASM. The algorithms contained here all come from a variety of sources. As such, each source is listed below with a list of all the algorithms that came from that source. 

## [Quantum Algorithm Implementations for Beginners](https://arxiv.org/abs/1804.03719)

## [QISKit Benchmarks](https://github.com/Qiskit/qiskit/tree/master/test/benchmarks/qasm)
### Includes
Includes are not algorithms that are cabable of being executed, but instead are included as components to some of the algorithms listed below.

- `qelib1.inc`: Standard gate definitions for OpenQASM algorithms

### Generic
Algorithms used by QISkit as benchmarks.

- `pea_3_pi_8.qasm`: 
- `test_eoh.qasm`: 

## [OpenQASM Examples](https://github.com/Qiskit/openqasm/tree/master/examples)
### Includes
Includes are not algorithms that are cabable of being executed, but instead are included as components to some of the algorithms listed below.

- `qelib1.inc`: Standard gate definitions for OpenQASM algorithms

### Generic
Generic algorithms can be run on quantum simulators but may not be able to run current hardware.

- `adder.qasm`: Adds two four-bit numbers.
- `bigadder.qasm`: Quantum ripple-carry adder. 8-bit adder made out of 2 4-bit adders from adder.qasm.
- `inverseqft1.qasm`: Inverse quantum Fourier transform using 4 qubits.
- `inverseqft2.qasm`: Another version of the inverse quantum Fourier transform using 4 qubits.
- `ipea_3_pi_8.qasm`: 4-bit Iterative Phase Estimation algorithm for phase 3\pi/8 using two qubits.ss
- `pea_3_pi_8.qasm`: 4-bit Phase Estimation algorithm for a phase 3\pi/8 using 5 qubits.
- `qec.qasm`: Repetition code to correct quantum errors.
- `qft.qasm`: Quantum Fourier transform on 4 qubits.
- `qpt.qasm`: Quantum Process Tomography example.
- `rb.qasm`: Example of a single instance of two-qubits randomized benchmarking.
- `teleport.qasm`: Quantum Teleportation example.
- `teleportv2.qasm`: Quantum Teleportation example (one classical register).
- `W-state.qasm`: Generating a 3-qubit W-state using Toffoli gates

### IBM Q Experience
These algorithms are able to be run on the **ibmqx2** device.

- `iswap.qasm`: Implements the two-qubit entangling gate iSWAP, defined by the matrix: `[1 0 0 0],[0,0,i,0],[0,i,0,0],[0,0,0,1]`.
- `W3test.qasm`: Implements the three-qubit maximally entangled W state |001> + |010> + |100>.
- `Deutsch_Algorithm.qasm`: A two-qubit example of Deutsch to determine whether a function is constant (in which case the algorithm returns 0) or balanced (returns 1). In this example the algorithm looks at the function f(x) = x, which is balanced.
- `011_3_qubit_grover_50_.qasm`: This circuit demonstrate Grover's search algorithm over three qubits. In this case it searchs for the state 110 with probability of success > 50%.
- `qe_qft_3(4)(5).qasm`: Quantum Fourier transforms with 3, 4, and 5 qubits.
