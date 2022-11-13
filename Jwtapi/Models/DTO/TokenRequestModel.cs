using System.ComponentModel.DataAnnotations;

namespace Jwtapi.Models.DTO
{
    public class TokenRequestModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
