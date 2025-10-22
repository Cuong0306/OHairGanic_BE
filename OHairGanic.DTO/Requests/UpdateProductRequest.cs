using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Requests
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }

        public string? Tags { get; set; }

        public float? Price { get; set; }

        public string? ImageUrl { get; set; }
        public int? Stock { get; set; }

        public bool? IsActive { get; set; }
    }
}
