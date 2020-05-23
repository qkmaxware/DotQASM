using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Compile.Generators {

/// <summary>
/// Arguments provided to Quantum Approximate Optimization Algorithms
/// </summary>
public struct QaoaArguments<T>{
    public int RoundsOfOptimization;
    public float[] GammaAngles;
    public float[] BetaAngles;
    public IGraph<int, T> Graph;
    
    public float Gamma(int k) {
        return GammaAngles.ElementAtOrDefault(k);
    }

    public float Beta(int k) {
        return BetaAngles.ElementAtOrDefault(k);
    }

    public int M => Graph.VertexCount;
}

/// <summary>
/// Base class for all Quantum Approximate Optimization problems
/// </summary>
/// <typeparam name="QaoaArguments"></typeparam>
public abstract class QaoaGenerator<T> : ICircuitGenerator<QaoaArguments<T>> {

    public abstract string OptimizationProblemName {get;}

    protected abstract void ApplyGammaAngle(Register<Qubit> reg, IGraph<int, T> graph, float angle);
    protected abstract void ApplyBetaAngle(Register<Qubit> reg, IGraph<int, T> graph, float angle);

    public Circuit Generate(QaoaArguments<T> args) {
        var Circuit = new Circuit("Quantum Approximate Optimization" + (OptimizationProblemName != null ? " " + OptimizationProblemName: string.Empty));
        var n = args.Graph.VertexCount;
        var qreg = Circuit.AllocateQubits(n);
        var creg = Circuit.AllocateCbits(n);

        // 1. Construct the n-qubit uniform superposition 
        foreach (var qubit in qreg) {
            qubit.H();
        }

        // 2. Apply operators
        for (int k = 0; k < args.RoundsOfOptimization; k++) {
            // 2a. Apply Product(alpha = 1, m) of e^-i*Gamma[k]*Ca 
            ApplyGammaAngle(qreg, args.Graph, args.Gamma(k));
            // 2b. Apply Product(j = 1, n) of e^-1*Beta[k]Xj
            ApplyBetaAngle(qreg, args.Graph, args.Beta(k));
        }

        // 3. Measure resultant state
        for (int i = 0; i < n; i++) {
            qreg[i].Measure(creg[i]);
        }

        return Circuit;
    }

}

/// <summary>
/// Generator for the MaxCut Quantum Approximate Optimization Algorithm
/// created by study of "Quantum Algorithm Implementations for Beginners": https://arxiv.org/pdf/1804.03719.pdf
/// </summary>
public class MaxCutGenerator<T>: QaoaGenerator<T> {
    public override string OptimizationProblemName => "MaxCut";

    private void ApplyGammaAngle(Qubit q1, Qubit q2, float angle) {
        q2.CX(q1);
        q1.U1(-angle);
        q2.CX(q1);
    }

    protected override void ApplyGammaAngle(Register<Qubit> reg, IGraph<int, T> graph, float angle) {
        // TODO, guarantee undirected edges (remove back duplicates)
        foreach(var edge in graph.Edges) {
            ApplyGammaAngle(reg[edge.Startpoint], reg[edge.Endpoint], angle);
        }
    }

    protected override void ApplyBetaAngle(Register<Qubit> reg, IGraph<int, T> graph, float angle) {
        var pi_2 = Math.PI / 2d;
        var gate = Gate.U3(2 * angle, -pi_2, pi_2);

        foreach (var qubit in reg) {
            qubit.Apply(gate);
        }
    }
}

}