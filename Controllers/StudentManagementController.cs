
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternManagement.Models;
using InternManagement.Models.ViewModels;

namespace InternManagement.Controllers
{
    public class StudentManagementController : Controller
    {
        private readonly InternmanagementContext _context;

        public StudentManagementController(InternmanagementContext context)
        {
            _context = context;
        }

        // GET: StudentManagement
        public async Task<IActionResult> Index(string searchString, string status, int? groupId)
        {
            var studentRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Intern"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var studentsQuery = _context.Users
                .Include(u => u.Group)
                .Where(u => u.RoleId == studentRoleId);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.FullName.Contains(searchString) ||
                    s.Email.Contains(searchString) ||
                    s.Username.Contains(searchString));
            }

            // Apply status filter
/*            if (!string.IsNullOrEmpty(status))
            {
                studentsQuery = studentsQuery.Where(s => s.InternStatus == status);
            }*/

            // Apply group filter
            if (groupId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.GroupId == groupId.Value);
            }

            var students = await studentsQuery
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Get all groups for filter dropdown
            ViewBag.Groups = new SelectList(await _context.Interngroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync(), "Id", "Name");

            // Get status options
            ViewBag.StatusOptions = new List<string> { "ACTIVE", "COMPLETED", "DROPPED", "ON_HOLD" };

            return View(students);
        }

        // GET: StudentManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Get task progress for student
            var taskProgress = await _context.Tasks
                .Include(t => t.Tasksubmits)
                .Where(t => t.StudentId == id || t.GroupId == student.GroupId)
                .Select(t => new TaskProgressSummary
                {
                    Task = t,
                    LastSubmission = t.Tasksubmits
                        .Where(s => s.StudentId == id)
                        .OrderByDescending(s => s.SubmittedAt)
                        .FirstOrDefault(),
                    Progress = t.Tasksubmits
                        .Where(s => s.StudentId == id)
                        .OrderByDescending(s => s.SubmittedAt)
                        .Select(s => s.ProgressEvaluate ?? 0)
                        .FirstOrDefault(),
                    IsOverdue = t.Deadline < System.DateTime.Now
                })
                .ToListAsync();

            var viewModel = new StudentViewModel
            {
                Student = student,
                Group = student.Group,
                TaskProgress = taskProgress
            };

            return View(viewModel);
        }

        // GET: StudentManagement/Create
        public async Task<IActionResult> Create()
        {
            var studentRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Intern"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            ViewBag.Groups = new SelectList(await _context.Interngroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync(), "Id", "Name");

            ViewBag.StudentRoleId = studentRoleId;

            return View();
        }

        // POST: StudentManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,FullName,Email,Phone,RoleId,GroupId,InternStatus")] User student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var studentRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Intern"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            ViewBag.Groups = new SelectList(await _context.Interngroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync(), "Id", "Name", student.GroupId);

            ViewBag.StudentRoleId = studentRoleId;

            return View(student);
        }

        // GET: StudentManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Users.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            ViewBag.Groups = new SelectList(await _context.Interngroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync(), "Id", "Name", student.GroupId);

            ViewBag.StatusOptions = new List<string> { "ACTIVE", "COMPLETED", "DROPPED", "ON_HOLD" };

            return View(student);
        }

        // POST: StudentManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,FullName,Email,Phone,GroupId,InternStatus")] User student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Users.FindAsync(id);
                    existingStudent.Username = student.Username;
                    existingStudent.FullName = student.FullName;
                    existingStudent.Email = student.Email;
                    existingStudent.Phone = student.Phone;
                    existingStudent.GroupId = student.GroupId;
                    //existingStudent.InternStatus = student.InternStatus;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(student.Id))
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

            ViewBag.Groups = new SelectList(await _context.Interngroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync(), "Id", "Name", student.GroupId);

            ViewBag.StatusOptions = new List<string> { "ACTIVE", "COMPLETED", "DROPPED", "ON_HOLD" };

            return View(student);
        }

        // GET: StudentManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: StudentManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Users.FindAsync(id);
            _context.Users.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}