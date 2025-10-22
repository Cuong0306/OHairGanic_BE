using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Requests
{
    public class RegisterRequest
    {
        [DefaultValue("Full name")]
        public string? FullName { get; set; }
        [DefaultValue("example@email.com")]
        public string? Email { get; set; }
        [DefaultValue("Your_password")]
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        //[DefaultValue("Active")]
        //public string? Status { get; set; }
        //[DefaultValue("User")]
        //public string? Role { get; set; }
    }
}
