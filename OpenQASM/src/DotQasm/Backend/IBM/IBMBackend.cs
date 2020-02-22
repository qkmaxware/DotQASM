using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using DotQasm.Backend.IBM.Api;
using DotQasm.Scheduling;

namespace DotQasm.Backend.IBM {

/// <summary>
/// Base class for all IBM Quantum Experience devices
/// </summary>
public abstract class IBMBackend : IBackend {

    /// <summary>
    /// Api key
    /// </summary>
    private string key;
    /// <summary>
    /// Reference to the underlying API interface
    /// </summary>
    private IBM.Api.IBMApi _api;
    protected IBM.Api.IBMApi api {
        get {
            if (_api == null) {
                _api = new IBM.Api.IBMApi(key);
            }
            // _api.AuthenticateWithToken(key); Could do this...?
            return _api;
        }
    }

    /// <summary>
    /// Number of times to retry the quantum circuit
    /// </summary>
    public int Shots {get; set;}

    /// <summary>
    /// Name of the device according to IBM's api
    /// </summary>
    public abstract string BackendName {get;}

    /// <summary>
    /// Obtain device information
    /// </summary>
    public BackendInformation Information => new BackendInformation(){
        Name = BackendName,
        Description = QubitCount + " qubit " + (BackendName.Contains("simulator") ? "simulator" : "computer")
    };
    
    /// <summary>
    /// Number of qubits supported by this device
    /// </summary>
    public abstract int QubitCount {get;}

    /// <summary>
    /// Delay between each status check with the IBM api 
    /// </summary>
    public TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
    /// <summary>
    /// Number of times to check the IBM api before the job is considered failed
    /// </summary>
    public int Retries = 60;

    /// <summary>
    /// Natively supported quantum gates
    /// </summary>
    public abstract IEnumerable<string> SupportedGates {get;}

    /// <summary>
    /// List of connectivity between qubits
    /// </summary>
    public abstract IEnumerable<KeyValuePair<int, int>> QubitConnectivity {get;}

    /// <summary>
    /// Create a new ibm backend with the given api key
    /// </summary>
    /// <param name="key">api key</param>
    public IBMBackend(string key) {
        this.key = key;
        this.Shots = 1024;
    }

    /// <summary>
    /// Check if the device is available to be used
    /// </summary>
    /// <returns>true if the device is available</returns>
    public bool IsAvailable() {
        return api.GetDeviceStatus(BackendName).IsActive();
    }

    /// <summary>
    /// True if the device supports the controlled-not operator
    /// </summary>
    /// <returns>true if the device supports the controlled-not operator</returns>
    protected bool SupportsControlledX() {
        return SupportedGates.Contains("cx");
    }

    /// <summary>
    /// Check if the backend supports the given quantum gate
    /// </summary>
    /// <param name="gate">quantum gate</param>
    /// <returns>true if the gate's name is in the supported gates list</returns>
    public bool SupportsGate(Gate gate) {
        return SupportedGates.Contains(gate.Symbol);
    }

    /// <summary>
    /// Check if two qubits are physically adjacent
    /// </summary>
    /// <param name="from">starting qubit</param>
    /// <param name="to">endinf qubit</param>
    /// <returns>true if there is a qubit connection from a qubit to another</returns>
    public bool AreQubitsAdjacent(int from, int to) {
        return QubitConnectivity.Contains(new KeyValuePair<int, int>(from, to));
    }

    /// <summary>
    /// Execute the given quantum circuit against on this device
    /// </summary>
    /// <param name="circuit">quantum program</param>
    /// <returns>Async task which eventually contain the results of the circuit</returns>
    public Task<BackendResult> Exec(Circuit circuit) {
        return new Task<BackendResult>(() => {
            // Start timer
            var total = System.Diagnostics.Stopwatch.StartNew();

            // Convert circuit to quantum object
            var qobj = convert(circuit, Shots);
            
            // Submit the job
            var job = this.api.SubmitJob(this.BackendName, circuit.Name, qobj, Shots);

            // wait for job to complete
            var retriesleft = Retries;
            while (retriesleft > 0 && !job.IsDone()) {
                Thread.Sleep(RetryDelay);
                retriesleft--;
                job = this.api.GetJobInfo(job.id); // Keep fetching
            }

            // process results
            return new IBMJobResults(this, total.Elapsed, job);
        });
    }

