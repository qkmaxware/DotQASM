using System.Linq;

namespace DotQasm.Hardware {
    
public class HardwareConfiguration {
    public int PhysicalQubitCount => Connectivity.Vertices.Count();
    public ConnectivityGraph Connectivity {get; set;}
}

}