using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Requests
{
    public class AnalyzeByUrlRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Angle { get; set; } = string.Empty;
    }
}
