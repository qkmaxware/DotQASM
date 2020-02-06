using System;
using System.Linq;
using System.Collections.Generic;

namespace DotQASM.Desktop.Models {

    public class PropertyModel {
        public string Name {get; set;}
        public string Value {get; set;}

        public PropertyModel(string name) {
            this.Name = name;
        }
    }

    public class SectionModel {
        public List<PropertyModel> Properties {get; set;}
        public string Name {get; set;}

        public SectionModel(string section) {
            this.Name = section;
            this.Properties = new List<PropertyModel>();
        }
    }

    public class RegionModel {
        public List<SectionModel> Sections {get; set;}
        public string Name {get; set;}

        public RegionModel(string region) {
            this.Name = region;
            this.Sections = new List<SectionModel>();
        }
    }

    public class ServerConfigModel {
        public List<RegionModel> Regions {get; set;}

        public ServerConfigModel() {
            Regions = new List<RegionModel>();

            var providers = new RegionModel("Providers");
            foreach (var provider in DotQasm.Tools.Commands.Run.Providers) {
                var section = new SectionModel(provider.ProviderName);

                section.Properties.Add(new PropertyModel("Access Token"));

                providers.Sections.Add(section);
            }
            Regions.Add(providers);

        }

        public PropertyModel FindProperty(string region, string section, string value) {
            return Regions
            .Where(reg => reg.Name.ToLower() == region.ToLower()).SelectMany(reg => reg.Sections)
            .Where(sec => sec.Name.ToLower() == section.ToLower()).SelectMany(sec => sec.Properties)
            .Where(prop => prop.Name.ToLower() == value.ToLower())
            .FirstOrDefault();
        }
    }
}
