using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCProject.Models;
using MVCProject.Models.ViewModels;

namespace MVCProject.Controllers;

[Authorize]
public class MyAccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public MyAccountController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var courses = await _context.Courses
            .Include(c => c.Questions)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var progressRecords = await _context.CourseProgresses
            .Where(cp => cp.UserId == user.Id)
            .ToDictionaryAsync(cp => cp.CourseId);

        var progressItems = courses.Select(course =>
        {
            progressRecords.TryGetValue(course.Id, out var progress);
            return new CourseProgressItemViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                QuestionsCompleted = progress?.QuestionsCompleted ?? 0,
                TotalQuestions = course.Questions.Count,
                LastUpdated = progress?.LastUpdated
            };
        }).ToList();

        var model = new MyAccountViewModel
        {
            Profile = new EditProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            },
            CourseProgress = progressItems
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MyAccountViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            model.CourseProgress = await BuildProgressListAsync(user.Id);
            return View(model);
        }

        user.DisplayName = model.Profile.DisplayName;
        user.Email = model.Profile.Email;
        user.UserName = model.Profile.Email;
        user.PhoneNumber = model.Profile.PhoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.CourseProgress = await BuildProgressListAsync(user.Id);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.Profile.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(
                user,
                token,
                model.Profile.NewPassword);

            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.CourseProgress = await BuildProgressListAsync(user.Id);
                return View(model);
            }
        }

        TempData["SuccessMessage"] = "Profil został zaktualizowany.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProgress(int courseId, int questionsCompleted)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var course = await _context.Courses
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
        {
            return NotFound();
        }

        var total = course.Questions.Count;
        questionsCompleted = Math.Clamp(questionsCompleted, 0, total);

        var progress = await _context.CourseProgresses
            .FirstOrDefaultAsync(cp => cp.UserId == user.Id && cp.CourseId == courseId);

        if (progress is null)
        {
            progress = new CourseProgress
            {
                UserId = user.Id,
                CourseId = courseId
            };
            _context.CourseProgresses.Add(progress);
        }

        progress.QuestionsCompleted = questionsCompleted;
        progress.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Zaktualizowano postęp kursu „{course.Name}”.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<CourseProgressItemViewModel>> BuildProgressListAsync(string userId)
    {
        var courses = await _context.Courses
            .Include(c => c.Questions)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var progressRecords = await _context.CourseProgresses
            .Where(cp => cp.UserId == userId)
            .ToDictionaryAsync(cp => cp.CourseId);

        return courses.Select(course =>
        {
            progressRecords.TryGetValue(course.Id, out var progress);
            return new CourseProgressItemViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                QuestionsCompleted = progress?.QuestionsCompleted ?? 0,
                TotalQuestions = course.Questions.Count,
                LastUpdated = progress?.LastUpdated
            };
        }).ToList();
    }
}
