namespace MVCProject.Models;

public class Lesson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public char CorrectAnswer { get; set; }
}