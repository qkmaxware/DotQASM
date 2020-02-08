using System;
using System.Collections.Generic;

namespace DotQASM.Desktop.Models {

    public class CodeModel {
        public string qasm {get; set;}
    }

    public class SubmissionModel {
        public IEnumerable<string> qasms {get; set;}
        public string provider {get; set;}
        public string backend {get; set;}
    }
}
