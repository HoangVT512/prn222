
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternManagement.Models;
using System.Data;
using InternManagement.Models.ViewModels;

namespace InternManager.Controllers
{
    public class StudentsController : Controller
    {
        private readonly InternmanagementContext _context;

        public StudentsController(InternmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string status, string field)
        {
            var students = _context.Users
                .Where(u => u.Role.Name == "Intern")
                .Include(s => s.Group)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                students = students.Where(s => s.FullName.Contains(search));
            }

            if (!string.IsNullOrEmpty(field))
            {
                students = students.Where(s => s.Group.Field.Equals(field));
            }
            return View(await students.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Users
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == id && s.Role.Name == "Intern");

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User model, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string filePath = Path.Combine("wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    model.Pfp = "/uploads/" + fileName;
                }

                _context.Users.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Users.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, User model, IFormFile file)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string filePath = Path.Combine("wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    model.Pfp = "/uploads/" + fileName;
                }
                _context.Users.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Users.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Users.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = 1; // Lấy từ session hoặc User.Identity sau này
            var tasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => t.StudentId == userId || t.GroupId != null)
                .ToListAsync();

            var trackingHistory = await _context.Dailytrackings
                .Where(t => t.StudentId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var viewModel = new StudentDashboardViewModel
            {
                PendingTasks = tasks,
                RecentTracking = trackingHistory,
                RecentNotifications = notifications
            };

            return View(viewModel);

        }
    }
}

