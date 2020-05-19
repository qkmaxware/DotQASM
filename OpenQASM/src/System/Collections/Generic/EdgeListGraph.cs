using System.Linq;

namespace System.Collections.Generic {

/// <summary>
/// Graph data structure represented by an edge list
/// </summary>
/// <typeparam name="VertexType">Type of data stored in each vertex</typeparam>
/// <typeparam name="EdgeType">Type of data stored in each edge</typeparam>
public class EdgeListGraph<VertexType, EdgeType> : IGraph<VertexType, EdgeType> {

    private List<VertexType> _vertices = new List<VertexType>();
    private List<List<IGraphEdge<VertexType,EdgeType>>> _edges = new List<List<IGraphEdge<VertexType,EdgeType>>>();

    public IEnumerable<VertexType> Vertices => _vertices.AsReadOnly();
    public IEnumerable<IGraphEdge<VertexType,EdgeType>> Edges => _edges.SelectMany(x => x);

    /// <summary>
    /// List of all leaf nodes in the graph. Leaf nodes do not have any outbound edges
    /// </summary>
    public IEnumerable<VertexType> LeafNodes => this.Vertices.Where(
        vert => !this.Edges.Where(
            edge => edge.Startpoint?.Equals(vert) ?? false
        ).Any()
    );

    /// <summary>
    /// List of all root nodes in the graph. Root nodes do not have any inbound edges
    /// </summary>
    public IEnumerable<VertexType> RootNodes => this.Vertices.Where(
        vert => !this.Edges.Where(
            edge => edge.Endpoint?.Equals(vert) ?? false
        ).Any()
    );

    /// <summary>
    /// Number of vertices in the graph
    /// </summary>
    public int VertexCount => _vertices.Count;

    public bool Contains(VertexType vertex) {
        return _vertices.IndexOf(vertex) >= 0;
    }

    public bool Contains(EdgeType edge) {
        return _edges.SelectMany(x => x).Where(edge => edge.Data?.Equals(edge) ?? false).Any();
    }

    public IEnumerable<IGraphEdge<VertexType,EdgeType>> IncidentEdges(VertexType vertex) {
        var index = _vertices.IndexOf(vertex);
        if (index >= 0) {
            return _edges[index];
        } else {
            throw new ArgumentException(paramName: "vertex", message: "Vertex does not exist within graph");
        }
    }

    public bool AreAdjacent(VertexType a, VertexType b) {
        var ai = _vertices.IndexOf(a);
        if (ai < 0) { throw new ArgumentException(paramName: "a", message: "Vertex does not exist within graph"); }

        return _edges[ai].Where((edge) => edge.Endpoint?.Equals(b) ?? false).Any();
    }

    public void Replace(VertexType old, VertexType @new) {
        var ai = _vertices.IndexOf(old);
        if (ai < 0) { throw new ArgumentException(paramName: "old", message: "Vertex does not exist within graph"); }

        _vertices[ai] = @new;
    }

    public void Replace(EdgeType old, EdgeType @new) {
        var edges = _edges.SelectMany(x => x).Where(edge => edge.Data?.Equals(@old) ?? false);
        foreach (var edge in edges) {
            edge.Data = @new;
        }
    }

    public void Add(VertexType vertex) {
        _vertices.Add(vertex);
        _edges.Add(new List<IGraphEdge<VertexType, EdgeType>>());
    }

    public void Remove(VertexType vertex) {
        var ai = _vertices.IndexOf(vertex);
        if (ai < 0) { throw new ArgumentException(paramName: "vertex", message: "Vertex does not exist within graph"); }

        _vertices.RemoveAt(ai);
        _edges.RemoveAt(ai);
    }

    public void Remove(EdgeType type) {
        foreach (var collection in _edges) {
            collection.RemoveAll(edge => edge.Data?.Equals(type) ?? false);
        }
    }

    public IGraphEdge<VertexType,EdgeType> DirectedEdge(VertexType from, VertexType to, EdgeType data) {
        var ai = _vertices.IndexOf(from);
        if (ai < 0) { throw new ArgumentException(paramName: "from", message: "Vertex does not exist within graph"); }

        var bi = _vertices.IndexOf(to);
        if (bi < 0) { throw new ArgumentException(paramName: "to", message: "Vertex does not exist within graph"); }

        var edge = new AbstractGraphEdge<VertexType, EdgeType>(data, from, to);
        _edges[ai].Add(edge);

        return edge;
    }

    public IGraphEdge<VertexType,EdgeType> DirectedEdge (int from, int to, EdgeType data) {
        var edge = new AbstractGraphEdge<VertexType, EdgeType>(data, _vertices[from], _vertices[to]);
        _edges[from].Add(edge);

        return edge;
    }

    public (IGraphEdge<VertexType,EdgeType>, IGraphEdge<VertexType,EdgeType>) UndirectedEdge(VertexType a, VertexType b, EdgeType data) {
        var ai = _vertices.IndexOf(a);
        if (ai < 0) { throw new ArgumentException(paramName: "a", message: "Vertex does not exist within graph"); }

        var bi = _vertices.IndexOf(b);
        if (bi < 0) { throw new ArgumentException(paramName: "b", message: "Vertex does not exist within graph"); }


        var edgeAB = new AbstractGraphEdge<VertexType, EdgeType>(data, a, b);
        var edgeBA = new AbstractGraphEdge<VertexType, EdgeType>(data, b, a);
        _edges[ai].Add(edgeAB);
        _edges[bi].Add(edgeBA);

        return (edgeAB, edgeBA);
    }

    public (IGraphEdge<VertexType,EdgeType>, IGraphEdge<VertexType,EdgeType>) UndirectedEdge (int a, int b, EdgeType data) {
        var edgeAB = new AbstractGraphEdge<VertexType, EdgeType>(data, _vertices[a], _vertices[b]);
        _edges[a].Add(edgeAB);

        var edgeBA = new AbstractGraphEdge<VertexType, EdgeType>(data, _vertices[b], _vertices[a]);
        _edges[b].Add(edgeBA);

        return (edgeAB, edgeBA);
    }
}

}