using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Responses
{
    public class HairAnalyzeResponse
    {
        public string Label { get; set; } = string.Empty;
        public float Oiliness { get; set; }
        public float Dryness { get; set; }
        public float DandruffScore { get; set; }
    }
}
