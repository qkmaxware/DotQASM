using System;
using System.IO;
using System.Linq;
using System.Globalization;

namespace csvpostformat {
    class Program {
        static void Main(string[] args) {
            var path = Path.Combine("..", ".qasmdata", "experiments" ,"1", "Dell Alienware Intel i7 8700K");
            foreach (var dir in Directory.GetDirectories(path)) {
                foreach (var file in Directory.GetFiles(dir, "*.csv")) {
                    // Ignore already formatted files, only convert matrix files
                    if (!file.Contains(".formatted.csv") && file.Contains("matrix."))
                        format (file);
                }
            }
        }

        static void format(string file) {
            using (var fs = new StreamReader(file)) 
            using (var ws = new StreamWriter(file + ".formatted.csv")) {
                string line = null;
                while ((line = fs.ReadLine()) != null) {
                    var parts = line.Split(',');
                    var formatted = parts.Select((part) => {
                        TimeSpan span;
                        if(TimeSpan.TryParseExact(
                            input: part,
                            format: @"g",
                            formatProvider: CultureInfo.CurrentCulture,
                            out span
                        )) {
                            return span.TotalSeconds.ToString("F10").TrimEnd('0');
                        } else {
                            return part;
                        }
                    });
                    ws.WriteLine(string.Join(",", formatted));
                }
            }
        }
    }
}
