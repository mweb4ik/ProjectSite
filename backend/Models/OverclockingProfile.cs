using System;  
using System.ComponentModel.DataAnnotations;  
  
namespace PcComponentsApi.Models  
{  
    public class OverclockingProfile  
    {  
        [Key]  
        public string Id { get; set; } = Guid.NewGuid().ToString();  
        [Required]  
        public string UserId { get; set; } = string.Empty;  
        [Required, MaxLength(200)]  
        public string ComponentName { get; set; } = string.Empty;  
        [MaxLength(50)]  
        public string ComponentType { get; set; } = string.Empty;  
        public int BaseClock { get; set; }  
        public int OverclockedClock { get; set; }  
        public double Voltage { get; set; }  
        public int Temperature { get; set; }  
        public int StabilityScore { get; set; }  
        public string Notes { get; set; } = string.Empty;  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
    }  
} 
