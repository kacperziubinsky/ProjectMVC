using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCProject.Models;

namespace MVCProject.Controllers;

[Authorize]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuizController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Quiz
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses.ToListAsync();
        return View(courses);
    }

    // GET: Quiz/Start/5
    public async Task<IActionResult> Start(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var course = await _context.Courses
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        if (!course.Questions.Any())
        {
            TempData["Error"] = "Ten kurs nie ma jeszcze pytań.";
            return RedirectToAction(nameof(Index));
        }

        var progress = await _context.CourseProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == id);

        if (progress != null && (double)progress.QuestionsCompleted /course.Questions.Count > 0.5 )
        {
            TempData["Error"] = "Ten kurs został już ukończony.";
            return RedirectToAction(nameof(Index));
        }
        
        if (progress == null)
        {
            progress = new CourseProgress
            {
                UserId = userId,
                CourseId = id,
                QuestionsCompleted = 0,
                LastUpdated = DateTime.UtcNow
            };

            _context.CourseProgresses.Add(progress);
        }
        else
        {
            progress.QuestionsCompleted = 0;
            progress.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData[$"Quiz_{id}_Score"] = 0;

        return RedirectToAction(nameof(Question), new
        {
            courseId = id,
            index = 0
        });
    }

    // GET: Quiz/Question?courseId=5&index=0
    public async Task<IActionResult> Question(int courseId, int index)
    {
        var course = await _context.Courses
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return NotFound();

        var questions = course.Questions.ToList();

        if (index < 0 || index >= questions.Count)
            return RedirectToAction(nameof(Result), new { courseId });

        ViewBag.CourseId = courseId;
        ViewBag.CourseName = course.Name;
        ViewBag.Index = index;
        ViewBag.Total = questions.Count;

        return View(questions[index]);
    }

    // POST: Quiz/Answer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Answer(int courseId, int index, char selectedAnswer)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var course = await _context.Courses
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return NotFound();

        var questions = course.Questions.ToList();

        if (index < 0 || index >= questions.Count)
            return RedirectToAction(nameof(Result), new { courseId });

        var currentQuestion = questions[index];

        var scoreKey = $"Quiz_{courseId}_Score";
        var score = TempData[scoreKey] as int? ?? 0;

        if (char.ToUpper(currentQuestion.CorrectAnswer) == char.ToUpper(selectedAnswer))
        {
            score++;
        }

        TempData[scoreKey] = score;

        var progress = await _context.CourseProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

        if (progress != null)
        {
            progress.QuestionsCompleted++;
            progress.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var nextIndex = index + 1;

        if (nextIndex >= questions.Count)
        {
            return RedirectToAction(nameof(Result), new { courseId });
        }

        return RedirectToAction(nameof(Question), new
        {
            courseId,
            index = nextIndex
        });
    }

    // GET: Quiz/Result?courseId=5
    public async Task<IActionResult> Result(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var course = await _context.Courses
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return NotFound();

        var progress = await _context.CourseProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);

        var scoreKey = $"Quiz_{courseId}_Score";
        var score = TempData[scoreKey] as int? ?? 0;

        ViewBag.CourseName = course.Name;
        ViewBag.Score = score;
        ViewBag.Total = course.Questions.Count;
        ViewBag.QuestionsCompleted = progress?.QuestionsCompleted ?? 0;

        return View();
    }
}