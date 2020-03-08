using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Estimate the time of a schedule using basic timings
/// </summary>
public class BasicTimeEstimator : BasicLatencyEstimator, ITimeEstimator {

    private IGraph<IEvent,IWeightedEdgeData> graph;

    public BasicTimeEstimator(IGraph<IEvent,IWeightedEdgeData> graph) {
        this.graph = graph;
    }

    public TimeSpan? TimeBetween (IEvent start, IEvent end) {
        return DotQasm.Search.AStarSearch.Search<IEvent, IWeightedEdgeData>(
            this.graph,
            start,
            (node) => node?.Equals(end) ?? false,   // Search query
            (edge) => edge.Weight,                  // Edge weighting
            (edge) => 0                             // Edge heuristic ? TimeOf(evt.Current).TotalMilliseconds
        )
        ?.Select(x => TimeOf(x))
        ?.Aggregate((a,b) => a.Add(b));
    }
}

}