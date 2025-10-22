using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Responses
{
    public class CaptureResponse
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? Angle { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime TakenAt { get; set; }
    }
}
