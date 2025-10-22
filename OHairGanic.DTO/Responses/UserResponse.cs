using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Responses
{
    public class UserResponse
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
