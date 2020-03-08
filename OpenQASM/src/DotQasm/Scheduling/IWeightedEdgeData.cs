using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Interface representing graph edge data that includes edge weights
/// </summary>
public interface IWeightedEdgeData {
    /// <summary>
    /// Weight of the edge
    /// </summary>
    double Weight {get;}
}

}