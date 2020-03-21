using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using DotQasm.Scheduling;
using DotQasm.IO.Svg;
using DotQasm.Hardware;
using DotQasm.Search;

using PhysicalDataPrecedenceTable = System.Collections.Generic.List<System.Collections.Generic.List<DotQasm.Scheduling.DataPrecedenceNode>>;
using PhysicalDataPrecedenceRow = System.Collections.Generic.List<DotQasm.Scheduling.DataPrecedenceNode>;


namespace DotQasm.Optimization.Strategies {

/// <summary>
/// Hardware scheduling based on the algorithm provided by Gian Giacomo Guerreschi and Jongsoo Park
/// https://www.researchgate.net/publication/318849647_Gate_scheduling_for_quantum_algorithms
/// </summary>
public class HardwareScheduling : 
    IOptimization<LinearSchedule, LinearSchedule>, 
    IUsing<HardwareConfiguration>,
    IUsing<DotQasm.IO.PhysicalFile>
{

    public string Name => "Hardware Scheduling";

    public string Description => "Schedule gates for a given hardware configuration";

    private Svg GraphToSvg(LogicalDataPrecedenceGraph graph, int iterations = 100) {
        float width = 128;
        float height = 32;
        float buffer = 256;
        var rng = new Random();

        float maxMove = Math.Max(width, height);
        float springRestLength = maxMove + buffer;

        Svg svg = new Svg();

        // Define arrow heads
        MarkerDefintion triangle = new MarkerDefintion("triangle", new BoundingBox(0,0,10,10));
        triangle.RefX = 1;
        triangle.RefY = 3;
        triangle.MarkerUnits = MarkerUnits.strokeWidth;
        triangle.MarkerWidth = 6;
        triangle.MarkerHeight = 6;
        triangle.Orientation = MarkerOrientation.auto;
    
        triangle.MarkerShape = new Polygon(new Vector2[]{
            new Vector2(0, 0),
            new Vector2(6, 3),
            new Vector2(0, 6)
        });
        svg.Define(triangle);

        // Canvas size (plus a little bit of room to move)
        var vertexCount = graph.Vertices.Count();
        var canvasWidth = (vertexCount + 2) * width;
        var canvasHeight = (vertexCount + 2) * height;
        var canvasMidpointX = canvasWidth / 2;
        var canvasMidpointY = canvasHeight / 2;

        // Create boxes
        var vertices = new List<Rect>();
        var boxVertexMap = new Dictionary<int, DataPrecedenceNode>();
        var vertexBoxMap = new Dictionary<DataPrecedenceNode, int>();
        foreach (var vertex in graph.Vertices) {
            var rect = new Rect(new BoundingBox((float)rng.NextDouble() * canvasWidth, (vertices.Count + 1) * height, width, height));
            boxVertexMap[vertices.Count] = vertex;
            vertexBoxMap[vertex] = vertices.Count;
            vertices.Add(rect);
        }

        // http://stevehanov.ca/blog/?id=65
        /*
        float[] fxs = new float[vertices.Count];
        float[] fys = new float[vertices.Count];
        float timeStep = 1;
        for (int it = 0; it < iterations; it++) {
            // Repulsive forces
            for (int vind = 0; vind < vertices.Count; vind++) {
                // Initialize forces to 0
                fxs[vind] = 0;
                fys[vind] = 0;

                var v = vertices[vind];

                // For each other node, calculate repulsive forces
                for (var uind = 0; uind < vertices.Count; uind++) {
                    if (vind == uind)
                        continue; // Skip if same vertex

                    var u = vertices[uind];

                    var dx = v.Bounds.MidpointX - u.Bounds.MidpointX;
                    var dy = v.Bounds.MidpointY - u.Bounds.MidpointY;
                    var d = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (d == 0)
                        continue;
                    
                    var mul = springRestLength * springRestLength / (d * d * timeStep);
                    fxs[vind] += dx * mul;
                    fys[vind] += dy * mul;
                }
            }

            // Attractive forces
            foreach (var edge in graph.Edges) {
                var vi = vertexBoxMap[edge.Startpoint];
                var ui = vertexBoxMap[edge.Endpoint];

                var v = vertices[vi];
                var u = vertices[ui];

                var dx = v.Bounds.MidpointX - u.Bounds.MidpointX;
                var dy = v.Bounds.MidpointY - u.Bounds.MidpointY;
                var d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d == 0)
                    continue;

                var mul = d * d / springRestLength / timeStep;
                var dxmul = dx * mul;
                var dymul = dy * mul;

                fxs[vi] -= dxmul;
                fys[vi] -= dymul;

                fxs[ui] += dxmul;
                fys[ui] += dymul;
            }

            // Update positions
            for (int i = 0; i < vertices.Count; i++) {
                var v = vertices[i];
                
                var fx = (float.IsNaN(fxs[i]) ? 0 : fxs[i]);
                var fy = (float.IsNaN(fys[i]) ? 0 : fys[i]);
                var d = fx * fx + fy * fy;

                if (d > maxMove * maxMove) {
                    var D = (float)Math.Sqrt(d);
                    fx = fx * (maxMove) / D;
                    fy = fy * (maxMove) / D;
                }

                var oldX = v.Bounds.MinX;
                var oldY = v.Bounds.MinY;
                var newX = v.Bounds.MinX + fx;
                var newY = v.Bounds.MinY + fy;

                // Constrain size
                if (newX < 0)
                    newX = 0;
                if (newY < 0)
                    newY = 0;

                v.Resize(new BoundingBox(
                    newX,
                    newY,
                    v.Bounds.Width,
                    v.Bounds.Height
                ));
            }
        }*/

        // Create lines (marker-end="url(#triangle)")
        foreach (var edge in graph.Edges) {
            var rectStart = vertices[vertexBoxMap[edge.Startpoint]];
            var rectEnd = vertices[vertexBoxMap[edge.Endpoint]];

            var start = new Vector2(
                rectStart.Bounds.MidpointX, 
                rectStart.Bounds.MidpointY
            );

            var ax = start.X; // 6 is the size of the arrow so 3 is our buffer
            if (start.X < (rectEnd.Bounds.MinX - 3)) {
                ax = (rectEnd.Bounds.MinX - 3);
            } else if (start.X > (rectEnd.Bounds.MaxX + 3)) {
                ax = (rectEnd.Bounds.MaxX + 3);
            }

            var ay = start.Y;
            if (start.Y < (rectEnd.Bounds.MinY - 3)) {
                ay = (rectEnd.Bounds.MinY - 3);
            } else if (start.Y > (rectEnd.Bounds.MaxY + 3)) {
                ay = (rectEnd.Bounds.MaxY + 3);
            }

            var arrow = new Line(
                start,
                new Vector2(ax, ay)
            );
            arrow.EndMarker = "url(#triangle)";
            svg.Add(arrow);
        }

        // Add boxes to the svg
        foreach (var vertex in vertices) {
            svg.Add(vertex);
        }

        // Add text to boxes
        for (int i = 0; i < vertices.Count; i++) {
            var vertex = vertices[i];
            var evt = boxVertexMap[i];

            var text = new Text(new Vector2(vertex.Bounds.MidpointX, vertex.Bounds.MidpointY), "(" + evt.Priority + ") " + evt.Event.Name);
            text.HorizontalAnchor = HorizontalTextAnchor.middle;
            text.VerticalAnchor = VerticalTextAnchor.middle;
            svg.Add(text);
        }
        
        return svg;
    }

    private void Pad<T>(List<T> list, int pad) {
        while(list.Count < pad) {
            list.Add(default(T));
        }
    }

    private string Quote(object str) {
        return "\"" + (str?.ToString() ?? string.Empty) + "\"";
    }

    private HardwareConfiguration hardware;
    public void Use(HardwareConfiguration config) {
        if (config == null)
            throw new Exception(this.Name + " strategy requires a valid hardware configuration");
        this.hardware = config;
    }
    private DotQasm.IO.PhysicalFile srcFile;
    public void Use(DotQasm.IO.PhysicalFile source) {
        this.srcFile = source;
    }

    private void ComputeLatencies (LogicalDataPrecedenceGraph ldpg, ILatencyEstimator TimeEstimator) {
        foreach (var evt in ldpg.Vertices) {
            evt.Latency = TimeEstimator?.TimeOf(evt.Event) ?? TimeSpan.Zero;
        }
    }

    private void ComputePriorities (LogicalDataPrecedenceGraph ldpg) {
        /*
            Priorities can be eﬃciently computed by traversing the (directed and acyclic) graph in a post-order,
            starting from the gates in the last generation (pj=tj when nj ∈ leaves) and, 
            for each node, adding its latency to the maximum of its children’s priorities.
            
            pi = Max ( Sum(tj) for each in paths to node from leaf generation)
        */
        var visitor = new LambdaGraphVisitor<double, DataPrecedenceNode, DataPrecedenceEdgeData>();
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
        visitor.Traverse(ldpg);
    }

    private void ScheduleCopy(PhysicalDataPrecedenceTable pdpt, double priority, ControlledGateEvent ce, Qubit ctrl, Qubit target) {
        var evt = new ControlledGateEvent(ce.Operator, ctrl, new Qubit[]{ target });

        var node1 = new DataPrecedenceNode();
        node1.Event = evt;
        node1.Priority = priority;

        pdpt[ctrl.QubitId].Add(node1);
        pdpt[target.QubitId].Add(node1);
    }

    private int Swap(PhysicalDataPrecedenceTable pdpt, double priority, Qubit lhs, Qubit rhs) {
        var CX1 = new ControlledGateEvent(Gate.PauliX, lhs, new Qubit[]{ rhs });
        var CX2 = new ControlledGateEvent(Gate.PauliX, rhs, new Qubit[]{ lhs });
        var CX3 = new ControlledGateEvent(Gate.PauliX, lhs, new Qubit[]{ rhs });

        var node1 = new DataPrecedenceNode();
        node1.Event = CX1;
        node1.Priority = priority;

        var node2 = new DataPrecedenceNode();
        node2.Event = CX2;
        node2.Priority = priority;

        var node3 = new DataPrecedenceNode();
        node3.Event = CX3;
        node3.Priority = priority;

        pdpt[lhs.QubitId].Add(node1);
        pdpt[rhs.QubitId].Add(node1);

        pdpt[lhs.QubitId].Add(node2);
        pdpt[rhs.QubitId].Add(node2);

        pdpt[lhs.QubitId].Add(node3);
        pdpt[rhs.QubitId].Add(node3);

        return 3; // 3 operations added
    }

    public LinearSchedule Transform(LinearSchedule schedule) {
        // Obtain information from CLI
        ILatencyEstimator TimeEstimator = new BasicLatencyEstimator();

        // Create scheduling constructs
        var ldpg = new LogicalDataPrecedenceGraph(schedule);
        var pdpt = new PhysicalDataPrecedenceTable(); // Row, Column Format
        var qubitCount = 0; 
        List<Qubit> qubits = null;
        foreach (var evt in schedule) {
            foreach (var qubit in evt.QuantumDependencies) {
                if (qubits == null) {
                    var circuit = qubit.Owner.Owner;
                    qubits = circuit.Qubits.ToList();
                }
                qubitCount = Math.Max(qubitCount, qubit.QubitId);
            }
        }
        if (qubits == null) {
            qubits = new List<Qubit>();
        }
        for (int i = 0; i <= qubitCount; i++) {
            pdpt.Add(new PhysicalDataPrecedenceRow()); // Add row for each qubit in the schedule
        }

        // Step 1, arrange data in the logical data precedence graph, assign priorities along longest line
        // Page 6, tj is the latency for an event
        ComputeLatencies(ldpg, TimeEstimator);
        // Page 7. the depth of a node ni corresponds to the maximum number of nodes traversed along any directed path from ni to any gate in the last generation
        // Page 7, pi = Max ( Sum(tj) for each in paths to node from leaf generation)
        ComputePriorities(ldpg);
        
        // Step 2, schedule each event by priority, add routing if necessary
        // Page 7, No gate will ever depend on a gate with a lower priority so we can use a priority iterator to construct the physical data precedence  table
        // Ambiguity resolution & routing occur here
        foreach (var evt in ldpg.Vertices.OrderByDescending((vert) => vert.Priority)) {
            // Naively schedule it at the lowest depth possible
            // Get the depth
            var depth = 0;
            foreach (var qubit in evt.Event.QuantumDependencies) {
                depth = Math.Max(pdpt[qubit.QubitId].Count, depth);
            }

            // Check if there is a conflict with ambiguity
            var ambiguities = pdpt.Where((row) => row.Count == depth && row.Last().Priority == evt.Priority).Select(row => row.Last());
            
            // Resolve ambiguity

            // Pad the depth to the current level
            foreach (var qubit in evt.Event.QuantumDependencies) {
                Pad(pdpt[qubit.QubitId], depth);
            }

            // Perform routing if required
            if (hardware != null && evt.Event is ControlledGateEvent ce) {
                var startQubitId = ce.ControlQubit.QubitId;
                var startQubit = hardware.ConnectivityGraph.Vertices.ElementAt(startQubitId);

                foreach (var endQubitRef in ce.TargetQubits) {
                    // AStar search for the routing path (first node is the control, last is the target)
                    var endQubit = hardware.ConnectivityGraph.Vertices.ElementAt(endQubitRef.QubitId);
                    var path = AStarSearch.Search(
                        hardware.ConnectivityGraph,                             // The connectivity graph 
                        startQubit,                                             // Start at the control qubit
                        (vert) => vert == endQubit,                             // End is when we reach the end qubit
                        (edge) => 1,                                            // No edge weighting
                        (edge) => (Math.Abs(endQubitRef.QubitId - startQubitId))// Estimated "distance" between qubits
                    );
                    if (path == null) {
                        throw new Exception($"No path found between qubits {startQubitId} and {endQubitRef.QubitId} for the given hardware");
                    }

                    // Swap, Swap, Swap
                    var lhs = startQubitId;
                    foreach (var physicalSwapQubit in path.Skip(1)) {
                        if (endQubit == physicalSwapQubit)
                            break; // not the target qubit

                        var rhs = hardware.ConnectivityGraph.Vertices.IndexOf(physicalSwapQubit);
                        var firstQubit = qubits[lhs];
                        var secondQubit = qubits[rhs];

                        Pad(pdpt[lhs], depth);
                        Pad(pdpt[rhs], depth);
                        //System.Console.WriteLine($"SWAP FORWARD {firstQubit.QubitId} -> {secondQubit.QubitId} at {depth}");
                        depth += Swap(pdpt, evt.Priority, firstQubit, secondQubit);

                        lhs = rhs;
                    }

                    // Schedule instruction
                    Pad(pdpt[lhs], depth);
                    Pad(pdpt[endQubitRef.QubitId], depth);
                    ScheduleCopy(pdpt, evt.Priority, ce, qubits[lhs], endQubitRef);
                    depth++;

                    // Swap back, Swap back, Swap back
                    // If last event, and last swap, don't bother swapping back
                    if (!(evt == ldpg.Vertices.Last() && evt.Event.QuantumDependencies.Last() == endQubitRef)) {
                        foreach (var physicalSwapQubit in path.Reverse().Skip(2)) {                        
                            var rhs = hardware.ConnectivityGraph.Vertices.IndexOf(physicalSwapQubit);
                            var firstQubit = qubits[lhs];
                            var secondQubit = qubits[rhs];

                            Pad(pdpt[lhs], depth);
                            Pad(pdpt[rhs], depth);
                            //System.Console.WriteLine($"SWAP BACK {firstQubit.QubitId} -> {secondQubit.QubitId} at {depth}");
                            depth += Swap(pdpt, evt.Priority, firstQubit, secondQubit);
                        }
                    }
                }
            } 
            else {
                // Add event at level
                foreach (var qubit in evt.Event.QuantumDependencies) {
                    pdpt[qubit.QubitId].Add(evt);
                }
            }
        }

        // Step 3, output all data
        var now = DateTime.Now.ToString("dd/MM/yyyy H.mmtt");
        var name = srcFile?.Name ?? string.Empty;
        using (var writer = new StreamWriter(now + " - " + name + " - Logical Data Precedence Graph.svg")) {
            GraphToSvg(ldpg).Stringify(writer);
        }
        using (var writer = new StreamWriter(now + " - "  + name + " - Physical Data Precedence Table.csv")) {
            // Print Header
            var columns = pdpt.Select(row => row.Count).Max();
            writer.Write(Quote("Qubit Index"));
            for (int i = 1; i <= columns; i++) {
                writer.Write(",");
                writer.Write(Quote("Priority " + i));
            }
            writer.WriteLine();

            for (var i = 0; i < pdpt.Count; i++) {
                writer.Write(Quote(i));
                
                if (pdpt[i].Count > 0)
                    writer.Write(",");

                writer.WriteLine(
                    string.Join(
                        ',', 
                        pdpt[i].Select(
                            x => x == null ? string.Empty : Quote(x.Event.Name + " (" + x.Event.GetHashCode().ToString("X") + ")")
                        )
                    )
                );
            }
        }
        return schedule;
    }
}

}