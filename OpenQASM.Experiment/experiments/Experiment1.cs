using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using DotQasm;
using DotQasm.IO;
using DotQasm.IO.OpenQasm;
using DotQasm.Hardware;
using DotQasm.Compile;
using DotQasm.Compile.Generators;
using DotQasm.Compile.Templates;
using DotQasm.Scheduling;
using DotQasm.Backend;
using DotQasm.Backend.IBM;

namespace DotQasm.Thesis.Experiments {

public class Experiment1 {

    public static readonly int ExperimentNumber = 1;

    public static void Run() {
        // -- Run configuration -----------------------------------------------------------------------------
        var runCircuits = true;        // Run circuits against IBM
        var skipLongCircuits = true;
        var longCircuitLength = 48;     // Skip all circuits with more than 'x' operations
        var skipLargeHardware = false;
        var largeHardwareSize = 32;     // Skip all hardware that has more than 'x' qubits

        // --------------------------------------------------------------------------------------------------

        var totalTimer = Stopwatch.StartNew();

        // Circuits to test
        IEnumerable<Circuit> generated_circuits = new Circuit[] {
        /* 1*/    new QuantumTeleportationGenerator().Generate(),                                             // Quantum teleportation algorithm
        /* 2*/    new SuperdenseCodingGenerator().Generate(0b00),                                             // Superdense coding of '0'
        /* 3*/    new SuperdenseCodingGenerator().Generate(0b01),                                             // Superdense coding of '1'
        /* 4*/    new SuperdenseCodingGenerator().Generate(0b10),                                             // Superdense coding of '2'
        /* 5*/    new SuperdenseCodingGenerator().Generate(0b11),                                             // Superdense coding of '3'
        /* 6*/    new DeutschGenerator().Generate((x) => true),                                               // Constant true Deutsch
        /* 7*/    new DeutschGenerator().Generate((x) => false),                                              // Constant false Deutsch
        /* 8*/    new DeutschGenerator().Generate((x) => x),                                                  // Identity Deutsch
        /* 9*/    new DeutschGenerator().Generate((x) => !x),                                                 // Bit-flip Deutsch
        /*10*/    new DeutschJoszaGenerator().Generate((qubits: 3, oracle: new BalancedOracle("101"))),       // Create a balanced oracle for binary string 101
        /*11*/    new BernsteinVazraniGenerator().Generate((qubits: 3, value: 0b011)),                        // Bernstein Vazrani on a 3 qubit machine with the binary string 011
        /*12*/    new SimonGenerator().Generate("11"),                                                        // Simon's algorithm with a bitstring of '11'
        /*13*/    new QftGenerator().Generate(qubits: 3),                                                     // Quantum Fourier Transform with 3 qubits
        /*14*/    new QftGenerator().Generate(qubits: 4),                                                     // Quantum Fourier Transform with 4 qubits
        /*15*/    new QftGenerator().Generate(qubits: 5),                                                     // Quantum Fourier Transform with 5 qubits
            // Quantum Phase Estimation
            //new ShorsGenerator().Generate((2, 9)),                                                      // Shor's algorithm to factor '9' with a guess of 2
        /*16*/    new GroverGenerator().Generate((itemCount: 9, oracle: new PhaseOracle())),                  // Grover's algorithm on a collection of 9 items looking for |101> and |110>
            // Quantum Counting
            // Quantum Key Distribution
        /*17*/    new MaxCutTemplate().GetTemplateCircuit()
        };
        IEnumerable<Circuit> prebuilt_circuits = Directory
            .GetFiles(Path.Combine("OpenQASM.Examples", "OpenQASM"), "*.qasm")
            .Where(filename => {
                return !filename.Contains("qelib1.inc"); // ignore qelib1.inc
            })
            .Select(filename => {
                var text = File.ReadAllText(filename);
                try {
                    var circ = DotQasm.IO.OpenQasm.Parser.ParseCircuit(
                        text, 
                        new PhysicalDirectory(Path.GetDirectoryName(filename))
                    );
                    circ.Name = filename;
                    return circ;
                } catch (DotQasm.IO.OpenQasm.OpenQasmSyntaxException ex) {
                    Console.WriteLine(ex.Format(filename, text));
                    throw;
                }
            });

        var circuits = generated_circuits.Concat(prebuilt_circuits).ToArray();

        // Hardware to compile against
        var hardware = Directory
            .GetFiles(Path.Combine("OpenQASM.Examples", "Hardware"), "*.yaml")
            .Select(filename => {
                return ParseYaml<HardwareConfiguration>(filename);
            }).Where( hw => {
                return !skipLargeHardware || hw.PhysicalQubitCount < largeHardwareSize; // Skip large hardware to avoid stack overflows
            }).ToArray();
        
        // Create timing model
        var timeModel = new BasicLatencyEstimator();

        // Create optimizers
        var scheduling = new Optimization.Strategies.HardwareScheduling();
        var swaps = new Optimization.Strategies.SwapDecompose();

        // Create backend provider
        string apiKey = runCircuits ? System.Environment.GetEnvironmentVariable("IBM_KEY") : null; // Set to null to disable IBM access
        var provider = new Backend.IBM.IBMBackendProvider();

        // Debug statements
        var circuitCount = circuits.Count();
        Console.WriteLine($"Testing: {circuits.Count()}");
        foreach (var circ in circuits) {
            Console.WriteLine($"  {circ.Name}");
        }
        Console.WriteLine();
        Console.WriteLine($"Against: {hardware.Count()}");
        foreach (var hw in hardware) {
            Console.WriteLine($"  {hw.Name} ({hw.PhysicalQubitCount} qubits)");
        }
        if (apiKey == null) {
            Console.WriteLine();
            Console.Error.WriteLine("($IBM_KEY) No IBM Quantum Experience API key found. Circuit running will be disabled.");
        }
        Console.WriteLine();
        Console.WriteLine("Running, this may take some time...please wait");

        // Actual experiment
        var na = $",N/A";
        var failed = $",FAIL";
        var directory = Path.Combine(".qasmdata", "experiments", ExperimentNumber.ToString(), DateTime.Now.ToString("yyyy/MM/dd H.mmtt")); 
        Directory.CreateDirectory(directory);
        scheduling.SetDataDirectory(directory);

        using (var summaryWriter = new StreamWriter(Path.Combine(directory, "index.csv"))) 
        using (var timingWriter = new StreamWriter(Path.Combine(directory, "timings.csv"))) 
        using (var errorLogWriter = new StreamWriter(Path.Combine(directory, "errors.log"))) 
        using (var qubitCountWriter = new StreamWriter(Path.Combine(directory, "matrix.qubitCountBefore.csv")))
        using (var eventCountWriter = new StreamWriter(Path.Combine(directory, "matrix.eventCount.csv"))) 
        using (var estimatedRuntimeBeforeWriter = new StreamWriter(Path.Combine(directory, "matrix.estimatedRuntimeBefore.csv"))) 
        using (var optTimeMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.optimizationTime.csv")))
        using (var eventChangeMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.eventCountDelta.csv")))
        using (var swapCountMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.eventSwapCountAfter.csv")))
        using (var ldpgMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.ldpg.csv")))
        using (var pdptMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.pdpt.csv")))
        using (var runtimeBeforeMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.ibmRuntimeBefore.csv")))
        using (var runtimeAfterMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.ibmRuntimeAfter.csv")))
        using (var estimatedRuntimeAfterMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.estimatedRuntimeAfter.csv")))
        using (var estimatedRuntimeDeltaMtxWriter = new StreamWriter(Path.Combine(directory, "matrix.estimatedRuntimeDelta.csv")))
        {
            // Write headers
            summaryWriter.WriteLine($"Circuit Id, Circuit Name, Total Experiment Time, Analysis Time, Average Optimization Time");
            var hwHeaderString = "Circuit Id," + string.Join(",", hardware.Select(hw => hw.Name));
            optTimeMtxWriter.WriteLine(hwHeaderString);
            ldpgMtxWriter.WriteLine(hwHeaderString);
            pdptMtxWriter.WriteLine(hwHeaderString);
            eventChangeMtxWriter.WriteLine(hwHeaderString);
            swapCountMtxWriter.WriteLine(hwHeaderString);
            runtimeBeforeMtxWriter.WriteLine(hwHeaderString);
            estimatedRuntimeAfterMtxWriter.WriteLine(hwHeaderString);
            estimatedRuntimeDeltaMtxWriter.WriteLine(hwHeaderString);
            runtimeAfterMtxWriter.WriteLine(hwHeaderString);
            eventCountWriter.WriteLine("Circuit Id, Event Count");
            estimatedRuntimeBeforeWriter.WriteLine("Circuit Id, Estimated Time");
            qubitCountWriter.WriteLine("Circuit Id, Qubits, Classical Bits");

            // Write timing chart
            timingWriter.WriteLine("Type, Timing");
            timingWriter.WriteLine($"{typeof(BarrierEvent)}, {timeModel.TimeOf(typeof(BarrierEvent))}");
            timingWriter.WriteLine($"{typeof(MeasurementEvent)}, {timeModel.TimeOf(typeof(MeasurementEvent))}");
            timingWriter.WriteLine($"{typeof(ResetEvent)}, {timeModel.TimeOf(typeof(ResetEvent))}");
            timingWriter.WriteLine($"{typeof(IfEvent)}, {timeModel.TimeOf(typeof(IfEvent))}");
            timingWriter.WriteLine($"{typeof(GateEvent)}, {timeModel.TimeOf(typeof(GateEvent))}");
            timingWriter.WriteLine($"{typeof(ControlledGateEvent)}, {timeModel.TimeOf(typeof(ControlledGateEvent))}");
            timingWriter.WriteLine($"Other, {timeModel.TimeOf(typeof(object))}");
            timingWriter.Flush();
    
            // Iterate over all circuits and run experiments
            for (var i = 0; i < circuits.Length; i++) {
                // Init variables / names
                var circuit = circuits[i];
                var circuit_name = Path.GetFileNameWithoutExtension(circuit.Name);
                var circuit_id = $"{ExperimentNumber}.{i,00}";
                var file_name_prefix = $"{circuit_id} {circuit_name}";
                var circuit_diagram = file_name_prefix + ".svg";
                circuit.Name = file_name_prefix;
                scheduling.Use(new VirtualFile(circuit.Name, string.Empty)); // For naming of generated LDPG & PDPT files

                // Update progress
                Console.Write($"{(i + 1).ToString().PadLeft(circuitCount.ToString().Length, ' ')}/{circuitCount} ");
                if (skipLongCircuits && circuit.GateSchedule.EventCount > longCircuitLength) {
                    Console.WriteLine("Skipped");
                    continue;
                }
                var progress = new ProgressBar(2 + hardware.Length);
                var stage = 0;

                // Print rows
                optTimeMtxWriter.Write(circuit_id);
                ldpgMtxWriter.Write(circuit_id);
                eventChangeMtxWriter.Write(circuit_id);
                runtimeBeforeMtxWriter.Write(circuit_id);
                pdptMtxWriter.Write(circuit_id);
                swapCountMtxWriter.Write(circuit_id);
                estimatedRuntimeAfterMtxWriter.Write(circuit_id);
                estimatedRuntimeDeltaMtxWriter.Write(circuit_id);
                runtimeAfterMtxWriter.Write(circuit_id);
                eventCountWriter.Write(circuit_id);
                estimatedRuntimeBeforeWriter.Write(circuit_id);
                qubitCountWriter.WriteLine(circuit_id);

                // Pre-analysis
                var complete_timer = Stopwatch.StartNew();
                TimeSpan estimated_linear_time;
                var timer = Stopwatch.StartNew();
                {   
                    // Compute events
                    var number_of_events = circuit.GateSchedule.EventCount;
                    eventCountWriter.WriteLine  ($", {number_of_events}");
                    eventCountWriter.Flush();

                    // Compute qubits
                    qubitCountWriter.Write      ($", {circuit.QubitCount}");
                    qubitCountWriter.WriteLine  ($", {circuit.BitCount}");
                    qubitCountWriter.Flush();

                    //Estimate time if instructions performed sequentially
                    estimated_linear_time = circuit.GateSchedule.Select((evt) => timeModel.TimeOf(evt)).Aggregate((t1, t2) => t1 + t2);
                    estimatedRuntimeBeforeWriter.WriteLine($", {estimated_linear_time}");
                    estimatedRuntimeBeforeWriter.Flush();

                    // Emit circuit diagram
                    var emitter = new DotQasm.IO.Svg.SvgEmitter();
                    using (StreamWriter writer = new StreamWriter(Path.Combine(directory, circuit_diagram))) {
                        emitter.Emit(circuit, writer);
                    }

                    // Try run algorithm on all hardware
                    foreach (var hw in hardware) {
                        var backend = provider.CreateBackendInterface(hw.Alias, circuit.QubitCount, apiKey);
                        if (backend != null && apiKey != null) {
                            var taskRun = backend.Exec(circuit);
                            taskRun.RunSynchronously();
                            var taskRunResult = (IBMJobResults)taskRun.Result;
                            if (taskRunResult.Success) {
                                runtimeBeforeMtxWriter.Write($",{taskRunResult.ExecutionTime}");
                            } else {
                                runtimeBeforeMtxWriter.Write(failed);
                            }
                        } else {
                            runtimeBeforeMtxWriter.Write(na);
                        }
                    }
                }
                var pre_analysis_time = timer.Elapsed;
                progress.Update(++stage);

                // Optimization
                var results = new List<ExperimentResultSet>(hardware.Length); 
                timer = Stopwatch.StartNew();
                {
                    foreach (var device in hardware) {
                        // Prepare
                        scheduling.Use(device);
                        var result = new ExperimentResultSet();
                        results.Add(result);

                        // Optimize
                        try {
                            var thisHwTime = Stopwatch.StartNew();
                            var sched = scheduling.Transform(circuit.GateSchedule, out result.Ldpg, out result.LdpgFilename, out result.Pdpt, out result.PdptFilename);
                                sched = swaps.Transform(sched);
                            result.Schedule = sched;
                            optTimeMtxWriter.Write($",{thisHwTime.Elapsed}");
                        } catch (Exception e) {
                            optTimeMtxWriter.Write(na);
                            errorLogWriter.WriteLine(e);
                            errorLogWriter.Flush();
                        }
                        progress.Update(++stage);
                    }
                }
                var optimization_time = timer.Elapsed;
                var average_optimization_time = optimization_time / hardware.Length;
                optTimeMtxWriter.Flush();

                // Post-analysis
                timer = Stopwatch.StartNew();
                {
                    for (var resultIndex = 0; resultIndex < results.Count; resultIndex++) {
                        var result = results[resultIndex];
                        var hw = hardware[resultIndex];

                        if (result.Schedule != null) {
                            // Compute number of added swaps
                            var swapCount = result.Pdpt.SelectMany(row => row).Where(cell => cell.Item2 != null && cell.Item2.Event is SwapEvent).Count();
                            // Compute the estimated time with parallelisation of the pdpt
                            TimeSpan estimatedTime = new TimeSpan();
                            // Iterate over columns
                            for (var column = 0; column < result.Pdpt.ColumnCount; column++) {
                                // Get max row value in the column
                                var maxTimeInColumn = TimeSpan.Zero;
                                for (var row = 0; row < result.Pdpt.RowCount; row++) {
                                    var pdptRow = result.Pdpt[row];
                                    if (pdptRow.Count > column) {
                                        var cell = pdptRow[column];
                                        var cellTime = cell.Item2 != null && cell.Item2.Event != null ? timeModel.TimeOf(cell.Item2.Event) : TimeSpan.Zero;
                                        maxTimeInColumn = maxTimeInColumn > cellTime ? maxTimeInColumn : cellTime;
                                    }
                                }
                                estimatedTime += maxTimeInColumn;
                            }

                            ldpgMtxWriter.Write($",{result.LdpgFilename}");
                            pdptMtxWriter.Write($",{result.PdptFilename}");
                            eventChangeMtxWriter.Write($",{result.Schedule.EventCount - circuit.GateSchedule.EventCount}");
                            swapCountMtxWriter.Write($",{swapCount}");
                            estimatedRuntimeAfterMtxWriter.Write($",{estimatedTime}");
                            estimatedRuntimeDeltaMtxWriter.Write($",{estimatedTime - estimated_linear_time}");

                            // Try run algorithm on hardware
                            var backend = provider.CreateBackendInterface(hw.Alias, circuit.QubitCount, apiKey);
                            if (backend != null && apiKey != null) {
                                var taskRun = backend.Exec(circuit);
                                taskRun.RunSynchronously();
                                var taskRunResult = (IBMJobResults)taskRun.Result;
                                if (taskRunResult.Success) {
                                    runtimeAfterMtxWriter.Write($",{taskRunResult.ExecutionTime}");
                                } else {
                                    runtimeAfterMtxWriter.Write(failed);
                                }
                            } else {
                                runtimeAfterMtxWriter.Write(na);
                            }
                            
                        } else {
                            ldpgMtxWriter.Write(na);
                            pdptMtxWriter.Write(na);
                            eventChangeMtxWriter.Write(na);
                            swapCountMtxWriter.Write(na);
                            estimatedRuntimeAfterMtxWriter.Write(na);
                            estimatedRuntimeDeltaMtxWriter.Write(na);
                            runtimeAfterMtxWriter.Write(na);
                        }
                    }
                }
                var post_analysis_time = timer.Elapsed;
                progress.Update(++stage);

                // Write results / end lines
                summaryWriter.WriteLine($"{circuit_id}, {circuit.Name}, {complete_timer.Elapsed}, {pre_analysis_time + post_analysis_time}, {average_optimization_time}");
                optTimeMtxWriter.WriteLine();
                ldpgMtxWriter.WriteLine();
                pdptMtxWriter.WriteLine();
                eventChangeMtxWriter.WriteLine();
                swapCountMtxWriter.WriteLine();
                runtimeBeforeMtxWriter.WriteLine();
                estimatedRuntimeAfterMtxWriter.WriteLine();
                estimatedRuntimeDeltaMtxWriter.WriteLine();
                runtimeAfterMtxWriter.WriteLine();

                // Flush so that I get at least some data printed out on each iteration
                summaryWriter.Flush();
                optTimeMtxWriter.Flush();
                ldpgMtxWriter.Flush();
                pdptMtxWriter.Flush();
                eventChangeMtxWriter.Flush();
                swapCountMtxWriter.Flush();
                runtimeBeforeMtxWriter.Flush();
                estimatedRuntimeAfterMtxWriter.Flush();
                estimatedRuntimeDeltaMtxWriter.Flush();
                runtimeAfterMtxWriter.Flush();

                // Clean up garbage to give better performance next algorithm (in-case lots of objects got created)
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Experiment Completed...{totalTimer.Elapsed}");
        Console.WriteLine($"Files emitted to {directory}");
    }

    private static YamlDotNet.Serialization.Deserializer yamlParser = (new YamlDotNet.Serialization.DeserializerBuilder())
        .IgnoreUnmatchedProperties()
        .Build();
    private static T ParseYaml<T>(string path) {
        if (path != null) {
            return yamlParser.Deserialize<T>(File.ReadAllText(path));
        } else {
            return default(T);
        }
    }

    private class ExperimentResultSet {
        public LinearSchedule Schedule;
        public string LdpgFilename;
        public LogicalDataPrecedenceGraph Ldpg;
        public string PdptFilename;
        public PhysicalDataPrecedenceTable Pdpt;
    }

    private class PhaseOracle : IOperator<IEnumerable<Qubit>> {
        public void Invoke(IEnumerable<Qubit> qreg) {
            // Phase oracle to mark states |101> and |110>
            qreg.ElementAt(0).ControlledApply(qreg.ElementAt(2), Gate.PauliZ);
            qreg.ElementAt(1).ControlledApply(qreg.ElementAt(2), Gate.PauliZ);
        }
    }

    private class BalancedOracle : IOperator<IEnumerable<Qubit>> {
        private string bstring;
        public BalancedOracle(string binary) {
            this.bstring = binary;
        }
        public void Invoke(IEnumerable<Qubit> qreg) {
            var counts = qreg.Count();
            var inputs = qreg.Take(counts - 1);
            var ancilla = qreg.Last();

            // Place X gates where the binary string is 1
            for (var i = 0; i < counts -1; i ++) {
                if (bstring[i] == '1')
                    inputs.ElementAt(i).X();
            }

            // Controlled Not gates
            foreach (var qubit in inputs) {
                qubit.CX(ancilla);
            }

            // Place X gates
            for (var i = 0; i < counts -1; i ++) {
                if (bstring[i] == '1')
                    inputs.ElementAt(i).X();
            }
        }
    }

}

}