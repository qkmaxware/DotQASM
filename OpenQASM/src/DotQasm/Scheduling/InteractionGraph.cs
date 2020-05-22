using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DotQasm.Scheduling {

public class Interaction {
    public IEvent Event;
    public int Colour;

    public void ClearColour() {
        this.Colour = default(int);
    }
    public bool HasColour() {
        return this.Colour != default(int);
    }
}

public class InteractionGraph: EdgeListGraph<Qubit, Interaction> {

    public InteractionGraph(IEnumerable<DataPrecedenceNode> nodes) {
        // Add qubits
        var logicals = nodes.SelectMany(node => node.Event.QuantumDependencies).Distinct();
        foreach (var logical in logicals) {
            this.Add(logical);
        }

        // Add edges
        foreach (var (evt, index) in nodes.Select((node, index) => (node.Event, index))) {
            var interaction = new Interaction() {
                Event = evt,
                Colour = default(int)
            };

            // Add undirected edge for interaction
            var qubits = evt.QuantumDependencies.ToList();
            if (qubits.Count == 1) {
                // Single-qubit self-operation
                this.UndirectedEdge(qubits[0], qubits[0], interaction);
            } else if (qubits.Count > 1) {
                // Multi-qubit spanning operations, create links from control to all targets, use same interaction for all
                for (int i = 1; i < qubits.Count; i++) {
                    this.UndirectedEdge(qubits[0], qubits[i], interaction);
                }
            }
        }
    }

    private void ClearColours() {
        foreach (var edge in this.Edges) {
            edge.Data.ClearColour();
        }
    }

    public void AssignColours() {
        BreadthFirstColourSharing();
    }

   private void BreadthFirstColourSharing() {
        if (this.VertexCount > 0) {
            foreach (var edge in this.Edges) {
                if (edge.Data.HasColour()) {
                    continue;
                }

                var startEdges = this.IncidentEdges(edge.Startpoint);
                var endEdges = this.IncidentEdges(edge.Endpoint);

                var colour = 1; // Always bias towards 1 (force more things to be 1 than not 1)

                var coloursItCantBe = startEdges.Select(e => e.Data.Colour).Concat(endEdges.Select(e => e.Data.Colour)).ToList();

                while (coloursItCantBe.Contains(colour)) {
                    colour++;
                }

                edge.Data.Colour = colour;
            }
        }
    }
    
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach (var edge in this.Edges.DistinctBy((e) => e.Data)) {
            var start = edge.Startpoint;
            var end = edge.Endpoint;

            sb.Append('q').Append(start.QubitId);
            sb.Append(" -> ");
            sb.Append('q').Append(end.QubitId);
            sb.Append(" [").Append("colour=").Append(edge.Data.Colour).Append(",event=").Append(edge.Data.Event.Name).Append(']');

            sb.Append(System.Environment.NewLine);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Assign colours using the Misra & Gries algorithm
    /// https://en.wikipedia.org/wiki/Misra_%26_Gries_edge_coloring_algorithm
    /// </summary>
    /*public void MisraGriesEdgeColouring() {
        /*
            input: A graph G.
            output: A proper coloring c of the edges of G.

            Let U := E(G)

            while U ≠ ∅ do
                Let (u, v) be any edge in U.  
                Let F[1:k] be a maximal fan of u starting at F[1] = v.
                Let c be a color that is free on u and d be a color that is free on F[k].  
                Invert the cdu path  
                Let w ∈ V(G) be such that w ∈ F, F' = [F[1]...w] is a fan and d is free on w.  
                Rotate F' and set c(u, w) = d.
                U := U − {(u, v)}
            end while
        * /

        
    }*/

}

}