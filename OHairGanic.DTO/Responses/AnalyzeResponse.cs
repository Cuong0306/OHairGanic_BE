using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Responses
{
    public class AnalyzeResponse
    {
        public int Id { get; set; }

        public int CaptureId { get; set; }

        public double? Oiliness { get; set; }

        public double? Dryness { get; set; }

        public double? DandruffScore { get; set; }

        public string? Label { get; set; }

        public string? ModelVersion { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
