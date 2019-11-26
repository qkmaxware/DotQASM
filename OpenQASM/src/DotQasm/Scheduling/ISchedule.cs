using System;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public interface ISchedule {
    IEnumerable<IEvent> Events {get;}

    void ScheduleEvent(IEvent evt);
}

}