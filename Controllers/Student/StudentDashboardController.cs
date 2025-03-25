using System;
using System.Linq;
using System.Security.Claims;
using InternManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Controllers
{
    //[Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly InternmanagementContext _context;

        public StudentDashboardController(InternmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = 1;

            // Get student information including group
            var student = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (student == null)
            {
                return NotFound();
            }

            // Get pending tasks
            var pendingTasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => (t.StudentId == currentUserId || t.GroupId == student.GroupId) &&
                       t.Deadline >= DateTime.Now)
                .OrderBy(t => t.Deadline)
                .Take(5)
                .ToListAsync();

            // Get recent submissions
            var recentSubmissions = await _context.Tasksubmits
                .Include(ts => ts.Task)
                .Where(ts => ts.StudentId == currentUserId)
                .OrderByDescending(ts => ts.SubmittedAt)
                .Take(5)
                .ToListAsync();

            // Get recent evaluations
            var recentEvaluations = await _context.Evaluations
                .Include(e => e.Task)
                .Include(e => e.Mentor)
                .Where(e => e.StudentId == currentUserId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get unread notifications
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == currentUserId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.PendingTasks = pendingTasks;
            ViewBag.RecentSubmissions = recentSubmissions;
            ViewBag.RecentEvaluations = recentEvaluations;
            ViewBag.UnreadNotifications = unreadNotifications;

            return View();
        }
    }
}
