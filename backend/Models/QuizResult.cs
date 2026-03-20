using System;  
using System.ComponentModel.DataAnnotations;  
  
namespace PcComponentsApi.Models  
{  
    public class QuizResult  
    {  
        [Key]  
        public string Id { get; set; } = Guid.NewGuid().ToString();  
        [Required]  
        public string UserId { get; set; } = string.Empty;  
        public int Score { get; set; }  
        public int TotalQuestions { get; set; }  
        public string Answers { get; set; } = "{}";  
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;  
    }  
} 
