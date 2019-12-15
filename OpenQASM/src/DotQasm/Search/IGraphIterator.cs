using System.Collections.Generic;

namespace DotQasm.Search {

public class IGraphEdge<T> {
   public IGraphIterator<T> Node {get; private set;}
   public int Weight {get; private set;}

   public IGraphEdge(int weight, IGraphIterator<T> iterator) {
      this.Weight = weight;
      this.Node = iterator;
   }
}

public interface IGraphIterator<T> {
   IEnumerable<IGraphEdge<T>> Next {get;}
   T Current {get;}
}

}