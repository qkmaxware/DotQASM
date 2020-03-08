using System.Linq;
using System.Collections.Generic;
using DotQasm.Search;
using System.Collections;

namespace DotQasm.Scheduling {

/// <summary>
/// A schedule of events in which each event linearly follows the last
/// </summary>
public class LinearSchedule: ISchedule, IEnumerable<IEvent> {

    private List<IEvent> events = new List<IEvent>();

    public IEnumerable<IEvent> Events => events;

    public IEvent First => events.FirstOrDefault();
    public IEvent Last => events.LastOrDefault();

    public int EventCount => Events.Count();

    public LinearSchedule() {}

    public LinearSchedule(List<IEvent> evts) {
        this.events = evts;
    }

    public void ScheduleEvent(IEvent evt) {
        this.events.Add(evt);
    }

    public void ClearSchedule() {
        this.events.Clear();
    }

    public IEnumerable<IEvent> Enumerate() {
        return this;
    }

    public IEnumerator<IEvent> GetEnumerator() {
        return Events.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

}