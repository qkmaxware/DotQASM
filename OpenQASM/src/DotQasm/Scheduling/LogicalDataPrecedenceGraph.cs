using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Edge data for logical data precedence graphs
/// </summary>
public class DataPrecedenceEdgeData: IWeightedEdgeData {
    public double Weight => 0;
}

/// <summary>
/// Vertex data for logical data precedence graphs
/// </summary>
public class DataPrecedenceNode {
    // Computed by LDPG
    public int EventIndex {get; set;}
    public IEvent Event {get; set;}
    public int Depth {get; set;}
    public int DependencyCount {get; set;}

    // Computed manually
    public TimeSpan Latency {get; set;}
    public double Priority {get; set;}

}

/// <summary>
/// Graph representing logical data precedences
/// </summary>
public class LogicalDataPrecedenceGraph: EdgeListGraph<DataPrecedenceNode, DataPrecedenceEdgeData> {

    public LogicalDataPrecedenceGraph () {}
    public LogicalDataPrecedenceGraph (LinearSchedule events) {
        Dictionary<Qubit, IEvent> quantumLastUsageMap = new Dictionary<Qubit, IEvent>();
        Dictionary<Cbit, IEvent> classicalLastUsageMap = new Dictionary<Cbit, IEvent>();

        int index = 0;
        foreach (var evt in events) {
            // Add self
            var node = new DataPrecedenceNode();
            node.Event = evt;
            node.EventIndex = index++;
            this.Add(node);

            // Get dependencies 
            HashSet<IEvent> deps = new HashSet<IEvent>();
            if (evt.QuantumDependencies != null) {
                foreach (var qs in evt.QuantumDependencies) {
                    if (quantumLastUsageMap.ContainsKey(qs)) {
                        deps.Add(quantumLastUsageMap[qs]);
                    }
                }
            }
            if (evt.ClassicalDependencies != null){
                foreach (var cs in evt.ClassicalDependencies) {
                    if (classicalLastUsageMap.ContainsKey(cs)) {
                        deps.Add(classicalLastUsageMap[cs]);
                    }
                }
            }

            // Add self as last value to using the provided qubits and cbits
            if (evt.QuantumDependencies != null) {
                foreach (var qs in evt.QuantumDependencies) {
                    quantumLastUsageMap[qs] = evt;
                }
            }
            if (evt.ClassicalDependencies != null) {
                foreach (var cs in evt.ClassicalDependencies) {
                    classicalLastUsageMap[cs] = evt;
                }
            }

            // Add connections from dependencies to self (which direction?)
            var depth = 0;
            foreach (var dependency in deps) {
                var depNode = this.Vertices.Where(vert => vert.Event?.Equals(dependency) ?? false).FirstOrDefault();
                if (depNode != null) {
                    this.DirectedEdge(depNode, node, new DataPrecedenceEdgeData());
                    node.DependencyCount += depNode.DependencyCount + 1; // All the dependencies of that node, plus the node itself
                    depth = Math.Max(depth, depNode.Depth);
                }
            }
            node.Depth = depth + 1;
        }
    }
}

}