using System;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM.Api {

/// <summary>
/// Measurement type as defined by the IBM api
/// </summary>
public enum IBMApiJobMeasurementDataType {
    None,
    Histogram, 
    Memory,
    StateVector,
    UnitaryMatrix,
}

/// <summary>
/// Measurement results as defined by the IBM api
/// </summary>
public class IBMApiJobMeasurementData {
    /// <summary>
    /// Histogram of counts in the different memory states
    /// </summary>
    public Dictionary<string, int> counts {get; set;}
    /// <summary>
    /// State of the classical memory
    /// </summary>
    public int[] memory {get; set;}
    /// <summary>
    /// Final statevector corresponding to evolution of the zero state
    /// </summary>
    public float[][][] statevector {get; set;}
    /// <summary>
    /// Final unitary matrix corresponding to the quantum circuit
    /// </summary>
    public float[][][] unitary {get; set;}    

    public IBMApiJobMeasurementDataType Type {
        get {
            if (counts != null) {
                return IBMApiJobMeasurementDataType.Histogram;
            } else if (memory != null) {
                return IBMApiJobMeasurementDataType.Memory;
            } else if (statevector != null) {
                return IBMApiJobMeasurementDataType.StateVector;
            } else if (unitary != null) {
                return IBMApiJobMeasurementDataType.UnitaryMatrix;
            } else {
                return IBMApiJobMeasurementDataType.None;
            }
        }
    }
}

/// <summary>
/// Experiment data as defined by the IBM api
/// </summary>
public class IBMApiJobExperimentResult {
    public int shots {get; set;}
    public string status {get; set;}
    public bool success {get; set;}
    public IBMQObjHeaderMetadata header {get; set;}
    public int seed {get; set;}
    public IBMApiJobMeasurementData data {get; set;}
}

/// <summary>
/// Quantum object results as defined by the IBM api
/// </summary>
public class IBMApiQObjectResult {
    /// <summary>
    /// Backend identifier name
    /// </summary>
    public string backend_name {get; set;}
    /// <summary>
    /// Backend identifier version
    /// </summary>
    public string backend_version {get; set;}
    /// <summary>
    /// User generated id corresponding to the qobj id in the Qobj
    /// </summary>
    public string qobj_id {get; set;}
    /// <summary>
    /// Unique backend job identifier corresponding to these results
    /// </summary>
    public string job_id {get; set;}
    /// <summary>
    /// Date when the job was run
    /// </summary>
    public DateTime date {get; set;}
    /// <summary>
    /// Header structure for the job that was passed in with the Qobj
    /// </summary>
    public IBMQObjHeaderMetadata header {get; set;}
    /// <summary>
    /// Status of the job
    /// </summary>
    public string status {get; set;}
    /// <summary>
    /// If the job was successful or not
    /// </summary>
    public bool success {get; set;}
    /// <summary>
    /// Number of seconds taken
    /// </summary>
    public double time_taken {get; set;}

    // -- Results of each experiment go here
    public IBMApiJobExperimentResult[] results {get; set;}
}

}