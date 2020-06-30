using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Estimate the time of a schedule using basic timings
/// </summary>
public class BasicTimeEstimator : BasicLatencyEstimator {

    private IGraph<IEvent,IWeightedEdgeData> graph;

    public BasicTimeEstimator(IGraph<IEvent,IWeightedEdgeData> graph) {
        this.graph = graph;
    }
}

}