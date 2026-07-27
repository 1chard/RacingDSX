using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RacingDualSense.Config
{
    public class Config
    {
        public bool DisableAppCheck { get; set; }
        public VerboseLevel VerboseLevel { get; set; } = VerboseLevel.Off;
        public Theme Theme { get; set; } = Theme.Auto;
        public Dictionary<String, Profile> Profiles { get; set; } = new Dictionary<String, Profile>();
        [JsonIgnore]
        public Profile ActiveProfile { get; set; } = null;
        public int? DSXPort { get; set; } = null; // This sets the default dsx port
        public String DefaultProfile { get; set; } = "Forza";
        public string Version { get; set; }
    }
}
