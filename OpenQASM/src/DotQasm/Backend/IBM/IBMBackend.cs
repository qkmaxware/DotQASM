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

public abstract class IBMBackend : IBackend {

    private string key;
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

    public int Shots {get; set;}

    public abstract string BackendName {get;}
    public abstract int QubitCount {get;}

    public TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
    public int Retries = 60;

    public abstract IEnumerable<string> SupportedGates {get;}

    public IBMBackend(string key) {
        this.key = key;
        this.Shots = 1024;
    }

    public bool IsAvailable() {
        return api.GetDeviceStatus(BackendName).IsActive();
    }

    protected bool SupportsControlledX() {
        return SupportedGates.Contains("cx");
    }

    public bool SupportsGate(Gate gate) {
        return SupportedGates.Contains(gate.Symbol);
    }

    public Task<BackendResult> Exec(Circuit circuit) {
        return new Task<BackendResult>(() => {
            // Start timer
            var total = System.Diagnostics.Stopwatch.StartNew();

            // Convert circuit to quantum object
            var qobj = Convert(circuit, Shots);
            
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
            return new IBMJobResults(this, job.id, total.Elapsed, job);
        });
    }

    private IEnumerable<IBMQObjInstruction> Convert(IEvent evt) {
        switch (evt) {
            case BarrierEvent be: {
                var inst =  new IBMQObjBarrierInstruction();
                inst.qubits = be.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                return new IBMQObjInstruction[]{inst};
            } break;
            case ControlledGateEvent cge: {
                return null;
                if (this.SupportsControlledX() && cge.Operator.Symbol == "x") {

                } else {
                    throw new InvalidOperationException("controlled-" + cge.Operator.Name + " is not supported on IBM Backends");
                }
            } break;
            case GateEvent ge: {
                if (SupportsGate(ge.Operator)) {
                    var inst = new IBMQObjGateInstruction(ge.Operator.Symbol);
                    inst.qubits = ge.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                    inst.@params = new float[]{ ge.Operator.Parametres.Item1, ge.Operator.Parametres.Item2, ge.Operator.Parametres.Item3 };
                    return new IBMQObjInstruction[]{inst};
                } else {
                    throw new InvalidOperationException(ge.Operator.Name + " is not supported on IBM Backends");
                }
            } break;
            case IfEvent ife: {
                return null;

                /*var inst1 = new IBMQObjBfuncInstruction();
                inst1.relation = "==";
                inst1.mask = ;
                inst1.val = ife.LiteralValue;
                inst1.register = 0; // Store tmp value always in the 0 register

                var inst2 = new IBMQObjConditionalGateInstruction();
                inst2.conditional = 0; // Store tmp value always in the 0 register
                inst2.qubits = ife.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                return new IBMQObjInstruction[]{inst1, inst2};*/
            } break;
            case MeasurementEvent me: {
                var inst = new IBMQObjMeasureInstruction();
                inst.qubits = me.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                inst.memory = me.ClassicalDependencies.Select(cbit => cbit.ClassicalBitId).ToArray();
                //inst.register = ; // Store values in anything other that the 0 register
                return new IBMQObjInstruction[]{inst};
            } break;
            case ResetEvent re: {
                var inst = new IBMQObjResetInstruction();
                inst.qubits = re.QuantumDependencies.Select(qubit => qubit.QubitId).ToArray();
                return new IBMQObjInstruction[]{inst};
            } break;
            default: {
                throw new InvalidOperationException(evt.GetType() + " is not supported on IBM Backends");
            }
        }
    }

    private IBMQObj Convert(Circuit circuit, int shots = 1024) {
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
            var inst = Convert(evt);
            if (inst != null) {
                instructions.AddRange(inst);
            }
        }

        qobj.experiments[0].SetInstructions(instructions.ToArray());

        return qobj;
    }
}

}