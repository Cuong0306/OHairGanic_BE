using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Responses
{
    public class GetUserByIdResponse
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public string PhoneNumber { get; set; }
        
        public string Role { get; set; } = "User";
    }
}
