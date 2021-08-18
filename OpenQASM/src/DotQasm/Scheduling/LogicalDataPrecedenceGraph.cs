using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DotQasm;
using DotQasm.Scheduling;

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
public class DataPrecedenceNode : IAttributedVertex {
    // Computed by LDPG
    public int EventIndex {get; set;}
    public IEvent Event {get; set;}
    public int Depth {get; set;}
    public int DependencyCount {get; set;}

    // Computed manually
    public TimeSpan Latency {get; set;}
    public double? Priority {get; set;}

    public override string ToString() => "event_" + EventIndex + "_" + Depth;
    public IEnumerable<KeyValuePair<string,string>> GetAttributes() {
        yield return new KeyValuePair<string, string>("EventIndex", this.EventIndex.ToString());
        yield return new KeyValuePair<string, string>("EventType", Event.GetType().ToString());
        yield return new KeyValuePair<string, string>("Depth", this.Depth.ToString());
        yield return new KeyValuePair<string, string>("Dependencies", this.DependencyCount.ToString());
        yield return new KeyValuePair<string, string>("Latency", this.Latency.ToString());
        yield return new KeyValuePair<string, string>("Priority", this.Priority.ToString());
    }

}

/// <summary>
/// Graph representing logical data precedences
/// </summary>
public class LogicalDataPrecedenceGraph: EdgeListGraph<DataPrecedenceNode, DataPrecedenceEdgeData> {

    public LogicalDataPrecedenceGraph () {}
    public LogicalDataPrecedenceGraph (LinearSchedule events) {
        this.AddEventsToGraph(events);
        this.AddEdgesBasedOnEventDependencies();
    }

    private void AddEventsToGraph(IEnumerable<IEvent> events) {
        int index = this.VertexCount;
        foreach (var evt in events) {
            // Add self to the graph
            var node = new DataPrecedenceNode();
            node.Event = evt;
            node.EventIndex = index++;
            this.Add(node);
        }
    }

