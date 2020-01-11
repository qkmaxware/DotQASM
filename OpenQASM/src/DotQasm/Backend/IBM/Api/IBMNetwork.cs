using System;
using System.Collections.Generic;

namespace DotQasm.Backend.IBM.Api {

public class IBMNetwork {
    public class Group {
        public string name {get; set;}    
        public string title {get; set;}  
        public string description {get; set;} 
        public DateTime creationDate {get; set;}   
        public bool deleted {get; set;}  
        public Dictionary<string,Project> projects {get; set;}
    }

    public class Project {
        public string name {get; set;}    
        public string title {get; set;}  
        public string description {get; set;} 
        public DateTime creationDate {get; set;}   
        public bool deleted {get; set;}  
        public Dictionary<string, ProjectDevice> devices {get; set;}
    }

    public class ProjectDevice {
        public int priority {get; set;}
        public bool deleted {get; set;}
        public string name {get; set;}
    }

    public string name {get; set;}    
    public string title {get; set;}  
    public string description {get; set;} 
    public DateTime creationDate {get; set;}   
    public bool deleted {get; set;}  
    public bool @private {get; set;}  
    public bool licenseNotRequired {get; set;}  
    public bool isDefault {get; set;} 
    public bool analytics {get; set;}   
    public string @class {get; set;}  
    public string id {get; set;}  
    public Dictionary<string, Group> groups {get; set;}
}

}