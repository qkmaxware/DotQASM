using DotQasm;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DotQasm {
    
[TestClass]
public class GateTest {
    [TestMethod]
    public void TestEquals() {
        Assert.AreEqual(Gate.Identity, Gate.Identity);
        Assert.AreEqual(Gate.Hadamard, Gate.Hadamard);
        Assert.AreEqual(Gate.PauliX, Gate.PauliX);
        Assert.AreEqual(Gate.PauliY, Gate.PauliY);
        Assert.AreEqual(Gate.PauliZ, Gate.PauliZ);

        Assert.AreNotEqual(Gate.Identity, Gate.Hadamard);
        Assert.AreNotEqual(Gate.Identity, Gate.PauliX);
        Assert.AreNotEqual(Gate.Identity, Gate.PauliY);
        Assert.AreNotEqual(Gate.Identity, Gate.PauliZ);

        Assert.AreNotEqual(Gate.Hadamard, Gate.Identity);
        Assert.AreNotEqual(Gate.Hadamard, Gate.PauliX);
        Assert.AreNotEqual(Gate.Hadamard, Gate.PauliY);
        Assert.AreNotEqual(Gate.Hadamard, Gate.PauliZ);

        Assert.AreNotEqual(Gate.PauliX, Gate.Identity);
        Assert.AreNotEqual(Gate.PauliX, Gate.Hadamard);
        Assert.AreNotEqual(Gate.PauliX, Gate.PauliY);
        Assert.AreNotEqual(Gate.PauliX, Gate.PauliZ);

        Assert.AreNotEqual(Gate.PauliY, Gate.Identity);
        Assert.AreNotEqual(Gate.PauliY, Gate.Hadamard);
        Assert.AreNotEqual(Gate.PauliY, Gate.PauliX);
        Assert.AreNotEqual(Gate.PauliY, Gate.PauliZ);

        Assert.AreNotEqual(Gate.PauliZ, Gate.Identity);
        Assert.AreNotEqual(Gate.PauliZ, Gate.Hadamard);
        Assert.AreNotEqual(Gate.PauliZ, Gate.PauliX);
        Assert.AreNotEqual(Gate.PauliZ, Gate.PauliY);
    }

    [TestMethod]
    public void TestCommutativity() {
        // All gates should commute with themselves AA == AA
        Assert.AreEqual(true, Gate.Identity.CommutesWith(Gate.Identity));
        Assert.AreEqual(true, Gate.Hadamard.CommutesWith(Gate.Hadamard));
        Assert.AreEqual(true, Gate.PauliX.CommutesWith(Gate.PauliX));
        Assert.AreEqual(true, Gate.PauliY.CommutesWith(Gate.PauliY));
        Assert.AreEqual(true, Gate.PauliZ.CommutesWith(Gate.PauliZ));

        // Identity gate should commute with all gates IA = AI
        Assert.AreEqual(true, Gate.Identity.CommutesWith(Gate.Identity));
        Assert.AreEqual(true, Gate.Identity.CommutesWith(Gate.Hadamard));
        Assert.AreEqual(true, Gate.Identity.CommutesWith(Gate.PauliX));
        Assert.AreEqual(true, Gate.Identity.CommutesWith(Gate.PauliY));
        Assert.AreEqual(true, Gate.Identity.CommutesWith(Gate.PauliZ));

        // Hadamard
        Assert.AreEqual(false, Gate.Hadamard.CommutesWith(Gate.PauliX));
        Assert.AreEqual(false, Gate.Hadamard.CommutesWith(Gate.PauliY));
        Assert.AreEqual(false, Gate.Hadamard.CommutesWith(Gate.PauliZ));

        // Pauli gates 
        Assert.AreEqual(false, Gate.PauliX.CommutesWith(Gate.PauliY));
        Assert.AreEqual(false, Gate.PauliX.CommutesWith(Gate.PauliZ));
        Assert.AreEqual(false, Gate.PauliY.CommutesWith(Gate.PauliZ));
    }

    private void TestGateMultiplication(Gate A, Gate B) {
        // Check if A.Matrix * B.Matrix = Gate.Multiply(A,B)
        //var matrix = A.Matrix.Multiply(B.Matrix);
        //var gate = A.Multiply(B);
        //Assert.AreEqual(true, matrix.Equivalent(gate.Matrix));
    }

    [TestMethod]
    public void TestMultiplication() {
        TestGateMultiplication(Gate.Identity, Gate.Identity);
        TestGateMultiplication(Gate.Hadamard, Gate.Identity);
        TestGateMultiplication(Gate.PauliX, Gate.Identity);
        TestGateMultiplication(Gate.PauliY, Gate.Identity);
        TestGateMultiplication(Gate.PauliZ, Gate.Identity);

        TestGateMultiplication(Gate.Hadamard, Gate.Hadamard);
        TestGateMultiplication(Gate.Hadamard, Gate.PauliX);
        TestGateMultiplication(Gate.Hadamard, Gate.PauliY);
        TestGateMultiplication(Gate.Hadamard, Gate.PauliZ);

        TestGateMultiplication(Gate.PauliX, Gate.Hadamard);
        TestGateMultiplication(Gate.PauliX, Gate.PauliY);
        TestGateMultiplication(Gate.PauliX, Gate.PauliZ);

        TestGateMultiplication(Gate.PauliY, Gate.Hadamard);
        TestGateMultiplication(Gate.PauliY, Gate.PauliX);
        TestGateMultiplication(Gate.PauliY, Gate.PauliZ);

        TestGateMultiplication(Gate.PauliZ, Gate.Hadamard);
        TestGateMultiplication(Gate.PauliZ, Gate.PauliY);
        TestGateMultiplication(Gate.PauliZ, Gate.PauliX);
    }
}

}