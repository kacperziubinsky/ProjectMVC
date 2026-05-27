namespace MVCProject.Models;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Course Course { get; set; } = null!;
    public int CourseId { get; set; }



    public string A { get; set; } = string.Empty;
    public string B { get; set; } = string.Empty;
    public string C { get; set; } = string.Empty;
    public string D { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;
}