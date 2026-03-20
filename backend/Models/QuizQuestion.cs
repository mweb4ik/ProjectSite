using System;  
using System.ComponentModel.DataAnnotations;  
  
namespace PcComponentsApi.Models  
{  
    public class QuizQuestion  
    {  
        [Key]  
        public string Id { get; set; } = Guid.NewGuid().ToString();  
        [Required]  
        public string Question { get; set; } = string.Empty;  
        public string Options { get; set; } = "[]";  
        public int CorrectAnswer { get; set; }  
        [MaxLength(50)]  
        public string? Category { get; set; }  
        public int Difficulty { get; set; } = 1;  
    }  
} 
