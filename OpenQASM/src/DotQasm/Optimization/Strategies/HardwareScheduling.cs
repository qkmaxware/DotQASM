using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using DotQasm.Scheduling;
using DotQasm.IO.Svg;
using DotQasm.Hardware;
using DotQasm.Search;
using System.Collections;

namespace DotQasm.Optimization.Strategies {

class SwapSearchColouring : ISearchable {
    public Hardware.ConnectivityGraph graph;
    public Hardware.PhysicalQubit[] qubits;
    public int[] colours; 

    public (int, int)? lastSwap;
    public (PhysicalQubit, PhysicalQubit)? lastSwapQubits {
        get {
            if (this.lastSwap.HasValue) {
                return (qubits[lastSwap.Value.Item1], qubits[lastSwap.Value.Item2]);
            } else {
                return null;
            }
        }
    }

    private SwapSearchColouring(Hardware.ConnectivityGraph graph, Hardware.PhysicalQubit[] qubits, int[] colours, (int, int) swap) {
        // Copy parameters
        this.graph = graph;
        this.qubits = qubits;
        this.lastSwap = swap;
        this.colours = new int[colours.Length];
        for (int i = 0; i < this.colours.Length; i++) {
            this.colours[i] = colours[i];
        }
        
        // Swap colours
        var id1 = swap.Item1;
        var id2 = swap.Item2;
        var tmp = this.colours[id1];
        this.colours[id1] = this.colours[id2];
        this.colours[id2] = tmp;
    }
    
    public SwapSearchColouring(Hardware.ConnectivityGraph graph) {
        this.graph = graph;
        this.lastSwap = null;
        this.qubits = graph.Vertices.ToArray();
        this.colours = new int[this.qubits.Length];
        for (int i = 0; i < this.qubits.Length; i++) {
            this.colours[i] = this.qubits[i].Colour;
        }
    }

    public IEnumerable<ISearchable> Neighbours() {
        //Console.WriteLine(string.Join(',', this.colours) + " " + AreAllColoursAdjacent());
        foreach (var edge in graph.Edges) {
            // Get elements
            var inId = Array.IndexOf(this.qubits, edge.Startpoint);
            var outId = Array.IndexOf(this.qubits, edge.Endpoint);

            // Return next element
            if (inId != outId) {
                yield return new SwapSearchColouring(this.graph, this.qubits, this.colours, (inId, outId));
            }
        }
    }

    public bool AreAllColoursAdjacent() {
        var colourCount = this.colours.GroupBy(colour => colour).ToDictionary(grp => grp.Key, grp => grp.Count());
        for (var index = 0; index < this.colours.Length; index++) {
            var colour = this.colours[index];
            if (colour == default(int))
                continue; // uncolored

            var qubit = this.qubits[index];

            if (colourCount.ContainsKey(colour) && colourCount[colour] > 1) {
                // If others of this colour exist
                // This vertex is the start node of each edge
                var edges = this.graph.IncidentEdges(qubit);
                var connected = edges.Where(edge => edge.Endpoint.Colour == colour).Any();
                if (!connected) {
                    return false;
                }
            }
        }
        return true;
    }

    public int DistanceFromAdjacency() {
        var distance = 0;
        var colourCount = this.colours.GroupBy(colour => colour).ToDictionary(grp => grp.Key, grp => grp.Count());
        for (var index = 0; index < this.colours.Length; index++) {
            var colour = this.colours[index];
            if (colour == default(int))
                continue; // uncolored

            var qubit = this.qubits[index];

            if (colourCount.ContainsKey(colour) && colourCount[colour] > 1) {
                // If others of this colour exist
                // This vertex is the start node of each edge
                var edges = this.graph.IncidentEdges(qubit);
                var connected = edges.Where(edge => edge.Endpoint.Colour == colour).Any();
                if (!connected) {
                    distance++;
                }
            }
        }
        return distance;
    }

    public bool Equals(ISearchable other) {
        // Equal if they have the same colouring pattern
        return other switch {
            SwapSearchColouring ssc => this.colours.SequenceEqual(ssc.colours),
            _ => base.Equals(other)
        };
    }

    public override bool Equals(object obj) {
        return obj switch {
            ISearchable searchable => Equals(searchable),
            _ => base.Equals(obj)
        };
    }

