using System.ComponentModel.DataAnnotations;

namespace Worktest.backend.Models
{
    public class RegisterModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]  // Minimum password length for security
        public string Password { get; set; }

        
    }
}
