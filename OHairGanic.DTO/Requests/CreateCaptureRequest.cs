using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Requests
{
    public class CreateCaptureRequest
    {
        public int UserId { get; set; }
        public string? Angle { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime TakenAt { get; set; }
    }
}
