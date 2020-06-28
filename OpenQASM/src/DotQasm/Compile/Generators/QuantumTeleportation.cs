using DotQasm.Scheduling;

namespace DotQasm.Compile.Generators {

public class QuantumTeleportationGenerator : ICircuitGenerator {
    
    private static void Entangle(Qubit a, Qubit b) {
        a.H();
        a.CX(b);
    }

    private static void Alice(Qubit psi, Qubit a) {
        psi.CX(a);
        psi.H();
    }
    private static void Bob(Cbit c1, Cbit c2, Qubit bobs) {
        // If 00
            // Do nothing
        // If 01
            // Apply X
        // If 10 
            // Apply Z
        // If 11
            // Apply ZX
        bobs.IfApply(c1, Gate.PauliX);
        bobs.IfApply(c2, Gate.PauliZ);
    }

    public Circuit Generate() {
        // https://qiskit.org/textbook/ch-algorithms/teleportation.html
        Circuit circ = new Circuit("Quantum Teleportation");
        
        // Prepare
        var qreg = circ.AllocateQubits(3);
        var c1 = circ.AllocateCbit();
        var c2 = circ.AllocateCbit();

        // Create Entangled Bell State
        Entangle(qreg[1], qreg[2]);

        // Do Alice's Steps
        Alice(qreg[0], qreg[1]);
        qreg[0].Measure(c1);
        qreg[1].Measure(c2);

        // Do Bob's Steps
        Bob(c1, c2, qreg[2]);

        return circ;
    }
}

}