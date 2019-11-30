using System.IO;

namespace DotQasm.IO {

public interface IParser<C> {
    C Parse(TextReader program);
}

}