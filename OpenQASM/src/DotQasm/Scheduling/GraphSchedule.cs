using System.Linq;
using System.Collections.Generic;
using DotQasm.Search;

namespace DotQasm.Scheduling {

public class GraphSchedule : ISchedule {
    // List of events
    public IEnumerable<IEvent> Events {get; private set;}
    
    // Total number of events
    public int EventCount {get; private set;}

    // Get iterators to the first and last events
    public IEventGraphIterator First => throw new System.Exception();
    public IEventGraphIterator Last => throw new System.Exception();

    private IEvent startNode;
    private IEvent endNode;

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

    public IEnumerable<IEvent> Neighbors(int from) {
        List<IEvent> evts = new List<IEvent>();
        for (int to = 0; to < this.EventCount; to++) {
            if (adjacencyMatrix[from, to]) {
                if (to == 0) {
                    evts.Add(startNode);
                } else if (to == 1) {
                    evts.Add(endNode);
                } else {
                    evts.Add(Events.ElementAt(to - 2));
                }
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