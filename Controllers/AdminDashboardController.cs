using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternManagement.Models;
using InternManagement.Models.ViewModels;
using InternManagement.Models;

namespace InternManagementSystem.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly InternmanagementContext _context;

        public AdminDashboardController(InternmanagementContext context)
        {
            _context = context;
        }

        // GET: AdminDashboard
        public async Task<IActionResult> Index()
        {
            // Get role IDs
            var studentRoleId = await _context.Roles.Where(r => r.Name.Equals("Intern")).Select(r => r.Id).FirstOrDefaultAsync();
            var mentorRoleId = await _context.Roles.Where(r => r.Name.Equals("Mentor")).Select(r => r.Id).FirstOrDefaultAsync();

            // Get summary statistics
            var totalStudents = await _context.Users.CountAsync(u => u.RoleId == studentRoleId);
            var totalMentors = await _context.Users.CountAsync(u => u.RoleId == mentorRoleId);
            var totalGroups = await _context.Interngroups.CountAsync(g => g.IsActive);

            // Get tasks by status
            var tasksByStatus = await _context.Tasks
                .Include(t => t.Status)
                .GroupBy(t => t.Status.Name)
                .Select(g => new { StatusName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusName, x => x.Count);

            // Get students with late submissions
            var currentDate = DateTime.Now;
            var studentsWithLateSubmissions = await _context.Tasks
                .Include(t => t.Student)
                .Include(t => t.Tasksubmits)
                .Where(t => t.Deadline < currentDate &&
                       (t.Tasksubmits.Count == 0 ||
                        t.Tasksubmits.OrderByDescending(s => s.SubmittedAt).First().SubmittedAt > t.Deadline))
                .Select(t => new StudentWithLateSubmission
                {
                    Student = t.Student,
                    Task = t,
                    DaysLate = EF.Functions.DateDiffDay(t.Deadline.Date, currentDate)
                })
                .Take(10)
                .ToListAsync();


            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalMentors = totalMentors,
                TotalGroups = totalGroups,
                TasksByStatus = tasksByStatus,
                StudentsWithLateSubmissions = studentsWithLateSubmissions,
            };

            return View(viewModel);
        }
    }
}