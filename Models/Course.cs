namespace MVCProject.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = new();
}