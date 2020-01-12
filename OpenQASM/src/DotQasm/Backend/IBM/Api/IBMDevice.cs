using System;

namespace DotQasm.Backend.IBM.Api {

/// <summary>
/// A backend device as defined by IBM's API
/// </summary>
public class IBMDevice {
    public class IBMDeviceGateParametre {
        public DateTime date {get; set;}
        public string name {get; set;}
        public string unit {get; set;}
        public float value {get; set;}
    }
    public class IBMDeviceGate {
        public string gate {get; set;}
        public string name {get; set;}
        public IBMDeviceGateParametre[] parametres {get; set;}
        public int[] qubits {get; set;}
    }

    public string backend_name {get; set;}
    public string backend_version {get; set;}
    public DateTime last_update_date {get; set;}
    public IBMDeviceGate[] gates {get; set;}
    public IBMDeviceGateParametre[][] qubits {get; set;}
}

}