using System;
using DotQasm.Search;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

public interface ISchedule: IEnumerable<IEvent> {
    int EventCount {get;}
    IEventGraphIterator First {get;}
    IEventGraphIterator Last {get;}

    void ScheduleEvent(IEvent evt);
    void ClearSchedule();
}

}