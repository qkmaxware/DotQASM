namespace DotQasm.Backend.IBM.Api {

/// <summary>
/// Quantum object type
/// </summary>
public enum IBMQObjType {
    /// <summary>
    /// OpenQASM type
    /// </summary>
    QASM,
    /// <summary>
    /// OpenPulse type
    /// </summary>
    PULSE
}

public class IBMQObjUserConfig {
    /// <summary>
    /// Number of times to repeat the experiment
    /// </summary>
    public int shots {get; set;}
    /// <summary>
    /// Number of classical memory slots used in this job
    /// </summary>
    public int memory_slots {get; set;}
    /// <summary>
    /// Randomization seed for simulators
    /// </summary>
    public int seed {get; set;}
    /// <summary>
    ///  For credit-based backends, the maximum number of credits that a user is willing to spend on this run 
    /// </summary>
    public int max_credits {get; set;}

    public IBMQObjUserConfig() {
        this.shots = 1024;
        this.memory_slots = 5;
        this.seed = 1;
        this.max_credits = 0;
    }
}

public class IBMQObjHeaderMetadata {

}

public class IBMQObjSnapshotInstruction: IBMQObjInstruction {
    public override string name => "snapshot";
    /// <summary>
    /// Snapshot label which is used to identify the snapshot in the output
    /// </summary>
    public string label {get; set;}
    /// <summary>
    /// Type of snapshot
    /// </summary>
    public string type {get; set;}
}

public class IBMQObjMeasureInstruction: IBMQObjInstruction {
    public override string name => "measure";
    /// <summary>
    /// List of qubits to apply the barrier
    /// </summary>
    public int[] qubits {get; set;}
    /// <summary>
    /// List of memory slots in which to store the measurement results
    /// </summary>
    public int[] memory {get; set;}
    /// <summary>
    /// List of register slots in which to store the measurement results 
    /// </summary>
    public int[] register {get; set;}
}

public class IBMQObjBarrierInstruction: IBMQObjInstruction {
    public override string name => "barrier";
    /// <summary>
    /// List of qubits to apply the barrier
    /// </summary>
    public int[] qubits {get; set;}
}

public class IBMQObjConditionalGateInstruction: IBMQObjGateInstruction {
    /// <summary>
    /// Apply the gate if the given register is 1
    /// </summary>
    public int conditional {get; set;}
    public IBMQObjConditionalGateInstruction(string gate_name): base(gate_name) {}
}

public class IBMQObjGateInstruction: IBMQObjInstruction {
    /// <summary>
    /// Name of the gate
    /// </summary>
    private string gate_name;
    public override string name => gate_name;
    /// <summary>
    /// List of qubits to apply the gate
    /// </summary>
    public int[] qubits {get; set;}
    /// <summary>
    /// List of parameters for the gate
    /// </summary>
    public float[] @params {get; set;}

    public IBMQObjGateInstruction(string gate_name){
        this.gate_name = gate_name;
    }
}

public class IBMQObjCopyInstruction: IBMQObjInstruction {
    public override string name => "copy";
    /// <summary>
    /// Register slot to copy
    /// </summary>
    public int register_orig {get; set;}
    /// <summary>
    /// Register slot(s) to copy to
    /// </summary>
    public int[] register_copy {get; set;}
}

public class IBMQObjBfuncInstruction: IBMQObjInstruction {
    public override string name => "bfunc";
    /// <summary>
    /// Hex value which is applied as an AND to the register bits
    /// </summary>
    public int mask {get; set;}
    /// <summary>
    /// Relational operator for comparing the masked register to the val (“==”: equals, “!=” not equals)
    /// </summary>
    public string relation {get; set;}
    /// <summary>
    /// Value to which to compare the masked register
    /// </summary>
    public int val {get; set;}
    /// <summary>
    /// Register slot in which to store the boolean function result
    /// </summary>
    public int register {get; set;}
    /// <summary>
    /// Memory slot in which to store the boolean function result
    /// </summary>
    public int memory {get; set;}
}

public abstract class IBMQObjInstruction {
    public abstract string name {get;}
}

public class IBMQObjExperiment {
    /// <summary>
    /// User-defined structure that contains metadata on each experiment and is not used by the backend
    /// </summary>
    public IBMQObjHeaderMetadata header {get; set;}
    /// <summary>
    /// Configuration structure for user settings
    /// </summary>
    public IBMQObjUserConfig config {get; set;}
    /// <summary>
    /// List of sequence commands that define the experiment
    /// </summary>
    public IBMQObjInstruction[] instructions {get; set;}
}

/// <summary>
/// IBM Quantum Object data structure as defined in https://arxiv.org/pdf/1809.03452.pdf
/// </summary>
public class IBMQObj {
    /// <summary>
    /// User generated run identifier
    /// </summary>
    public string qobj_id {get; set;}
    /// <summary>
    /// Type of experiment, can be either “QASM” for openQASM experiments or “PULSE” for OpenPulse experiments
    /// </summary>
    public IBMQObjType type {get; protected set;}
    /// <summary>
    /// Version of the schema that was used to generate and validate this Qob
    /// </summary>
    public string schema_version {get; set;}
    /// <summary>
    /// List of m experiment sequences to run
    /// </summary>
    public IBMQObjExperiment[] experiments {get; set;}
    /// <summary>
    /// User-defined structure that contains metadata on the job and is not used by the backend
    /// </summary>
    public IBMQObjHeaderMetadata header {get; set;}
    /// <summary>
    /// Configuration settings structure
    /// </summary>
    public IBMQObjUserConfig user_config {get; set;}

    public IBMQObj() {
        schema_version = "1.0";
        user_config = new IBMQObjUserConfig();
    }

    public IBMQObj(string name): this() {
        this.qobj_id = name;
    }
}

}