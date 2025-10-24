using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OHairGanic.DTO.Constants.ApiRoutes;

namespace OHairGanic.DTO.Requests
{
    public class GetAllPaymentRequest
    {
        public int OrderId { get; set; }

        public float Amount { get; set; }

        public string Currency { get; set; }

        public string Provider { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