    public override int GetHashCode() {
        return colours.Aggregate((a,b) => a ^ b); // not the greatest hash, but should work(tm)
    }
}

/// <summary>
/// Hardware scheduling inspired by the algorithm provided by Gian Giacomo Guerreschi and Jongsoo Park
/// https://www.researchgate.net/publication/318849647_Gate_scheduling_for_quantum_algorithms
/// </summary>
public class HardwareScheduling : 
    BaseOptimizationStrategy<LinearSchedule, LinearSchedule>, 
    IUsing<HardwareConfiguration>,
    IUsing<DotQasm.IO.PhysicalFile>
{

    public override string Name => "Hardware Scheduling";

    public override string Description => "Schedule gates for a given hardware configuration";

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
        var canvasWidth = (vertexCount / 2) * width;
        var canvasHeight = (vertexCount / 2) * height;
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

            var text = new Text(new Vector2(vertex.Bounds.MidpointX, vertex.Bounds.MidpointY), evt.Event.Name + " (" + evt.Event.GetHashCode().ToString("X") + ")");
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
        ldpg.RecalculateLatencies(TimeEstimator);
    }

    private void ComputePriorities (LogicalDataPrecedenceGraph ldpg) {
        ldpg.RecalculatePriorities();
    }

    private int Swap(PhysicalDataPrecedenceTable pdpt, double priority, Qubit lhs, Qubit rhs) {
        var swap = new SwapEvent(lhs, rhs);
        
        var node1 = new DataPrecedenceNode();
        node1.Event = swap;
        node1.Priority = priority;

        return 1; // 1 event added
        /*var CX1 = new ControlledGateEvent(Gate.PauliX, lhs, new Qubit[]{ rhs });
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

        return 3; // 3 operations added*/
    }

    private IEnumerable<IGrouping<double, DataPrecedenceNode>> GroupByPriority(IEnumerable<DataPrecedenceNode> vertices){
        return vertices.GroupBy((vert) => vert.Priority).OrderByDescending((group) => group.Key);
    }

    private bool DoAmbiguitiesExist(IGrouping<double, DataPrecedenceNode> group) {
        HashSet<Qubit> qubits = new HashSet<Qubit>();
        foreach (var node in group) {
            if (node.Event.QuantumDependencies != null) {
                foreach (var qubit in node.Event.QuantumDependencies) {
                    if (qubits.Contains(qubit)) {
                        return true;
                    } else {
                        qubits.Add(qubit);
                    }
                }
            }
        }
        return false;
    }

    private IEnumerable<IGrouping<double, DataPrecedenceNode>> SplitAmbiguousGroup(IGrouping<double, DataPrecedenceNode> group) {
        // Create interaction graph
        InteractionGraph ig = new InteractionGraph(group);
        // Assign colours to the interactions
        ig.AssignColours();
        System.Console.WriteLine(ig);
        // Create unambiguous groupings
        var unambiguous_groups = ig.Edges
            .Select(edge => edge.Data)
            .Distinct()
            // Group by the colour of the edge
            .GroupBy(data => data.Colour)
            // Convert edge coloured groups to groups of DataPrecendenceNodes
            .Select(group => {
                var events = group.Select(edge => new DataPrecedenceNode() { 
                    Event = edge.Event,
                    Priority = group.Key
                });
                return new BasicGrouping<double, DataPrecedenceNode>(group.Key, events);
            });
        // Return groups
        return unambiguous_groups;
    }

    private IEnumerable<IGrouping<double, DataPrecedenceNode>> ResolveAmbiguitiesToIterable(IEnumerable<IGrouping<double, DataPrecedenceNode>> iterable) {
        foreach (var group in iterable) {
            // detect ambiguities
            if (DoAmbiguitiesExist(group)) {
                // Console.WriteLine("Ambiguities exist");
                // resolve ambiguities
                foreach (var unambiguous_group in SplitAmbiguousGroup(group)) {
                    yield return unambiguous_group;
                }
            } else {
                yield return group;
            }
        }
    }

