
using System.Security.Claims;

using InternManagement.Models;
using Microsoft.AspNetCore.Authorization;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Controllers
{
    //[Authorize(Roles = "Student")]
    public class StudentTasksController : Controller
    {
        private readonly InternmanagementContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentTasksController(InternmanagementContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = 1;

            var student = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (student == null)
            {
                return NotFound();
            }

            // Get tasks assigned to student or their group
            var tasks = await _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Mentor)
                .Where(t => t.StudentId == currentUserId || t.GroupId == student.GroupId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = 1;

            var task = await _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Mentor)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            // Check if task is assigned to this student or their group
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (task.StudentId != currentUserId && task.GroupId != student.GroupId)
            {
                return Forbid();
            }

            // Get student's submissions for this task
            var submissions = await _context.Tasksubmits
                .Where(ts => ts.TaskId == id && ts.StudentId == currentUserId)
                .OrderByDescending(ts => ts.SubmittedAt)
                .ToListAsync();

            // Get daily tracking entries for this task
            var dailyReports = await _context.Dailytrackings
                .Where(dt => dt.TaskId == id && dt.StudentId == currentUserId)
                .OrderByDescending(dt => dt.CreatedAt)
                .ToListAsync();

            ViewBag.Submissions = submissions;
            ViewBag.DailyReports = dailyReports;

            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.TaskId = id;
            ViewBag.TaskTitle = task.Title;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int taskId, IFormFile submittedFile, string remarks)
        {
            var currentUserId = 1;

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound();
            }

            string filePath = null;
            if (submittedFile != null && submittedFile.Length > 0)
            {
                // Save the file
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/submissions");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(submittedFile.FileName);
                filePath = Path.Combine(uploadsFolder, uniqueFileName).Replace("\\", "/");

                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await submittedFile.CopyToAsync(fileStream);
                }

                // Store relative path
                filePath = "/uploads/submissions/" + uniqueFileName;
            }

            var taskSubmit = new Tasksubmit
            {
                TaskId = taskId,
                StudentId = currentUserId,
                SubmittedFile = filePath,
                Status = "Submitted",
                Remarks = remarks
            };

            _context.Tasksubmits.Add(taskSubmit);
            await _context.SaveChangesAsync();

            // Create notification for mentor
            var notification = new Notification
            {
                UserId = task.MentorId,
                Content = $"New submission for task '{task.Title}'",
                StatusId = 1 // Assuming 1 is the status ID for "New"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        [HttpGet]
        public async Task<IActionResult> DailyTracking(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.TaskId = id;
            ViewBag.TaskTitle = task.Title;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DailyTracking(int taskId, string report)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var tracking = new Dailytracking
            {
                TaskId = taskId,
                StudentId = currentUserId,
                Report = report
            };

            _context.Dailytrackings.Add(tracking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = taskId });
        }
    }
}