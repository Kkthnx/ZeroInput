using System.Collections.Generic;

namespace ZeroInput.Models
{
    public class ConfigData
    {
        public AppSettings Settings { get; set; } = new AppSettings();
        public List<BlockRule> Rules { get; set; } = new List<BlockRule>();
    }
}