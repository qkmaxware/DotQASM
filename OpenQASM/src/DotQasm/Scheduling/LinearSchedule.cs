using System.Collections.Generic;

namespace DotQasm.Scheduling {

public class LinearSchedule: ISchedule {

    private List<IEvent> events = new List<IEvent>();

    public IEnumerable<IEvent> Events => events;

    public void ScheduleEvent(IEvent evt) {
        this.events.Add(evt);
    }

}

}