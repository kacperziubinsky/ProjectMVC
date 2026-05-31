using Microsoft.AspNetCore.Identity;

namespace MVCProject.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
}
