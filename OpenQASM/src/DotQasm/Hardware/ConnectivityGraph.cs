using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotQasm.Hardware {

public class PhysicalQubit {
    public string Name;
    public int Colour = 0;    

    public PhysicalQubit() {}
    public PhysicalQubit(PhysicalQubit other) {
        this.Colour = other.Colour;
    }

    public override string ToString() => Name;
}

public class Channel {
    public Channel() {}
    public Channel(Channel other) {}
}

/// <summary>
/// Graph representing the hardware connectivity between qubits
/// </summary>
public class ConnectivityGraph: EdgeListGraph<PhysicalQubit, Channel> {
    /// <summary>
    /// Create an empty connectivity graph
    /// </summary>
    public ConnectivityGraph() {}

    /// <summary>
    /// Copy a connectivity graph
    /// </summary>
    /// <param name="other">graph to copy</param>
    public ConnectivityGraph(ConnectivityGraph other) {
        foreach (var vertice in other.Vertices) {
            // Convert vertice
            this.Add(new PhysicalQubit(vertice));
        }
        foreach (var edge in other.Edges) {
            // Convert edge
            this.DirectedEdge(
                other.Vertices.IndexOf(edge.Startpoint), 
                other.Vertices.IndexOf(edge.Endpoint), 
                new Channel(edge.Data)
            );
        }
    }

    /// <summary>
    /// Each qubit is connected to the next bi-directionally
    /// </summary>
    /// <param name="qubits">number of qubits</param>
    /// <returns>connectivity graph</returns>
    public static ConnectivityGraph Linear(int qubits) {
        var graph = new ConnectivityGraph();

        // Add qubits
        for (int i = 0; i < qubits; i++) {
            graph.Add(new PhysicalQubit());
        }

        // Connect qubits
        for (int i = 0; i < qubits - 1; i++) {
            graph.UndirectedEdge(i, i + 1, new Channel());
        }

        return graph;
    }

    /// <summary>
    /// Fully connect each qubit to each other qubit
    /// </summary>
    /// <param name="qubits">number of qubits</param>
    /// <returns>connectivity graph</returns>
    public static ConnectivityGraph FullyConnected(int qubits) {
        var graph = new ConnectivityGraph();

        // Add qubits
        for (int i = 0; i < qubits; i++) {
            graph.Add(new PhysicalQubit());
        }

        // Connect qubits
        foreach (var from in graph.Vertices) {
            foreach (var to in graph.Vertices) {
                if (from != to) {
                    graph.DirectedEdge(from, to, new Channel());
                }
            }
        }

        return graph;
    }

    private void WriteQubit(StringBuilder writer, PhysicalQubit qubit) {
        writer.Append(qubit.Name).Append('[').Append("colour=").Append(qubit.Colour).Append(']');
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach (var edge in this.Edges) {
            var start = edge.Startpoint;
            var end = edge.Endpoint;

            WriteQubit(sb, start);
            sb.Append(" -> ");
            WriteQubit(sb, end);
            sb.Append(System.Environment.NewLine);
        }
        return sb.ToString();
    }

}

}