    /// <summary>
    /// Convert a list of bit locations into a state mask
    /// </summary>
    /// <param name="spots">bits</param>
    /// <returns>mask with '1' on each of the given bits</returns>
    private int makeMask(IEnumerable<int> spots) {
        int mask = 0;
        foreach (int position in spots) {
            mask |= 1 << position;
        }
        return mask;
    }

    /// <summary>
    /// Convert a quantum event to a quantum object compatible list of instructions
    /// </summary>
    /// <param name="qubits">number of qubits in the circuit</param>
    /// <param name="cbits">number of classical bits in the circuit</param>
    /// <param name="evt">event to convert</param>
    /// <returns>list of IBM compatible instructions</returns>
    private IEnumerable<IBMQObjInstruction> convert(int qubits, int cbits, IEvent evt) {
        switch (evt) {
            case BarrierEvent be: {
                var inst =  new IBMQObjBarrierInstruction();
                inst.qubits = be.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                return new IBMQObjInstruction[]{ inst };
            } 
            case ControlledGateEvent cge: {
                if (this.SupportsControlledX() && cge.Operator.Symbol == "x") {
                    List<IBMQObjInstruction> insts = new List<IBMQObjInstruction>();
                    foreach (var qubit in cge.TargetQubits) {
                        if (!AreQubitsAdjacent(cge.ControlQubit.QubitId, qubit.QubitId)) {
                            throw new InvalidOperationException("no connectivity between qubits " + cge.ControlQubit.QubitId + " and " + qubit.QubitId);
                        }

                        var inst = new IBMQObjGateInstruction("cx");
                        inst.qubits = new int[]{ cge.ControlQubit.QubitId, qubit.QubitId };
                        insts.Add(inst);
                    }
                    return insts;
                } else {
                    throw new InvalidOperationException("controlled-" + cge.Operator.Name + " is not supported on IBM Backends");
                }
            } 
            case GateEvent ge: {
                if (SupportsGate(ge.Operator)) {
                    List<IBMQObjInstruction> insts = new List<IBMQObjInstruction>();
                    foreach (var qubit in ge.QuantumDependencies) {
                        var inst = new IBMQObjGateInstruction(ge.Operator.Symbol);
                        inst.qubits = new int[]{ qubit.QubitId };
                        inst.@params = new double[]{ ge.Operator.Parametres.Item1, ge.Operator.Parametres.Item2, ge.Operator.Parametres.Item3 };
                        insts.Add(inst);
                    }
                    return insts;
                } else {
                    throw new InvalidOperationException(ge.Operator.Name + " is not supported on IBM Backends");
                }
            } 
            case IfEvent ife: {
                // TODO MEASURE, RESET, UNITARY OPERATOR
                var inst1 = new IBMQObjBfuncInstruction();
                inst1.relation = "==";
                inst1.mask = makeMask(ife.ClassicalDependencies.Select(cbit => cbit.ClassicalBitId));
                inst1.val = ife.LiteralValue;
                inst1.register = cbits; // Store tmp value always in the last unused register
                
                List<IBMQObjInstruction> insts = new List<IBMQObjInstruction>();
                insts.Add(inst1);

                switch (ife.Event) {
                    case GateEvent ge: {
                        if (SupportsGate(ge.Operator)) {
                            foreach (var qubit in ge.QuantumDependencies) {
                                var inst = new IBMQObjConditionalGateInstruction(ge.Operator.Symbol);
                                inst.conditional = cbits;
                                inst.qubits = new int[]{ qubit.QubitId };
                                inst.@params = new double[]{ ge.Operator.Parametres.Item1, ge.Operator.Parametres.Item2, ge.Operator.Parametres.Item3 };
                                insts.Add(inst);
                            }
                        } else {
                            throw new InvalidOperationException(ge.Operator.Name + " is not supported on IBM Backends");
                        }
                    } break;
                    case ControlledGateEvent cge: {
                        if (this.SupportsControlledX() && cge.Operator.Symbol == "x") {
                            foreach (var qubit in cge.TargetQubits) {
                                if (!AreQubitsAdjacent(cge.ControlQubit.QubitId, qubit.QubitId)) {
                                    throw new InvalidOperationException("no connectivity between qubits " + cge.ControlQubit.QubitId + " and " + qubit.QubitId);
                                }

                                var inst = new IBMQObjConditionalGateInstruction("cx");
                                inst.conditional = cbits;
                                inst.qubits = new int[]{ cge.ControlQubit.QubitId, qubit.QubitId };
                                insts.Add(inst);
                            }
                        } else {
                            throw new InvalidOperationException("controlled-" + cge.Operator.Name + " is not supported on IBM Backends");
                        }
                    } break;
                    case MeasurementEvent me: {
                        var inst = new IBMQObjConditionalMeasureInstruction();
                        inst.conditional = cbits;
                        inst.qubits = me.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                        inst.memory = me.ClassicalDependencies.Select(cbit => cbit.ClassicalBitId).ToArray();
                        inst.register = me.ClassicalDependencies.Select(cbit => cbit.ClassicalBitId).ToArray(); 
                        insts.Add(inst);
                    } break;
                    case ResetEvent re: {
                        var inst = new IBMQObjConditionalResetInstruction();
                        inst.conditional = cbits;
                        inst.qubits = re.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                        insts.Add(inst);
                    } break;
                    default: {
                        throw new InvalidOperationException(ife.Event.GetType() + " is not supported on with 'if' statements on IBM Backends");
                    }
                }
                return insts;
            }
            case MeasurementEvent me: {
                var inst = new IBMQObjMeasureInstruction();
                inst.qubits = me.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                inst.memory = me.ClassicalDependencies.Select(cbit => cbit.ClassicalBitId).ToArray();
                inst.register = me.ClassicalDependencies.Select(cbit => cbit.ClassicalBitId).ToArray(); 
                return new IBMQObjInstruction[]{ inst };
            }
            case ResetEvent re: {
                var inst = new IBMQObjResetInstruction();
                inst.qubits = re.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                return new IBMQObjInstruction[]{ inst };
            } 
            default: {
                throw new InvalidOperationException(evt.GetType() + " is not supported on IBM Backends");
            }
        }
    }

