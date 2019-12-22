using System;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public interface ISchedule {
    IEnumerable<IEvent> Events {get;}

    int EventCount {get;}
    IEventGraphIterator First {get;}
    IEventGraphIterator Last {get;}

    void ScheduleEvent(IEvent evt);
}

}