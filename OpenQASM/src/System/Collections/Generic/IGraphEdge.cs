namespace System.Collections.Generic {

/// <summary>
/// Edge connecting vertices
/// </summary>
/// <typeparam name="VertexType">Type of data stored in each vertex</typeparam>
/// <typeparam name="EdgeType">Type of data stored in each edge</typeparam>
public interface IGraphEdge<VertexType, EdgeType> {
    /// <summary>
    /// Vertex at the start of the edge connection
    /// </summary>
    /// <value></value>
    VertexType Startpoint {get;}
    /// <summary>
    /// Stored edge data
    /// </summary>
    EdgeType Data {get; set;}
    /// <summary>
    /// Vertex at the end of the edge connection
    /// </summary>
    VertexType Endpoint {get;}
}

/// <summary>
/// Abstract class representing an graph edge
/// </summary>
/// <typeparam name="VertexType">Type of data stored in each vertex</typeparam>
/// <typeparam name="EdgeType">Type of data stored in each edge</typeparam>
public class AbstractGraphEdge<VertexType, EdgeType> : IGraphEdge<VertexType, EdgeType> {
    /// <summary>
    /// Vertex at the start of the edge connection
    /// </summary>
    /// <value></value>
    public VertexType Startpoint {get;}
    /// <summary>
    /// Stored edge data
    /// </summary>
    public EdgeType Data {get; set;}
    /// <summary>
    /// Vertex at the end of the edge connection
    /// </summary>
    public VertexType Endpoint {get;}
    /// <summary>
    /// Edge with no data
    /// </summary>
    /// <param name="endpoint">edge endpoint</param>
    /// <param name="startpoint">edge startpoint</param>
    public AbstractGraphEdge(VertexType startpoint, VertexType endpoint) {
        this.Data = default;
        this.Endpoint = endpoint;
        this.Startpoint = startpoint;
    }
    /// <summary>
    /// Edge with data
    /// </summary>
    /// <param name="data">stored edge data</param>
    /// <param name="endpoint">edge endpoint</param>
    /// <param name="startpoint">edge startpoint</param>
    public AbstractGraphEdge(EdgeType data, VertexType startpoint, VertexType endpoint) {
        this.Data = data;
        this.Endpoint = endpoint;
        this.Startpoint = startpoint;
    }
}

}