    private void RouteGroup(BijectiveDictionary<Qubit, PhysicalQubit> logicalQubitMap, PhysicalDataPrecedenceTable pdpt, IGrouping<double, DataPrecedenceNode> group) {
        // Reset colours
        foreach (var qubit in hardware.ConnectivityGraph.Vertices) {
            qubit.Colour = default(int);
        }

        // Create colours using current map
        int nextColour = 1;
        foreach (var vert in group) {
            // Generate colour and record qubits to colour
            var @event = vert.Event;
            var invovledQubits = @event.QuantumDependencies;
            var thisColour = nextColour++;

            // Assign colour to qubits 
            foreach (var logical in invovledQubits) {
                logicalQubitMap[logical].Colour = thisColour;
            }
        }

        // Create swaps to satisfy connectivity (Search?)
        var swapPath = AStarSearch.Search<SwapSearchColouring>(
            new SwapSearchColouring(hardware.ConnectivityGraph), 
            (node) => {         // End Condition
                return node.AreAllColoursAdjacent();
            },
            (from, to) => {     // Edge Distance Weighting
                return 1;       // 1 swap added
            },
            (node) => {         // Node Heuristic
                return node.DistanceFromAdjacency();
            }
        );
        if (swapPath == null) {
            throw new DataException<ConnectivityGraph>(
                "Algorithm failed to meet hardware connectivity constraints for group {" + string.Join(',', group.Select(x => x.Event.ToString() + "(" + x.Event.GetHashCode().ToString("X") + ")")) + "}", 
                hardware.ConnectivityGraph
            );
        }

        Func<(PhysicalQubit, PhysicalQubit), (PhysicalQubit, PhysicalQubit), bool> InterferesWith = (a, b) => {
            return a.Item1 == b.Item1 || a.Item1 == b.Item2 || a.Item2 == b.Item1 || a.Item2 == b.Item2; 
        };

        // Add swaps to the schedule, put those swaps into place on the qubit map
        var depth = pdpt.ColumnCount;
        var swapsAtPreviousDepth = new List<SwapSearchColouring>();
        foreach (var node in swapPath) {
            if (node.lastSwap.HasValue) {
                // swap the physical qubits associated with the logical qubits
                var swap = node.lastSwapQubits.Value;
                var logical1 = logicalQubitMap[swap.Item1];
                var logical2 = logicalQubitMap[swap.Item2];
                logicalQubitMap.Swap(swap.Item1, swap.Item2);
                
                // Create swap event
                var swapOp = new SwapEvent(logical1, logical2);
                var swapNode = new DataPrecedenceNode();
                swapNode.Event = swapOp;
                swapNode.Priority = group.Key;
                
                // If this swap doesn't interfer with previous swaps, can schedule it at the same depth as the last ones, otherwise add new depth
                var conflicts = swapsAtPreviousDepth.Where(prior => prior.lastSwap.HasValue ? InterferesWith(swap, prior.lastSwapQubits.Value) : false).Any();
                if (!conflicts && swapsAtPreviousDepth.Count > 0) {
                    depth --;
                } else {
                    swapsAtPreviousDepth.Clear();
                }

                // Add a physical swap to the pdpt as well
                Pad(pdpt[logical1], depth);
                Pad(pdpt[logical2], depth++);
                pdpt[logical1].Add((swap.Item1, swapNode));
                pdpt[logical2].Add((swap.Item2, swapNode));
                swapsAtPreviousDepth.Add(node);
            }
        }

        // Apply operations as none should conflict
        foreach (var vert in group) {
            foreach (var qubit in vert.Event.QuantumDependencies) {
                // Pad the depth to the current level
                Pad(pdpt[qubit], depth);

                // Add event at level
                pdpt[qubit].Add((logicalQubitMap[qubit], vert));
            }
        }
    }

