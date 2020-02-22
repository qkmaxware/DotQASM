using System;
using DotQasm.Search;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Interface for a schedule of quatum events
/// </summary>
public interface ISchedule: IEnumerable<IEvent> {
    int EventCount {get;}
    IEventGraphIterator First {get;}
    IEventGraphIterator Last {get;}

    void ScheduleEvent(IEvent evt);
    void ClearSchedule();
}

}