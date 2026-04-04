namespace PcComponentsApi.Models;
public class QuizResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime? CompletedAt { get; set; }
}