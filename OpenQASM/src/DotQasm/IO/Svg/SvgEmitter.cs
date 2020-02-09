using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Colour = System.Drawing.Color;
using DotQasm.Scheduling;
using System.Collections.Generic;

namespace DotQasm.IO.Svg {

public class SvgEmitter : IEmitter<Circuit>, IConverter<Circuit, Svg> {

    private int cellHeight = 32;
    private int cellWidth = 32;

    private Colour GetColorFor(Gate gate) {
        return gate.Symbol switch {
            "h" => Colour.AliceBlue,
            "x" => Colour.Teal,
            "y" => Colour.Teal,
            "z" => Colour.Teal,
            "i" => Colour.Teal,
            "u1" => Colour.Pink,
            "u2" => Colour.Pink,
            "u3" => Colour.Pink,
            "U" => Colour.Pink,
            _ => Colour.DarkGray
        };
    }

    private Colour GetEventColor() {
        return Colour.DarkGray;
    }

    public void Emit(Circuit program, TextWriter writer) {
        Convert(program).Stringify(writer);
    }

    public Svg Convert(Circuit circuit) {
        var qubits = circuit.QubitCount;
        var svg = new Svg();

        // Add qubit markers
        int depth = 0;
        for(int qubit = 0; qubit < qubits; qubit++) {
            // Add qubit name
            svg.Add(
                new Text(
                    new Vector2(
                        0,
                        qubit * cellHeight + cellHeight / 2
                    ),
                    "|0\u3009"
                )
            );
        }

        // Add cbit marker and a spacer so it shows up
        var footer = new Rect(
            new BoundingBox (
                0,
                qubits * cellHeight,
                cellWidth,
                cellHeight
            )
        );
        footer.FillColour = Colour.Transparent;
        footer.StrokeColour = Colour.Transparent;
        svg.Add(
            footer
        );
        svg.Add(
            new Text(
                new Vector2(
                    0,
                    qubits * cellHeight + cellHeight / 2
                ),
                "c [" + circuit.BitCount + "]"
            )
        );

        // Add events
        foreach (var scheduled in circuit.GateSchedule) {
            switch (scheduled) {
                case GateEvent gevt: {
                    foreach (var qubit in gevt.QuantumDependencies) {
                        var rect = new Rect(
                            new BoundingBox(
                                (depth + 1) * cellWidth + 2,
                                (qubit.QubitId) * cellHeight + 2,
                                cellWidth - 2,
                                cellHeight - 2
                            )
                        );
                        rect.FillColour = GetColorFor(gevt.Operator);
                        svg.Add(rect);
                        var text = new Text(
                            new Vector2(
                                (depth + 1) * cellWidth + 2 + cellWidth/2,
                                (qubit.QubitId) * cellHeight + cellHeight/2
                            ),
                            gevt.Operator.Symbol
                        );
                        text.HorizontalAnchor = HorizontalTextAnchor.middle;
                        svg.Add(text);
                    }
                    break;
                }
                case ControlledGateEvent cevt: {
                    // Draw control bit sphere
                    var colour = GetColorFor(cevt.Operator);
                    var ctrlPosition = new Vector2(
                        (depth + 1) * cellWidth + cellWidth/ 2,
                        cevt.ControlQubit.QubitId * cellHeight + cellHeight/2
                    );
                    var ctrl = new Circle(
                        ctrlPosition,
                        4
                    );
                    ctrl.FillColour = colour;
                    svg.Add(ctrl);
                    // Draw lines to control bit
                    foreach (var qubit in cevt.TargetQubits) {
                        var mid = new Vector2(
                            (depth + 1) * cellWidth + 2 + cellWidth/2,
                            (qubit.QubitId) * cellHeight + cellHeight/2
                        );
                        var line = new Line(
                            ctrlPosition, 
                            mid
                        );
                        line.StrokeColour = colour;
                        svg.Add(line);
                    }
                    // Draw qubit gate
                    foreach (var qubit in cevt.TargetQubits) {
                        var rect = new Rect(
                            new BoundingBox(
                                (depth + 1) * cellWidth + 2,
                                (qubit.QubitId) * cellHeight + 2,
                                cellWidth - 2,
                                cellHeight - 2
                            )
                        );
                        rect.FillColour = colour;
                        svg.Add(rect);
                        var text = new Text(
                            new Vector2(
                                (depth + 1) * cellWidth + 2 + cellWidth/2,
                                (qubit.QubitId) * cellHeight + cellHeight/2
                            ),
                            cevt.Operator.Symbol
                        );
                        text.HorizontalAnchor = HorizontalTextAnchor.middle;
                        svg.Add(text);
                    }
                    break;
                }
                case MeasurementEvent mevt: {
                    // Draw lines
                    var classicalLine = new Vector2(
                        (depth + 1) * cellWidth + 2 + cellWidth/2,
                        (qubits) * cellHeight + cellHeight/2
                    );
                    foreach (var qubit in mevt.QuantumDependencies) {
                        var centre = new Vector2(
                            (depth + 1) * cellWidth + 2 + cellWidth/2,
                            (qubit.QubitId) * cellHeight + cellHeight/2
                        );
                        svg.Add(new Line(centre, classicalLine));
                    }
                    // Draw Boxes
                    foreach (var qubit in mevt.QuantumDependencies) {
                        var rect = new Rect(
                            new BoundingBox(
                                (depth + 1) * cellWidth + 2,
                                (qubit.QubitId) * cellHeight + 2,
                                cellWidth - 2,
                                cellHeight - 2
                            )
                        );
                        rect.FillColour = GetEventColor();
                        svg.Add(rect);
                        var text = new Text(
                            new Vector2(
                                (depth + 1) * cellWidth + 2 + cellWidth/2,
                                (qubit.QubitId) * cellHeight + cellHeight/2
                            ),
                            "\u2221"
                        );
                        text.HorizontalAnchor = HorizontalTextAnchor.middle;
                        svg.Add(text);
                    }
                    break;
                }
                case ResetEvent revt: {
                    foreach (var qubit in revt.QuantumDependencies) {
                        var rect = new Rect(
                            new BoundingBox(
                                (depth + 1) * cellWidth + 2,
                                (qubit.QubitId) * cellHeight + 2,
                                cellWidth - 2,
                                cellHeight - 2
                            )
                        );
                        rect.FillColour = GetEventColor();
                        svg.Add(rect);
                        var text = new Text(
                            new Vector2(
                                (depth + 1) * cellWidth + 2 + cellWidth/2,
                                (qubit.QubitId) * cellHeight + cellHeight/2
                            ),
                            "|0\u3009"
                        );
                        text.HorizontalAnchor = HorizontalTextAnchor.middle;
                        svg.Add(text);
                    }
                    break;
                }
                case BarrierEvent bevt: {
                    foreach (var qubit in bevt.QuantumDependencies) {
                        var rect = new Rect(
                            new BoundingBox(
                                (depth + 1) * cellWidth + 2,
                                (qubit.QubitId) * cellHeight + 2,
                                cellWidth - 2,
                                cellHeight - 2
                            )
                        );
                        rect.FillColour = GetEventColor();
                        svg.Add(rect);
                        var text = new Text(
                            new Vector2(
                                (depth + 1) * cellWidth + 2 + cellWidth/2,
                                (qubit.QubitId) * cellHeight + cellHeight/2
                            ),
                            "|"
                        );
                        text.HorizontalAnchor = HorizontalTextAnchor.middle;
                        svg.Add(text);
                    }
                    break;
                }
                default: {
                    break;
                }
            }
            depth ++;
        }

        // Add timelines
        for (int i = 0; i <= qubits; i++) {
            var start = new Vector2(
                (1) * cellWidth + 2,
                (i) * cellHeight + cellHeight/2
            );

            var end = new Vector2(
                (depth + 1) * cellWidth + 2,
                (i) * cellHeight + cellHeight/2
            );
            var line = new Line(start, end);
            line.StrokeColour = Colour.LightGray;
            svg.Insert(0, line);
        }
        return svg;
    }

}

}