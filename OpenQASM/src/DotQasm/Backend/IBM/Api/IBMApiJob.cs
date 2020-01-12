using System;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM.Api {
    
    /// <summary>
    /// API Job as defined by IBM
    /// </summary>
    public class IBMApiJob {
        public class IBMApiJobStorageInfo {
            public string jobId {get; set;}
            public string downloadQObjectUrlEndpoint {get; set;}
            public string validatedUrl {get; set;}
            public string validationUploadUrlEndpoint {get; set;}
            public string uploadQobjectUrlEndpoint {get; set;}
            public string jobQasmConverted {get; set;}
        }
        public class IBMJobSubmitter {
            public string ip {get; set;}
            public string city {get; set;}
            public string country {get; set;}
            public string continent {get; set;}
        }

        public string id {get; set;}
        public string[] qasms {get; set;}
        public string kind {get; set;}
        public int shots {get; set;}
        public string status {get; set;}
        public int cost {get; set;}
        public DateTime creationDate {get; set;}
        public IBMApiJobStorageInfo objectStorageInfo {get; set;}
        public Dictionary<string, DateTime> timePerStep {get; set;}
        public IBMJobSubmitter ip {get; set;}

        public bool IsCreating() {
            return (status ?? string.Empty) == "CREATING";
        }

        public bool IsComplete() {
            return (status ?? string.Empty) == "COMPLETE";
        }
    }

}