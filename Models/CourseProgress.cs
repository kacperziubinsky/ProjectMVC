namespace MVCProject.Models;

public class CourseProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int QuestionsCompleted { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
