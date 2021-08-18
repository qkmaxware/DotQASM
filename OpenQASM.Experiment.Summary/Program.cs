using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenQASM.Experiment.Summary {
    public class FieldSummary {
        public bool Any() => Values != null && Values.Count > 0;
        public List<double> Values = new List<double>();
        public double Sum => Values.Sum();
        public int Count => Values.Count;
        public double Mean => Sum / Count;
        public double Variance {
            get {
                var mean = this.Mean;
                var sum = Values.Select(x => x - mean).Sum();
                var sumSquared = sum * sum;
                return sumSquared / (Count - 1); //-1 because these are a sample of an infinite number of runs
            }
        }
        public double StandardDeviation => Math.Sqrt(Variance);
    }

    public class Summary {
        public string Filename;
        public List<string> Files = new List<string>();
        public Csv<FieldSummary> Fields = new Csv<FieldSummary>();
    }

    public class Csv<Cell> where Cell:new() {
        public Dictionary<string, Dictionary<string, Cell>> data = new Dictionary<string, Dictionary<string, Cell>>();
        public IEnumerable<string> Rows => data.Keys;
        public int RowCount => data.Count;
        public IEnumerable<string> Columns => data.SelectMany(row => row.Value.Keys).Distinct();
        public Cell GetOrAdd(string row, string column) {
            if (data.ContainsKey(row)) {
                var columns = data[row];
                if (columns.ContainsKey(column)) {
                    return columns[column];
                } else {
                    var field = new Cell();
                    columns[column] = field;
                    return field;
                }
            } else {
                var columns = new Dictionary<string, Cell>();
                var field = new Cell();
                columns[column] = field;
                data[row] = columns;
                return field;
            }
        }
        public bool TryGet(string row, string column, out Cell cell) {
            if (data.ContainsKey(row)) {
                var columns = data[row];
                if (columns.ContainsKey(column)) {
                    cell = columns[column];
                    return true;
                } else {
                    cell = default(Cell);
                    return false;
                }
            } else {
                cell = default(Cell);
                return false;
            }
        }
    }

    class Program {
        static void Main(string[] args) {
            if (!Directory.Exists(".qasmdata/experiments")) {
                Console.WriteLine("No experiments to summarize");
                return;
            }

            var trials = Directory.GetDirectories(".qasmdata/experiments").ToList();
            if (trials.Count < 1) {
                Console.WriteLine("No experiments to summarize");
                return;
            }   

            Console.WriteLine("Select an experiment number below");
            for (var i = 0; i < trials.Count; i++) {
                Console.WriteLine($"> {i}  '{trials[i]}'");
            }
            var trialNumber = int.Parse(Console.ReadLine());
            var trial = trials[trialNumber];
            Console.WriteLine();

            var runs = Directory.GetFiles(trial, "matrix.*.csv", SearchOption.AllDirectories);
            var fileNames = runs.Select(path => Path.GetFileName(path)).Distinct();
            Console.WriteLine("Summarizing");
            foreach (var name in fileNames) {
                Console.WriteLine($"> {name}");
            }

            var summaries = fileNames.Select(fileName => new Summary { Filename = fileName }).ToDictionary(summary => summary.Filename);
            foreach (var file in runs) {
                var summary = summaries[Path.GetFileName(file)];
                summary.Files.Add(file);

                string line;
                bool isFirstLine = true;
                Dictionary<int, string> headers = new Dictionary<int, string>();
                using (var reader = new StreamReader(file)) {
                    // Read file line by line
                    while ((line = reader.ReadLine()) != null) {
                        var cells = line.Split(',');

                        if (isFirstLine) {
                            for (var i = 0; i < cells.Length; i++) {
                                headers[i] = cells[i];
                            }
                            isFirstLine = false;
                        } else {
                            var rowLabel = cells[0];
                            for (var i = 1; i < cells.Length; i++) {
                                var columnLabel = headers[i];
                                
                                double numericValue;
                                string textValue = cells[i];
                                if (TryParseCellValue(textValue, out numericValue)) {
                                    FieldSummary field = summary.Fields.GetOrAdd(rowLabel, columnLabel);
                                    field.Values.Add(numericValue);
                                }
                            }
                        }
                    }
                }
            }

            var outputDir = Path.Combine(trial, "summary");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            foreach (var summary in summaries) {
                SaveSummaryCsv(Path.Combine(outputDir, $"sum.{summary.Key}"), summary.Value, (f) => f.Sum);
                SaveSummaryCsv(Path.Combine(outputDir, $"mean.{summary.Key}"), summary.Value, (f) => f.Mean);
                SaveSummaryCsv(Path.Combine(outputDir, $"sd.{summary.Key}"), summary.Value, (f) => f.StandardDeviation);
                SaveSummaryCsv(Path.Combine(outputDir, $"runs.{summary.Key}"), summary.Value, (f) => f.Count);
            }
        }
    
    
        private static bool TryParseCellValue(string value, out double d) {
            d = default(double);
            if (string.IsNullOrEmpty(value))
                return false;

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,  out d)) {
                return true;
            } else {
                return false;
            }
        }
    
        private static void SaveSummaryCsv(string path, Summary summary, Func<FieldSummary, double> selector) {
            if (summary.Fields.RowCount < 1)
                return;
                
            using (var writer = new StreamWriter(path)) {
                var rows = summary.Fields.Rows.ToList();
                var columns = summary.Fields.Columns.ToList();

                // Print column headers
                writer.Write("Circuit Id");
                foreach (var column in columns) {
                    writer.Write($",{column}");
                }
                writer.WriteLine();

                // Do each row
                foreach (var row in rows) {
                    writer.Write(row);
                    foreach (var column in columns) {
                        FieldSummary field;
                        if (summary.Fields.TryGet(row, column, out field)) {
                            if (field.Any()) {
                                var value = selector(field);
                                writer.Write($",{value}");
                            } else {
                                writer.Write(",");
                            }
                        } else {
                            writer.Write(",");
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}
