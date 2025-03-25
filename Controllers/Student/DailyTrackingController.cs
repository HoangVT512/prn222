
using System.Security.Claims;
using InternManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Controllers
{
    //[Authorize(Roles = "Student")]
    public class DailyTrackingController : Controller
    {
        private readonly InternmanagementContext _context;

        public DailyTrackingController(InternmanagementContext context)
        {
            _context = context;
        }

        // GET: DailyTracking
        public async Task<IActionResult> Index()
        {
            var currentUserId = 1;

            var dailyTrackings = await _context.Dailytrackings
                .Include(d => d.Task)
                .Where(d => d.StudentId == currentUserId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(dailyTrackings);
        }

        // GET: DailyTracking/Create
        public async Task<IActionResult> Create()
        {
            var currentUserId = 1;

            // Check if user already has a tracking entry for today
            var today = DateTime.Today;
            var existingTracking = await _context.Dailytrackings
                .FirstOrDefaultAsync(d => d.StudentId == currentUserId &&
                                        d.CreatedAt.Date == today);

            if (existingTracking != null)
            {
                // Redirect to edit if there's already an entry for today
                return RedirectToAction(nameof(Edit), new { id = existingTracking.Id });
            }

            // Get student's assigned tasks for dropdown
            var studentTasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => (t.StudentId == currentUserId ||
                           t.GroupId == _context.Users
                                          .Where(u => u.Id == currentUserId)
                                          .Select(u => u.GroupId)
                                          .FirstOrDefault()) &&
                           t.Status.Name != "Completed")
                .ToListAsync();

            ViewBag.TaskId = new SelectList(studentTasks, "Id", "Title");

            return View();
        }

        // POST: DailyTracking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskId,Report")] Dailytracking dailyTracking)
        {
            var currentUserId = 1;

            // Check if user already has a tracking entry for today
            var today = DateTime.Today;
            var existingTracking = await _context.Dailytrackings
                .FirstOrDefaultAsync(d => d.StudentId == currentUserId &&
                                        d.CreatedAt.Date == today);

            if (existingTracking != null)
            {
                ModelState.AddModelError(string.Empty, "You have already submitted a daily tracking report for today. You can edit your existing report.");

                var studentTasks = await _context.Tasks
                    .Include(t => t.Status)
                    .Where(t => (t.StudentId == currentUserId ||
                               t.GroupId == _context.Users
                                              .Where(u => u.Id == currentUserId)
                                              .Select(u => u.GroupId)
                                              .FirstOrDefault()) &&
                               t.Status.Name != "Completed")
                    .ToListAsync();

                ViewBag.TaskId = new SelectList(studentTasks, "Id", "Title");

                return View(dailyTracking);
            }

            if (ModelState.IsValid)
            {
                dailyTracking.StudentId = currentUserId;
                dailyTracking.CreatedAt = DateTime.Now;

                _context.Add(dailyTracking);
                await _context.SaveChangesAsync();

                // Create notification for mentor
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == dailyTracking.TaskId);

                if (task != null)
                {
                    var notification = new Notification
                    {
                        UserId = task.MentorId,
                        Content = $"New daily tracking report submitted by {User.Identity.Name}",
                        StatusId = 1 // Assuming 1 is the status ID for "New"
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var tasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => (t.StudentId == currentUserId ||
                           t.GroupId == _context.Users
                                          .Where(u => u.Id == currentUserId)
                                          .Select(u => u.GroupId)
                                          .FirstOrDefault()) &&
                           t.Status.Name != "Completed")
                .ToListAsync();

            ViewBag.TaskId = new SelectList(tasks, "Id", "Title");

            return View(dailyTracking);
        }

        // GET: DailyTracking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = 1;

            var dailyTracking = await _context.Dailytrackings
                .Include(d => d.Task)
                .FirstOrDefaultAsync(m => m.Id == id && m.StudentId == currentUserId);

            if (dailyTracking == null)
            {
                return NotFound();
            }

            // Get student's assigned tasks for dropdown
            var studentTasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => (t.StudentId == currentUserId ||
                           t.GroupId == _context.Users
                                          .Where(u => u.Id == currentUserId)
                                          .Select(u => u.GroupId)
                                          .FirstOrDefault()) &&
                           t.Status.Name != "Completed")
                .ToListAsync();

            ViewBag.TaskId = new SelectList(studentTasks, "Id", "Title", dailyTracking.TaskId);

            return View(dailyTracking);
        }

        // POST: DailyTracking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TaskId,Report")] Dailytracking dailyTracking)
        {
            var currentUserId = 1;

            var existingTracking = await _context.Dailytrackings
                .FirstOrDefaultAsync(d => d.Id == id && d.StudentId == currentUserId);

            if (existingTracking == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only the fields that can be changed
                    existingTracking.TaskId = dailyTracking.TaskId;
                    existingTracking.Report = dailyTracking.Report;

                    _context.Update(existingTracking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DailyTrackingExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Create notification for mentor about the update
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == existingTracking.TaskId);

                if (task != null)
                {
                    var notification = new Notification
                    {
                        UserId = task.MentorId,
                        Content = $"Daily tracking report updated by {User.Identity.Name}",
                        StatusId = 1 // Assuming 1 is the status ID for "New"
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var tasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => (t.StudentId == currentUserId ||
                           t.GroupId == _context.Users
                                          .Where(u => u.Id == currentUserId)
                                          .Select(u => u.GroupId)
                                          .FirstOrDefault()) &&
                           t.Status.Name != "Completed")
                .ToListAsync();

            ViewBag.TaskId = new SelectList(tasks, "Id", "Title", dailyTracking.TaskId);

            return View(dailyTracking);
        }

        private bool DailyTrackingExists(int id)
        {
            return _context.Dailytrackings.Any(e => e.Id == id);
        }
    }
}