    private void AddEdgesBasedOnEventDependencies() {
        Dictionary<object, IEnumerable<IEvent>> resourceUsageMap = new Dictionary<object, IEnumerable<IEvent>>();
        //Dictionary<Qubit, IEnumerable<IEvent>> quantumLastUsageMap = new Dictionary<Qubit, IEnumerable<IEvent>>();
        //Dictionary<Cbit, IEnumerable<IEvent>> classicalLastUsageMap = new Dictionary<Cbit, IEnumerable<IEvent>>();

        foreach (var node in this.Vertices) {
            // Get potential dependencies 
            HashSet<IEvent> potentialDependencies = new HashSet<IEvent>();
            if (node.Event.QuantumDependencies != null) {
                // foreach (var qs in node.Event.QuantumDependencies) {
                //     if (quantumLastUsageMap.ContainsKey(qs)) {
                //         potentialDependencies.UnionWith(quantumLastUsageMap[qs]);
                //     }
                // }
                foreach (var qs in node.Event.QuantumDependencies) {
                    if (resourceUsageMap.ContainsKey(qs)) {
                        potentialDependencies.UnionWith(resourceUsageMap[qs]);
                    }
                }
            }
            if (node.Event.ClassicalDependencies != null) {
                // foreach (var cs in node.Event.ClassicalDependencies) {
                //     if (classicalLastUsageMap.ContainsKey(cs)) {
                //         potentialDependencies.UnionWith(classicalLastUsageMap[cs]);
                //     }
                // }
                foreach (var cs in node.Event.ClassicalDependencies) {
                    if (resourceUsageMap.ContainsKey(cs)) {
                        potentialDependencies.UnionWith(resourceUsageMap[cs]);
                    }
                }
            }

            // Group into commuting and not commuting
            var split = potentialDependencies.ToLookup((evt) => this.CommutesWith(evt, node.Event));
            var commuting = split[true];
            var not_commuting = split[false];

            // Add self as last value to using the provided qubits and cbits
            var new_dependencies = commuting.Append(node.Event);
            var resource_dependency_lists = new Dictionary<object, HashSet<IEvent>>();
            foreach (var evt in new_dependencies) { // Sort dependencies by the resources they use
                if (node.Event.QuantumDependencies != null) {
                    foreach (var qs in node.Event.QuantumDependencies) {
                        if (resource_dependency_lists.ContainsKey(qs)) {
                            resource_dependency_lists[qs].Add(evt);
                        } else {
                            resource_dependency_lists[qs] = new HashSet<IEvent>(){ evt };
                        }
                    }
                }
                if (node.Event.ClassicalDependencies != null) {
                    foreach (var cs in node.Event.ClassicalDependencies) {
                        if (resource_dependency_lists.ContainsKey(cs)) {
                            resource_dependency_lists[cs].Add(evt);
                        } else {
                            resource_dependency_lists[cs] = new HashSet<IEvent>(){ evt };
                        }
                    }
                }
            }
            foreach (var (resource, list) in resource_dependency_lists) {
                resourceUsageMap[resource] = list;
            }
            
            // if (node.Event.QuantumDependencies != null) {
            //     foreach (var qs in node.Event.QuantumDependencies) {
            //         quantumLastUsageMap[qs] = node.Event;
            //     }
            // }
            // if (node.Event.ClassicalDependencies != null) {
            //     foreach (var cs in node.Event.ClassicalDependencies) {
            //         classicalLastUsageMap[cs] = node.Event;
            //     }
            // }

            // Add connections from dependencies (non-commuting) to self (which direction?)
            var depth = 0;
            foreach (var dependency in not_commuting) {
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

    /// <summary>
    /// Calculate the latencies of all events using the given latency estimator
    /// </summary>
    /// <param name="TimeEstimator"></param>
    public void RecalculateLatencies(ILatencyEstimator TimeEstimator) {
        foreach (var evt in this.Vertices) {
            evt.Latency = TimeEstimator?.TimeOf(evt.Event) ?? TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Compute the priorities of all events
    /// </summary>
    public void RecalculatePriorities() {
        /*
            Priorities can be eﬃciently computed by traversing the (directed and acyclic) graph in a post-order,
            starting from the gates in the last generation (pj=tj when nj ∈ leaves) and, 
            for each node, adding its latency to the maximum of its children’s priorities.
            
            pi = Max ( Sum(tj) for each in paths to node from leaf generation)
        */
        /*var visitor = new AccumulatorGraphVisitor<double, DataPrecedenceNode, DataPrecedenceEdgeData>();
        visitor.Accumulator = (old, @new) => {
            return Math.Max(old, @new);
        };
        visitor.PostorderAction = (accumulator, current, last) => {
            double pi = current.Latency.TotalMilliseconds;
            pi += accumulator; // accumulator is the max of the child PI's 

            if (pi > current.Priority) {
                current.Priority = pi;
            }

            return current.Priority;
        };
        visitor.Traverse(this);*/
        
        var flattenedTree = this.Vertices.OrderByDescending(vert => vert.Depth).ToList();
        foreach (var node in flattenedTree) {
            recursivelyComputePI(node); // Should almost never recurse because nodes at lower depth are already computed
        }

        
       // var roots = this.RootNodes.ToList();
        //foreach (var node in roots) {
          //  recursivelyComputePI(node);
        //}
    }

    private void recursivelyComputePI(DataPrecedenceNode node) {
        if (node.Priority.HasValue)
            return;

        var child_max = 0.0d;
        foreach (var link in this.IncidentEdges(node)) {
            recursivelyComputePI(link.Endpoint);
            child_max = Math.Max(child_max, link.Endpoint.Priority.Value);
        }
        node.Priority = child_max + node.Latency.TotalSeconds;
    }

    private bool CommutesWith(IEvent evt1, IEvent evt2) {
        if (evt1 is GateEvent && evt2 is GateEvent) {
            return ((GateEvent)evt1).Operator.CommutesWith(((GateEvent)evt2).Operator);
        } else {
            return false;
        }
    }

    private string Quote(object str) {
        return "\"" + (str?.ToString() ?? string.Empty) + "\"";
    }

    /// <summary>
    /// Write the graph as a CSV to the given text writer
    /// </summary>
    /// <param name="writer">writer to write to</param>
    public void Encode(TextWriter writer) {
        writer.Write(Quote("Operation Index"));
        writer.Write(",");
        writer.Write(Quote("Operation Name"));
        writer.Write(",");
        writer.Write(Quote("Latency"));
        writer.Write(",");
        writer.WriteLine(Quote("Priority"));
        
        foreach (var (index, evt) in this.Vertices.Select((evt, index) => (index, evt))) {
            writer.Write(index);
            writer.Write(",");
            writer.Write(Quote(evt.Event.Name + " (" + evt.Event.GetHashCode().ToString("X") + ")"));
            writer.Write(",");
            writer.Write(Quote(evt.Latency));
            writer.Write(",");
            writer.WriteLine(Quote(evt.Priority));
        }
    }
}

}