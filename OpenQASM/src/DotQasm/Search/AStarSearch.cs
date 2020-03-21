using System;
using System.Collections.Generic;

namespace DotQasm.Search {

/// <summary>
/// Generic A* Heuristic search algorithm
/// </summary>
public static class AStarSearch {

    private class SearchNode<T> {
        public T Current;
        public double G = double.MaxValue;
        public double H = double.MaxValue;
        public double F => G + H;
        public SearchNode<T> Parent;

        public override bool Equals(object other) {
            return other switch {
                SearchNode<T> node => node.Current.Equals(this.Current),
                _ => base.Equals(other)
            };
        }
        public override int GetHashCode() {
            return Current.GetHashCode();
        }
    }

    private class SmallestFirstComparison<T>: IComparer<SearchNode<T>> {
        public int Compare(SearchNode<T> x, SearchNode<T> y) {
            return x.F.CompareTo(y.F);
        }
    }

    /// <summary>
    /// Iterate backwards over a search node to collect the resultant path
    /// </summary>
    /// <param name="node">end search node</param>
    /// <typeparam name="T">stored node value</typeparam>
    /// <returns>path from the start node to the end search node</returns>
    private static IEnumerable<T> Backtrack<T>(SearchNode<T> node) {
        List<T> path = new List<T>();

        SearchNode<T> p = node;
        while (true) {
            path.Add(p.Current);
            p = p.Parent;
            if (p == null) {
                break;
            }
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Find the optimal path between two graph nodes
    /// </summary>
    /// <param name="graph">Graph being searched</param>
    /// <param name="start">Node to start with</param>
    /// <param name="stopCondition">search condition</param>
    /// <param name="weightFunction">edge weight function</param>
    /// <param name="heuristicFunction">edge heuristic function</param>
    /// <typeparam name="VertexType">type of vertex data</typeparam>
    /// <typeparam name="EdgeType">type of edge data</typeparam>
    /// <returns>path of vertices from the start to the node matching the search condition</returns>
    public static IEnumerable<VertexType> Search<VertexType, EdgeType> (
        IGraph<VertexType, EdgeType> graph,
        VertexType start,
        Func<VertexType, bool> stopCondition,
        Func<EdgeType, double> weightFunction,
        Func<EdgeType, double> heuristicFunction
    ) {
        // Start is end
        if (stopCondition.Invoke(start)) {
            var x = new List<VertexType>(1);
            x.Add(start);
            return x;
        } 

        // Already evaluated nodes
        HashSet<SearchNode<VertexType>> closed = new HashSet<SearchNode<VertexType>>();
        // Current nodes
        var comparer = new SmallestFirstComparison<VertexType>();
        PriorityQueue<SearchNode<VertexType>> open = new PriorityQueue<SearchNode<VertexType>>(comparer);

        // Init
        SearchNode<VertexType> startNode = new SearchNode<VertexType>();
        startNode.Current = start;
        startNode.G = 0;
        startNode.H = 0;
        open.Enqueue(startNode);
        
        // Loop
        while (!open.IsEmpty) {
            // Find the node with least F and pop it off the open list
            SearchNode<VertexType> current = open.Dequeue();

            // Generate the successors to current
            var successors = graph.IncidentEdges(current.Current);

            // Loop over successors
            foreach (var successor in successors) {
                // If at goal, stop
                SearchNode<VertexType> node = new SearchNode<VertexType>();
                node.Current = successor.Endpoint;
                node.Parent = current;

                if (stopCondition.Invoke(node.Current)) {
                    return Backtrack<VertexType>(node);
                }

                node.G = current.G + weightFunction.Invoke(successor.Data);
                node.H = heuristicFunction.Invoke(successor.Data);

                // If a node with the same position is in the open list and has a lower F score, skip this one
                SearchNode<VertexType> equivalentPosition;
                if (open.ContainsEquivalent(node, out equivalentPosition) && equivalentPosition.F <= node.F) {
                    continue;
                }

                // If a node with the same position is in the closed list with a lower f skip this one, else add to open list
                if (closed.TryGetValue(node, out equivalentPosition) && equivalentPosition.F <= node.F) {
                    continue;
                } else {
                    open.Enqueue(node);
                }
            }

            // Push current onto the closed list
            closed.Add(current);
        }

        // No path found
        return null;
    }

}

}