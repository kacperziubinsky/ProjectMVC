using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCProject.Models;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MVCProject.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Questions
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Questions.Include(q => q.Course);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Questions/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create(int courseId)
        {
            var question = new Question
            {
                CourseId = courseId
            };

            return View(question);
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Text,CourseId,A,B,C,D,CorrectAnswer")] Question question)
        {
            if (!ModelState.IsValid)
            {
                return View(question);
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = question.CourseId });
        }

        // GET: Questions/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", question.CourseId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,CourseId,A,B,C,D,CorrectAnswer")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", question.CourseId);
            return View(question);
        }

        // GET: Questions/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
        

        [HttpPost]
        public async Task<IActionResult> ReadText(FileReadViewModel model)
        {
            if (model.UploadedFile == null || model.UploadedFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Proszę wybrać poprawny plik.";
                return RedirectToAction("Index", "Questions");
            }

            var extension = Path.GetExtension(model.UploadedFile.FileName).ToLower();
            if (extension != ".json")
            {
                TempData["ErrorMessage"] = "Akceptowane są tylko pliki z rozszerzeniem .json.";
                return RedirectToAction("Index", "Questions");
            }

            try
            {
                using (var reader = new StreamReader(model.UploadedFile.OpenReadStream(), Encoding.UTF8))
                {
                    model.FileContent = await reader.ReadToEndAsync();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questions = JsonSerializer.Deserialize<List<Question>>(model.FileContent, options);

                if (questions != null && questions.Any())
                {
                    foreach (var question in questions)
                    {
                        question.Id = 0; 
                        _context.Questions.Add(question);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Pomyślnie dodano {questions.Count} pytań do bazy danych!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Plik JSON jest pusty lub ma niepoprawną strukturę.";
                }
            }
            catch (JsonException)
            {
                TempData["ErrorMessage"] = "Błąd formatowania pliku JSON. Upewnij się, że struktura jest poprawna.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas przetwarzania: {ex.Message}";
            }

            return RedirectToAction("Index", "Questions");
        }

        [HttpGet]
        public async Task<IActionResult> ExportToJson()
        {
            var questions = await _context.Questions.ToListAsync();

            if (!questions.Any())
            {
                TempData["ErrorMessage"] = "Brak pytań w bazie danych do wyeksportowania.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var q in questions)
            {
                q.Course = null;
            }

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            string json = JsonSerializer.Serialize(questions, options);
            
            byte[] fileBytes = Encoding.UTF8.GetBytes(json);
            
            string fileName = $"eksport_pytan_{DateTime.Now:yyyyMMdd_HHmm}.json";
            
            return File(fileBytes, "application/json", fileName);
        }
    }
}

