using System.Linq;
namespace System.Collections.Generic {

/// <summary>
/// Graph visitor with swapable preorder and postorder actions as well as an accumulator for using child variables to effect values of parent variable
/// </summary>
/// <typeparam name="VertexType">graph vertex type</typeparam>
/// <typeparam name="EdgeType">graph edge type</typeparam>
public class AccumulatorGraphVisitor<RType, VertexType, EdgeType> {
    /// <summary>
    /// Action to run on vertex in preorder traversal (current vertex, last vertex) -> void
    /// </summary>
    public System.Action<VertexType, VertexType> PreorderAction {get; set;}

    /// <summary>
    /// Action to run on vertex in ostorder traversal (child accumulator, current vertex, last vertex) -> parent accumulator value
    /// </summary>
    public System.Func<RType, VertexType, VertexType, RType> PostorderAction {get; set;}

    /// <summary>
    /// Accumulator in the form of (old, delta) -> next
    /// </summary>
    public System.Func<RType, RType, RType> Accumulator {get; set;}

    private RType Visit(IGraph<VertexType, EdgeType> graph, VertexType current, VertexType last) {
        // Pre (before)
        if (PreorderAction != null)
            PreorderAction(current, last);

        // Traverse sub-graph
        var next = graph.IncidentEdges(current);
        var accumulator = default(RType);
        foreach (var edge in next) {
           var result = Visit(graph, edge.Endpoint, current);
           accumulator = Accumulator(accumulator, result);
        }

        // Post (after)
        if (PostorderAction != null)
            return PostorderAction(accumulator, current, last);
        else
            return default(RType);
    }

    /// <summary>
    /// Traverse a given graph starting at the root nodes
    /// </summary>
    /// <param name="graph">graph to traverse</param>
    public void Traverse(IGraph<VertexType, EdgeType> graph) {
        var nextNodes = graph.RootNodes;

        foreach (var node in nextNodes) {
            Visit (graph, node, default(VertexType));
        }
    }
}

public class GraphVisitor<VertexType, EdgeType> {
    public System.Action<VertexType> OnVisitVertex;
    public System.Action<IGraphEdge<VertexType, EdgeType>> OnVisitEdge;
}

/// <summary>
/// Enumerator that traverses a graph breadth first
/// </summary>
public class BreadthFirstEnumerator<VertexType, EdgeType>: IEnumerable<VertexType> {

    private VertexType startVertex;
    private IGraph<VertexType, EdgeType> graph;

    public BreadthFirstEnumerator(VertexType start, IGraph<VertexType, EdgeType> graph) {
        this.startVertex = start;
        this.graph = graph;
    }

    public IEnumerator<VertexType> GetEnumerator() {
        Queue<VertexType> stack = new Queue<VertexType>();
        HashSet<VertexType> visited = new HashSet<VertexType>();

        stack.Enqueue(startVertex);

        while (stack.Count > 0) {
            var node = stack.Dequeue();
            visited.Add(node);
            yield return node;

            foreach (var edge in graph.IncidentEdges(node)) {
                if (!visited.Contains(edge.Endpoint)) { // prevents infinite loops
                    stack.Enqueue(edge.Endpoint);
                }
            }   
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

public class BreadthFirstWalker<VertexType, EdgeType> {

    private VertexType startVertex;
    private IGraph<VertexType, EdgeType> graph;

    public BreadthFirstWalker(VertexType start, IGraph<VertexType, EdgeType> graph) {
        this.startVertex = start;
        this.graph = graph;
    }

    public void Traverse(GraphVisitor<VertexType, EdgeType> visitor) {
        Queue<VertexType> stack = new Queue<VertexType>();
        HashSet<VertexType> visitedNodes = new HashSet<VertexType>();
        HashSet<IGraphEdge<VertexType, EdgeType>> visitedEdges = new HashSet<IGraphEdge<VertexType, EdgeType>>();

        stack.Enqueue(startVertex);
        visitedNodes.Add(startVertex);

        while (stack.Count > 0) {
            var node = stack.Dequeue();
            if (visitor?.OnVisitVertex != null) {
                visitor.OnVisitVertex(node);
            }

            foreach (var edge in graph.IncidentEdges(node)) {
                if (!visitedEdges.Contains(edge)) {
                    visitedEdges.Add(edge);
                    if (visitor?.OnVisitEdge != null) {
                        visitor.OnVisitEdge(edge);
                    }
                    if (!visitedNodes.Contains(edge.Endpoint)) {
                        stack.Enqueue(edge.Endpoint);
                        visitedNodes.Add(edge.Endpoint);
                    }
                }
            }   
        }
    }
}

}