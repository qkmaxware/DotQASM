using System.Linq;
using System.Collections.Generic;
using DotQasm.Search;

namespace DotQasm.Scheduling {

public class GraphSchedule : ISchedule {
    private class NanEvent : IEvent {
        private Circuit.Qubit[] qubits = new Circuit.Qubit[0];
        private Circuit.Cbit[] cbits = new Circuit.Cbit[0];
        public IEnumerable<Circuit.Qubit> QuantumDependencies => qubits;
        public IEnumerable<Circuit.Cbit> ClassicalDependencies => cbits;

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    public class GraphIterator : IEventGraphIterator {
        private GraphSchedule graph;
        private int evt;

        public GraphIterator(GraphSchedule graph, int evt) {
            this.graph = graph;
            this.evt = evt;
        }
        public IEnumerable<IGraphEdge<IEvent>> Next => graph.Neighbors(this.evt);
        public IEvent Current => this.graph.Events.ElementAt(this.evt);

        public override bool Equals(object obj) {
            return obj switch {
                GraphIterator it => it.graph == this.graph && it.evt == this.evt,
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode() {
            return this.evt ^ this.graph.GetHashCode();
        }
    }

    // List of events
    public IEnumerable<IEvent> Events {get; private set;}
    
    // Total number of events
    public int EventCount {get; private set;}

    // Get iterators to the first and last events
    public IEventGraphIterator First => new GraphIterator(this, StartNodeIndex);
    public IEventGraphIterator Last => new GraphIterator(this, EndNodeIndex);

    private IEvent startNode = new NanEvent();
    public IEvent ScheduleStartEvent => startNode;
    private IEvent endNode = new NanEvent();
    public IEvent ScheduleEndEvent => endNode;

    // Helper utility accessors to get quick access to elements of the matrix
    public int StartNodeIndex => 0;
    public int EndNodeIndex => 1;
    public int FirstEventNodeIndex => 2;

    // Map of FROM, TO connections in the graph with weights
    private double[,] weightMatrix; 
    private bool[,] adjacencyMatrix;

    public GraphSchedule(LinearSchedule schedule) : this(schedule.Events) {}

    public GraphSchedule(IEnumerable<IEvent> events) {
        this.Events = events;
        this.EventCount = events.Count();
        int count = this.EventCount;
        int total = count + 2;
        // Add 2 spots at the start for "start" and "end" node
        adjacencyMatrix = new bool[total, total];
        weightMatrix = new double[total, total];
        // Create linear chain
        adjacencyMatrix[StartNodeIndex, 2] = true;         // Start node to start of events
        adjacencyMatrix[count +2, EndNodeIndex] = true;    // last of events to end node

        int pos = 3;
        foreach (var evt in events) {
            if(pos < total)
                adjacencyMatrix[pos - 1, pos] = true;       // Chain from one event to the next
        }
    }

    public void AddEdge(int from, int to, double weight) {
        this.adjacencyMatrix[from, to] = true;
        this.weightMatrix[from, to] = weight;
    }

    public void RemoveEdge(int from, int to) {
        this.adjacencyMatrix[from, to] = false;
        this.weightMatrix[from, to] = default;
    }

    public IEnumerable<IGraphEdge<IEvent>> Neighbors(int from) {
        List<IGraphEdge<IEvent>> evts = new List<IGraphEdge<IEvent>>();
        for (int to = 0; to < this.EventCount; to++) {
            if (IsConnected(from, to)) {
                var weight = ConnectionWeight(from, to);
                evts.Add(new IGraphEdge<IEvent>(weight, new GraphIterator(this, to)));
            }
        }
        return evts;
    }

    public bool IsConnected(int from, int to) {
        return adjacencyMatrix[from, to];
    }

    public double ConnectionWeight(int from, int to) {
        return weightMatrix[from, to];
    }

    public void ScheduleEvent(IEvent evt){
        throw new System.Data.ReadOnlyException();
    }
}

}