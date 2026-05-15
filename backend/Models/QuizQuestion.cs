namespace PcComponentsApi.Models;

public class QuizQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Question { get; set; } = string.Empty;

    public string[] Options { get; set; } = Array.Empty<string>();

    public int CorrectOptionIndex { get; set; } 

    public string Difficulty { get; set; } = string.Empty;
}