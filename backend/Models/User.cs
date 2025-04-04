using System.ComponentModel.DataAnnotations;

namespace Worktest.backend.Models
{
    public class User
    {
        // Primärnyckel
        [Key]
        public int Id { get; set; }

        // Användarnamn som unikt identifierar användaren
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty; // Standardvärde för att undvika null

        // Hashat lösenord (vi kommer att använda hashning, så lagra aldrig lösenord som klartext)
        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Standardvärde för att undvika null

        // Roll för att definiera användarens rättigheter, t.ex. "Admin", "User"
        [StringLength(50)]
        public string Role { get; set; } = "User"; // Standardvärde "User" för att minimera null-risk
    }
}
