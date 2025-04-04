using System.ComponentModel.DataAnnotations;
namespace Worktest.backend.Models;
public class Device
{ [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public bool Status { get; set; } // True = On, False = Off
}
