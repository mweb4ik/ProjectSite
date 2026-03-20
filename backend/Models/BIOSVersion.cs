using System;  
using System.ComponentModel.DataAnnotations;  
  
namespace PcComponentsApi.Models  
{  
    public class BIOSVersion  
    {  
        [Key]  
        public string Id { get; set; } = Guid.NewGuid().ToString();  
        [Required, MaxLength(200)]  
        public string MotherboardModel { get; set; } = string.Empty;  
        [Required, MaxLength(100)]  
        public string Version { get; set; } = string.Empty;  
        public string ReleaseDate { get; set; } = string.Empty;  
        public string Changes { get; set; } = string.Empty;  
        public string DownloadUrl { get; set; } = string.Empty;  
    }  
} 
