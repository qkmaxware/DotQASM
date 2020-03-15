using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using DotQasm.Scheduling;
using DotQasm.IO.Svg;
using DotQasm.Hardware;

namespace DotQasm.Optimization.Strategies {

/// <summary>
/// Hardware scheduling based on the algorithm provided by Gian Giacomo Guerreschi and Jongsoo Park
/// https://www.researchgate.net/publication/318849647_Gate_scheduling_for_quantum_algorithms
/// </summary>
public class HardwareScheduling : IOptimization<LinearSchedule, LinearSchedule>, IUsing<HardwareConfiguration> {

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

    private HardwareConfiguration hardware;
    public void Use(HardwareConfiguration config) {
        if (config == null)
            throw new Exception(this.Name + " strategy requires a valid hardware configuration");
        this.hardware = config;
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

    private void RPad<T>(List<T> list, int pad) {
        while(list.Count < pad) {
            list.Add(default(T));
        }
    }

    private string Quote(object str) {
        return "\"" + (str?.ToString() ?? string.Empty) + "\"";
    }

    public LinearSchedule Transform(LinearSchedule schedule) {
        // Obtain information from CLI
        ILatencyEstimator TimeEstimator = new BasicLatencyEstimator();

        // Create scheduling constructs
        var ldpg = new LogicalDataPrecedenceGraph(schedule);
        var pdpt = new List<List<DataPrecedenceNode>>(); // Row, Column Format
        var qubitCount = schedule.Select(evt => evt.QuantumDependencies.Select(qubit => qubit.QubitId).Max()).Max();
        for (int i = 0; i <= qubitCount; i++) {
            pdpt.Add(new List<DataPrecedenceNode >()); // Add row for each qubit in the schedule
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
            // Pad the depth to the current level
            foreach (var qubit in evt.Event.QuantumDependencies) {
                RPad(pdpt[qubit.QubitId], depth);
            }
            // Add event at level
            foreach (var qubit in evt.Event.QuantumDependencies) {
                pdpt[qubit.QubitId].Add(evt);
            }

            // Check if there is a conflict with ambiguity
            // Resolve ambiguity
            // Perform routing if required
        }

        // Step 3, output all data
        var now = DateTime.Now.ToString("dd/MM/yyyy H.mmtt");
        using (var writer = new StreamWriter(now + " - Logical Data Precedence Graph.svg")) {
            GraphToSvg(ldpg).Stringify(writer);
        }
        using (var writer = new StreamWriter(now + " - Physical Data Precedence Table.csv")) {
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
                            x => x == null ? string.Empty : Quote(x.Event.Name)
                        )
                    )
                );
            }
        }
        return schedule;
    }
}

}