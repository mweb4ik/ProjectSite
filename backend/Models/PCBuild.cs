using System;  
using System.ComponentModel.DataAnnotations;  
  
namespace PcComponentsApi.Models  
{  
    public class PCBuild  
    {  
        [Key]  
        public string Id { get; set; } = Guid.NewGuid().ToString();  
        [Required]  
        public string UserId { get; set; } = string.Empty;  
        [Required, MaxLength(200)]  
        public string Name { get; set; } = string.Empty;  
        public string ComponentIds { get; set; } = "[]";  
        public decimal TotalPrice { get; set; }  
        public bool IsCompatible { get; set; } = true;  
        public string? CompatibilityNotes { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;  
    }  
} 
