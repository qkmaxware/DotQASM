using System;
using System.Collections.Generic;
using DotQasm.Compile.Generators;

namespace DotQasm.Compile.Templates {

public class MaxCutTemplate : ICircuitTemplate {

    public string TemplateName => "MaxCut";
    
    public Circuit GetTemplateCircuit() {
        // Create graph, similar to the actual hardware
        EdgeListGraph<int, object> graph = new EdgeListGraph<int, object>();
        graph.Add(0); graph.Add(1); graph.Add(2); graph.Add(3); graph.Add(4);
        // Triangle plus edge 
        graph.DirectedEdge(2, 1, default(object));
        graph.DirectedEdge(2, 4, default(object));
        graph.DirectedEdge(2, 3, default(object));
        graph.DirectedEdge(3, 4, default(object));

        // Create args
        var args = new QaoaArguments<object>(){
            RoundsOfOptimization    = 2,
            GammaAngles             = new float[] { (float)(0.2 * Math.PI),     (float)(0.4 * Math.PI)  },
            BetaAngles              = new float[] { (float)(0.15 * Math.PI),    (float)(0.05 * Math.PI) },
            Graph                   = graph
        };

        return (new MaxCutGenerator<object>()).Generate(args);
    }
}

}