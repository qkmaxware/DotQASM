using System;
using DotQasm.Search;
using System.Collections.Generic;

namespace DotQasm.Scheduling {

/// <summary>
/// Interface for a schedule of quatum events
/// </summary>
public interface ISchedule: IEnumerable<IEvent> {
    /// <summary>
    /// Number of events in the schedule
    /// </summary>
    int EventCount {get;}
    /// <summary>
    /// Schedule a new event
    /// </summary>
    void ScheduleEvent(IEvent evt);
    /// <summary>
    /// Clear all the events from the schedule
    /// </summary>
    void ClearSchedule();
}

}