using System;
using System.Collections.Generic;

namespace DotQASM.Desktop.Models {

    public class ProviderModel {
        public string[] Backends {get; set;}
        public string Name {get; set;}
    }

    public class ApplicationConfigModel {
        public ProviderModel[] Providers {get; set;}
    }
}
