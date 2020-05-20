using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DotQasm.Hardware;

namespace DotQasm.Scheduling {

using PhysicalDataPrecedenceCell = ValueTuple<PhysicalQubit, DataPrecedenceNode>;
using PhysicalDataPrecedenceRow = List<(PhysicalQubit, DataPrecedenceNode)>;
using LogicalQubitMap = Dictionary<Qubit, PhysicalQubit>;

class PhysicalDataPrecedenceTable: List<List<(PhysicalQubit, DataPrecedenceNode)>> {
    public PhysicalDataPrecedenceRow this[Qubit q] => this[q.QubitId];

    public int RowCount {get; private set;} = 0;
    public int ColumnCount => this.Select(row => row.Count).Max();

    public PhysicalDataPrecedenceTable(int rows) {
        for (int i = 0; i <= rows; i++) {
            this.Add(new PhysicalDataPrecedenceRow()); // Add row for each qubit in the schedule
            this.RowCount++;
        }
    }

    public LinearSchedule ToLinearSchedule() {
        LinearSchedule schedule = new LinearSchedule();

        var columns = this.ColumnCount;
        for (int column = 0; column < columns; column++) {
            for (int row = 0; row < this.RowCount; row++) {
                if (column < this[row].Count) {
                    var cell = this[row][column];
                    if (cell.Item2 != null && cell.Item2.Event != null)
                        schedule.ScheduleEvent(cell.Item2.Event);
                }
            }
        }

        return schedule;
    }

    private string Quote(object str) {
        return "\"" + (str?.ToString() ?? string.Empty) + "\"";
    }

    public void Encode(TextWriter writer) {
        // Print Header
        var columns = this.Select(row => row.Count).Max();
        writer.Write(Quote("Qubit Index"));
        for (int i = 1; i <= columns; i++) {
            writer.Write(",");
            writer.Write(Quote("Priority " + i));
        }
        writer.WriteLine();

        for (var i = 0; i < this.Count; i++) {
            writer.Write(Quote(i));
            
            if (this[i].Count > 0)
                writer.Write(",");

            writer.WriteLine(
                string.Join(
                    ',', 
                    this[i].Select(
                        x => 
                            x.Item2 == null 
                            ? string.Empty 
                            : Quote(x.Item1.Name + "::" + x.Item2.Event.Name + (x.Item2.Event is ControlledGateEvent ce && ce.ControlQubit.QubitId == i? "*" : string.Empty) + " (" + x.Item2.Event.GetHashCode().ToString("X") + ")")
                    )
                )
            );
        }
    }
}

}