    /// <summary>
    /// Transform the given schedule but keep references to the created data and files
    /// </summary>
    /// <param name="schedule">schedule to transform</param>
    /// <param name="save_ldpg">reference to the Logical Data Precedence Graph</param>
    /// <param name="saved_ldpg_filename">filename of the emitted Logical Data Precedence Graph on the hard-drive</param>
    /// <param name="save_pdpt">reference to the Physical Data Precedence Table</param>
    /// <param name="saved_pdpt_filename">filename of the emitted Physical Data Precedence Table on the hard-drive</param>
    /// <returns>transformed schedule</returns>
    public LinearSchedule Transform(LinearSchedule schedule, out LogicalDataPrecedenceGraph save_ldpg, out string saved_ldpg_filename, out PhysicalDataPrecedenceTable save_pdpt, out string saved_pdpt_filename) {
        // Obtain hardware reference
        if (hardware == null) {
            throw new ArgumentException(this.Name + " strategy requires a valid hardware configuration");
        }
        var logicalQubits = schedule.Events.SelectMany(x => x.QuantumDependencies).Distinct().ToList();
        var physicalQubits = hardware.ConnectivityGraph.Vertices.ToList();
        var qubitCount = hardware.PhysicalQubitCount;
        if (logicalQubits.Count > qubitCount) {
            throw new ArgumentOutOfRangeException("Number of logical qubits is greater than the number of physical qubits");
        }

        var now = DateTime.Now.ToString("yyyy/MM/dd H.mmtt");
        var filename = srcFile?.Name ?? string.Empty;

        // Obtain operation latencies (maybe get this from the hardware config?)
        ILatencyEstimator TimeEstimator = new IntegerLatencyEstimator();

        // Create scheduling constructs
        var ldpg = new LogicalDataPrecedenceGraph(schedule);
        var pdpt = new PhysicalDataPrecedenceTable(qubitCount); // Row, Column Format

        // Save these references
        save_ldpg = ldpg;
        save_pdpt = pdpt;

        // Fill initial logical qubit to physical qubit mapping
        // 1-1, logical maps directly to physical (better way of doing this would be nice)
        var logicalQubitMap = new BijectiveDictionary<Qubit, PhysicalQubit>(qubitCount);
        foreach (var logical in logicalQubits) {
            logicalQubitMap.Add(logical, physicalQubits.ElementAt(logical.QubitId));
        }

        // Step 1, arrange data in the logical data precedence graph, assign priorities along longest line
        // Page 6, tj is the latency for an event
        ComputeLatencies(ldpg, TimeEstimator);
        // Page 7. the depth of a node ni corresponds to the maximum number of nodes traversed along any directed path from ni to any gate in the last generation
        // Page 7, pi = Max ( Sum(tj) for each in paths to node from leaf generation)
        ComputePriorities(ldpg);
        saved_ldpg_filename = EmitLdpg(now, filename, ldpg);

        // Step 2, schedule each event by priority, add routing if necessary
        // Page 7, No gate will ever depend on a gate with a lower priority so we can use a priority iterator to construct the physical data precedence  table
        var groups = GroupByPriority(ldpg.Vertices);

        // Ambiguity resolution
        var unambiguous_groups = ResolveAmbiguitiesToIterable(groups);
        foreach (var unambiguous_group in unambiguous_groups) {
            // Routing
            RouteGroup(logicalQubitMap, pdpt, unambiguous_group);
        }

        // Step 3, output all data
        saved_pdpt_filename = EmitPdpt(now, filename, pdpt);
        return pdpt.ToLinearSchedule();
    }

    /// <summary>
    /// Transform the given schedule discarding all references to the created data and files 
    /// </summary>
    /// <param name="schedule">schedule to transform</param>
    /// <returns>transformed schedule</returns>
    public override LinearSchedule Transform(LinearSchedule schedule) {
        LogicalDataPrecedenceGraph ldpg;
        string ldpg_filename;
        PhysicalDataPrecedenceTable pdpt;
        string pdpt_filename;
        return Transform(schedule, out ldpg, out ldpg_filename, out pdpt, out pdpt_filename);
    }

    private string EmitLdpg(string timestamp, string filename, LogicalDataPrecedenceGraph ldpg) {
        // Emit logical data precedence graph
        var name = timestamp + " - " + filename + " - Logical Data Precedence Graph.csv";
        using (var writer = MakeDataFile(name)) {
            ldpg.Encode(writer);
        }
        return name;
    }

    private string EmitPdpt(string timestamp, string filename, PhysicalDataPrecedenceTable pdpt) {
        // Emit physical data precedence table
        var name = timestamp + " - " + filename + " - Physical Data Precedence Table.csv";
        using (var writer = MakeDataFile(name)) {
            pdpt.Encode(writer);
        }
        return name;
    }
}

}

/*
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
*/