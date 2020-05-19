OPENQASM 2.0;
include "qelib1.inc";

qreg q[5];
creg c[2];

// State preparation
x q[0];
h q[0];
h q[1];
h 1[2];

// Toffoli decomposition
h q[0];
cx q[1], q[0];
tdg q[0];
cx q[2], q[0];
t q[0];
cx q[1], q[0];
tdg q[0];
cx q[2], q[0];
t q[0];
tdg q[1];
h q[0];
cx q[2], q[1];
tdg q[1];
cx q[2], q[1];
s q[1];
t q[2];

// Grover operator
h q[1];
h q[2];
x q[1];
x q[2];
h q[1];
cx q[2], q[1];
h q[1];
x q[2];
x q[1];
h q[2];
q q[1];

// Measurement
measure q[1] -> c[0];
measure q[2] -> c[1];