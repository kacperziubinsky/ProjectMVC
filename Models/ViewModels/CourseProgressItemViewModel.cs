namespace MVCProject.Models.ViewModels;

public class CourseProgressItemViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int QuestionsCompleted { get; set; }
    public int TotalQuestions { get; set; }
    public int PercentComplete => TotalQuestions == 0
        ? 0
        : (int)Math.Round(100.0 * QuestionsCompleted / TotalQuestions);
    public DateTime? LastUpdated { get; set; }
}
