using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DotQasm.Hardware;

namespace DotQasm.Scheduling {

using PhysicalDataPrecedenceCell = ValueTuple<PhysicalQubit, DataPrecedenceNode>;
using PhysicalDataPrecedenceRow = List<(PhysicalQubit, DataPrecedenceNode)>;
using LogicalQubitMap = Dictionary<Qubit, PhysicalQubit>;

/// <summary>
/// Physical data precedence table indicating which operations apply to which logical and physical qubits during each discrete time-slice
/// </summary>
public class PhysicalDataPrecedenceTable: List<List<(PhysicalQubit, DataPrecedenceNode)>> {
    /// <summary>
    /// Fetch the row cooresponding to the given logical qubit
    /// </summary>
    public PhysicalDataPrecedenceRow this[Qubit q] => this[q.QubitId];

    /// <summary>
    /// Number of rows in the table
    /// </summary>
    public int RowCount {get; private set;} = 0;
    /// <summary>
    /// Number of columns in the table
    /// </summary>
    /// <returns></returns>
    public int ColumnCount => this.Select(row => row.Count).Max();

    public PhysicalDataPrecedenceTable(int rows) {
        for (int i = 0; i <= rows; i++) {
            this.Add(new PhysicalDataPrecedenceRow()); // Add row for each qubit in the schedule
            this.RowCount++;
        }
    }

    /// <summary>
    /// Convert the given table of events to a linear sequence
    /// </summary>
    /// <returns>linear sequence of events</returns>
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

    /// <summary>
    /// Write the table as a CSV to the given text writer
    /// </summary>
    /// <param name="writer">writer to write to</param>
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