    /// <summary>
    /// Convert a quantum circuit to a quantum object compatible list of instructions
    /// </summary>
    /// <param name="circuit">circuit to convert</param>
    /// <param name="shots">number of retries</param>
    /// <returns>IBM api compatible quantum object</returns>
    private IBMQObj convert(Circuit circuit, int shots = 1024) {
        IBMQObj qobj = new IBMQObj();
        qobj.type = IBMQObjType.QASM;
        
        qobj.qobj_id = (circuit.Name ?? string.Empty) + " " + DateTime.Now.ToString();
        
        qobj.header = new IBMQObjHeaderMetadata();

        qobj.config.memory_slots = circuit.BitCount;
        qobj.config.seed = 1;
        qobj.config.shots = shots;

        qobj.experiments = new IBMQObjExperiment[]{
            new IBMQObjExperiment()
        };
        qobj.experiments[0].config = qobj.config;
        qobj.experiments[0].header = new IBMQObjHeaderMetadata();

        List<IBMQObjInstruction> instructions = new List<IBMQObjInstruction>();

        foreach (var evt in circuit.GateSchedule) {
            var inst = convert(circuit.QubitCount, circuit.BitCount, evt);
            if (inst != null) {
                instructions.AddRange(inst);
            }
        }

        qobj.experiments[0].SetInstructions(instructions.ToArray());

        return qobj;
    }
}

}