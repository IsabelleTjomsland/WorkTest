using System.ComponentModel.DataAnnotations;

namespace Worktest.backend.Models
{
    public class LoginModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]  // Password length for validation
        public string Password { get; set; }
    }
}
