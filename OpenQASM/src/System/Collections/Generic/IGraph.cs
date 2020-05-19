using System.Linq;

namespace System.Collections.Generic {

/// <summary>
/// Interface for graph based data structures
/// </summary>
/// <typeparam name="VertexType">Type of data stored in each vertex</typeparam>
/// <typeparam name="EdgeType">Type of data stored in each edge</typeparam>
public interface IGraph<VertexType, EdgeType> {
    /// <summary>
    /// List of all root nodes in the graph. Root nodes do not have any inbound edges
    /// </summary>
    IEnumerable<VertexType> RootNodes {get;}
    /// <summary>
    /// List of all leaf nodes in the graph. Leaf nodes do not have any outbound edges
    /// </summary>
    IEnumerable<VertexType> LeafNodes {get;}

    /// <summary>
    /// Collection of vertices in the graph
    /// </summary>
    IEnumerable<VertexType> Vertices {get;}
    
    /// <summary>
    /// Number of vertices in the graph
    /// </summary>
    /// <returns>number of vertices</returns>
    int VertexCount => Vertices.Count(); // Default implementation, meant to be overwritten by implementors
     
    /// <summary>
    /// Collection of vertices in the graph
    /// </summary>
    IEnumerable<IGraphEdge<VertexType,EdgeType>> Edges {get;}
    /// <summary>
    /// Check if the graph contains the given vertex
    /// </summary>
    /// <param name="vertex">vertex to check</param>
    /// <returns>true if vertex exists in the graph</returns>
    bool Contains(VertexType vertex);
    /// <summary>
    /// Get all edges incident to the given vertex
    /// </summary>
    /// <param name="vertex">vertex</param>
    /// <returns>collection of edges</returns>
    IEnumerable<IGraphEdge<VertexType,EdgeType>> IncidentEdges(VertexType vertex);
    /// <summary>
    /// Test if two vertices are adjacent to each other
    /// </summary>
    /// <param name="a">first vertex</param>
    /// <param name="b">second vertex</param>
    /// <returns>true if an edge exists from a to b</returns>
    bool AreAdjacent(VertexType a, VertexType b);
    /// <summary>
    /// Replace the element stored at a given vertex
    /// </summary>
    /// <param name="old">vertex to replace</param>
    /// <param name="@new">vertex to store</param>
    void Replace(VertexType old, VertexType @new);
    /// <summary>
    /// Replace the edge data stored at the given vertex
    /// </summary>
    /// <param name="old">edge to replace</param>
    /// <param name="@new">vertex to store</param>
    void Replace(EdgeType old, EdgeType @new);
    /// <summary>
    /// Add a vertex to the graph
    /// </summary>
    /// <param name="vertex">vertex to add</param>
    void Add(VertexType vertex);
    /// <summary>
    /// Remove a vertex from the graph
    /// </summary>
    /// <param name="vertex">vertex to remove</param>
    void Remove(VertexType vertex);
    /// <summary>
    /// Remove a given edge
    /// </summary>
    /// <param name="type">edge to remove</param>
    void Remove(EdgeType type);
    /// <summary>
    /// Create a directed edge between vertices
    /// </summary>
    /// <param name="from">starting vertex</param>
    /// <param name="to">ending vertex</param>
    /// <param name="data">edge data</param>
    /// <returns>returns the directed edge</returns>
    IGraphEdge<VertexType,EdgeType> DirectedEdge(VertexType from, VertexType to, EdgeType data);
    /// <summary>
    /// Create a undirected edge between vertices
    /// </summary>
    /// <param name="a">first vertex</param>
    /// <param name="b">second vertex</param>
    /// <param name="data">edge data</param>
    /// <returns>returns two directed edges which form the undirected edge</returns>
    (IGraphEdge<VertexType,EdgeType>, IGraphEdge<VertexType,EdgeType>) UndirectedEdge(VertexType a, VertexType b, EdgeType data);
}

/// <summary>
/// Interface for graph based data structures which contain attributes
/// </summary>
/// <typeparam name="VertexType">Type of data stored in each vertex</typeparam>
/// <typeparam name="EdgeType">Type of data stored in each edge</typeparam>
public interface IAttributedGraph<VertexType, EdgeType> : IGraph<VertexType, EdgeType> { 
    Dictionary<string, string> Attributes {get;}
}

}