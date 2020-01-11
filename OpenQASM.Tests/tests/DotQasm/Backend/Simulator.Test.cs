using System.Numerics;
using DotQasm;
using DotQasm.Backend.Local;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DotQasm.Backend {

[TestClass]
public class SimulatorTest {

    [TestMethod]
    public void TestSimulatorConstruction() {
        Simulator sim = new Simulator(5);
        
        Assert.AreEqual(5, sim.QubitCount);
        Assert.AreEqual(32, sim.StateCount);
        for (int i = 0; i < sim.StateCount; i++) {
            // First state = 1, rest are 0
            Assert.AreEqual((i == 0 ? Complex.One : Complex.Zero), sim[i]);
        }
    }

    [TestMethod]
    public void TestIdentity() {
        Simulator sim = new Simulator(1);

        sim.ApplyGate(0, Gate.Identity);

        Assert.AreEqual(new Complex(1, 0), sim[0]);
        Assert.AreEqual(new Complex(0, 0), sim[1]);
    }

    [TestMethod]
    public void TestHadamard() {
        Simulator sim = new Simulator(1);

        sim.ApplyGate(0, Gate.Hadamard);

        Assert.AreEqual(new Complex(0.707, 0), sim[0]);
        Assert.AreEqual(new Complex(0.707, 0), sim[1]);
    }

    [TestMethod]
    public void TestX() {
        Simulator sim = new Simulator(1);

        sim.ApplyGate(0, Gate.PauliX);

        Assert.AreEqual(new Complex(0, 0), sim[0].Magnitude);
        Assert.AreEqual(new Complex(1, 0), sim[1].Magnitude);
    }

    [TestMethod]
    public void TestY() {
        Simulator sim = new Simulator(1);

        sim.ApplyGate(0, Gate.PauliY);

        Assert.AreEqual(new Complex(0, 0), sim[0]);
        Assert.AreEqual(new Complex(0, 1), sim[1]);
    }

    [TestMethod]
    public void TestZ() {
        Simulator sim = new Simulator(1);

        sim.ApplyGate(0, Gate.PauliZ);

        Assert.AreEqual(new Complex(1, 0), sim[0]);
        Assert.AreEqual(new Complex(0, 0), sim[1]);
    }

    [TestMethod]
    public void TestControlledGate() {
        Simulator sim = new Simulator(2);
        
        // Starting, bit 0 is unset, controlled not should do nothing
        sim.ApplyControlledGate(0, 1, Gate.PauliX);
        
        Assert.AreEqual(new Complex(1,0), sim[0]);
        Assert.AreEqual(new Complex(0,0), sim[1]);
        Assert.AreEqual(new Complex(0,0), sim[2]);
        Assert.AreEqual(new Complex(0,0), sim[3]);

        // bit 0 is now set, controlled not should swap the value
        sim.ApplyGate(0, Gate.PauliX);
        Assert.AreEqual(new Complex(0,0), sim[0]);
        Assert.AreEqual(new Complex(0,0), sim[1]);
        Assert.AreEqual(new Complex(1,0), sim[2]);
        Assert.AreEqual(new Complex(0,0), sim[3]);

        sim.ApplyControlledGate(0, 1, Gate.PauliX);
        Assert.AreEqual(new Complex(0,0), sim[0]);
        Assert.AreEqual(new Complex(0,0), sim[1]);
        Assert.AreEqual(new Complex(0,0), sim[2]);
        Assert.AreEqual(new Complex(1,0), sim[3]);
    }

    [TestMethod]
    public void TestZero() {
        Simulator sim = new Simulator(1);
    
        sim.ApplyGate(0, Gate.PauliX);              // Qubit is now 1
        Assert.AreEqual(new Complex(0,0), sim[0]);
        Assert.AreEqual(new Complex(1,0), sim[1]);

        sim.Zero(0);                                // Qubit is now 0
        Assert.AreEqual(new Complex(1,0), sim[0]);
        Assert.AreEqual(new Complex(0,0), sim[1]);
    }

    [TestMethod]
    public void TestMeasurement() {
        // Since measurements are probabalistic, repeat several times 
        for (int i = 0; i < 4; i++) {
            Simulator sim = new Simulator(1);

            sim.ApplyGate(0, Gate.Hadamard);

            var state = sim.Measure(0);
            switch (state) {
                case State.Zero: {
                        // Check state collapse
                        Assert.AreEqual(new Complex(1,0), sim[0]);
                        Assert.AreEqual(new Complex(0,0), sim[1]);
                        // Subsequent measurements should always return the same value
                        var restate = sim.Measure(0);
                        Assert.AreEqual(state, restate);
                        break;
                    }
                case State.One: {
                        // Check state collapse
                        Assert.AreEqual(new Complex(0,0), sim[0]);
                        Assert.AreEqual(new Complex(1,0), sim[1]);
                        // Subsequent measurements should always return the same value
                        var restate = sim.Measure(0);
                        Assert.AreEqual(state, restate);
                        break;
                    }
            }
        }
    }

    [TestMethod]
    public void TestMeasureAll() {
        for (int i = 0; i < 4; i++) {
            Simulator sim = new Simulator(2);

            sim.ApplyGate(0, Gate.Hadamard);
            sim.ApplyGate(1, Gate.Hadamard);

            // Subsequent measurements should always return the same value
            int state = sim.MeasureAll();
            Assert.AreEqual(state, sim.MeasureAll());
        }
    }

}

}