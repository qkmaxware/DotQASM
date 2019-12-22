using System.Linq;
using System.Collections.Generic;
using DotQasm.Search;

namespace DotQasm.Scheduling {

public class LinearSchedule: ISchedule {

    /// <summary>
    /// Iterator for linear schedules
    /// </summary>
    public class LinearIterator : IEventGraphIterator {
        private int position;
        private IEnumerable<IEvent> list;
        public IEnumerable<IGraphEdge<IEvent>> Next => 
        !IsEnd 
        ? new IGraphEdge<IEvent>[] { 
            new IGraphEdge<IEvent>(
                1,
                new LinearIterator(list, position + 1) 
            )
        } 
        : new IGraphEdge<IEvent>[0];
        public bool IsEnd => position >= list.Count() - 1;
        public IEvent Current {get; private set;}

        public LinearIterator(IEnumerable<IEvent> list, int position) {
            this.position = position;
            this.list = list;
            this.Current = list.ElementAt(position);
        }

        public override bool Equals(object obj) {
            return obj switch {
                LinearIterator it => it.position == this.position && it.list == this.list,
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode() {
            return position ^ list.GetHashCode();
        }
    }

    private List<IEvent> events = new List<IEvent>();

    public IEnumerable<IEvent> Events => events;

    public int EventCount => Events.Count();
    public IEventGraphIterator First => new LinearIterator(Events, 0);
    public IEventGraphIterator Last => new LinearIterator(Events, Events.Count() - 1);

    public void ScheduleEvent(IEvent evt) {
        this.events.Add(evt);
    }

}

}