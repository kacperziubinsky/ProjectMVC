namespace MVCProject.Models.ViewModels;

public class MyAccountViewModel
{
    public EditProfileViewModel Profile { get; set; } = new();
    public List<CourseProgressItemViewModel> CourseProgress { get; set; } = new();
}
