using System;
using System.Collections.Generic;

namespace DotQasm.Search {

public static class AStarSearch {

    private class SearchNode<T> {
        public IGraphIterator<T> Current;
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
    private static IEnumerable<IGraphIterator<T>> Backtrack<T>(SearchNode<T> node) {
        List<IGraphIterator<T>> path = new List<IGraphIterator<T>>();

        SearchNode<T> p = node;
        while (p.Parent != null) {
            path.Add(p.Current);
            p = p.Parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Find the optimal path between two graph nodes
    /// </summary>
    /// <param name="start">start iterator</param>
    /// <param name="end">end iterator</param>
    /// <param name="heuristic">comparison heuristic</param>
    /// <typeparam name="T">iterator stored type</typeparam>
    /// <returns>path from start to end iterator</returns>
    public static IEnumerable<IGraphIterator<T>> Path<T> (IGraphIterator<T> start, IGraphIterator<T> end, Func<IGraphIterator<T>,int> heuristic) {
        // Start is end
        if (start == end) {
            var x = new List<IGraphIterator<T>>(1);
            x.Add(start);
            return x;
        } 

        // Already evaluated nodes
        HashSet<SearchNode<T>> closed = new HashSet<SearchNode<T>>();
        // Current nodes
        var comparer = new SmallestFirstComparison<T>();
        PriorityQueue<SearchNode<T>> open = new PriorityQueue<SearchNode<T>>(comparer);

        // Init
        SearchNode<T> startNode = new SearchNode<T>();
        startNode.Current = start;
        startNode.G = 0;
        startNode.H = 0;
        open.Enqueue(startNode);

        // Loop
        while (!open.IsEmpty) {
            // Find the node with least F and pop it off the open list
            SearchNode<T> current = open.Dequeue();

            // Generate the successors to current
            var successors = current.Current.Next;

            // Loop over successors
            foreach (var successor in successors) {
                // If at goal, stop
                SearchNode<T> node = new SearchNode<T>();
                node.Current = successor.Node;
                node.Parent = current;

                if (node.Current.Equals(end)) {
                    return Backtrack<T>(node);
                }

                node.G = current.G + successor.Weight;
                node.H = heuristic(successor.Node);

                // If a node with the same position is in the open list and has a lower F score, skip this one
                SearchNode<T> equivalentPosition;
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