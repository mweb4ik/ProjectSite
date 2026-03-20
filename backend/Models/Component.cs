using System;  
using System.ComponentModel.DataAnnotations;  
  
namespace PcComponentsApi.Models  
{  
    public class Component  
    {  
        [Key]  
        public string Id { get; set; } = Guid.NewGuid().ToString();  
        [Required, MaxLength(200)]  
        public string Name { get; set; } = string.Empty;  
        [Required, MaxLength(50)]  
        public string Category { get; set; } = string.Empty;  
        [MaxLength(100)]  
        public string? Manufacturer { get; set; }  
        public decimal Price { get; set; }  
        public string Specifications { get; set; } = "{}";  
        [MaxLength(50)]  
        public string? Socket { get; set; }  
        public int PowerConsumption { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
    }  
} 
