using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternManagement.Models;
using InternManagement.Models.ViewModels;

namespace InternManagement.Controllers
{
    public class InternGroupController : Controller
    {
        private readonly InternmanagementContext _context;

        public InternGroupController(InternmanagementContext context)
        {
            _context = context;
        }

        // GET: InternGroup
        public async Task<IActionResult> Index(string searchString, string field)
        {
            var groupsQuery = _context.Interngroups
                .Include(g => g.Mentor).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                groupsQuery = groupsQuery.Where(g =>
                    g.Name.Contains(searchString) ||
                    g.Field.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(field))
            {
                groupsQuery = groupsQuery.Where(g => g.Field == field);
            }

            // Get all available fields for filter dropdown
            var fields = await _context.Interngroups
                .Select(g => g.Field)
                .Where(f => f != null)
                .Distinct()
                .ToListAsync();

            ViewBag.Fields = new SelectList(fields);

            var groups = await groupsQuery
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            // Get student counts for each group
            var groupStudentCounts = new Dictionary<int, int>();
            foreach (var group in groups)
            {
                var studentCount = await _context.Users
                    .CountAsync(u => u.GroupId == group.Id);
                groupStudentCounts[group.Id] = studentCount;
            }

            ViewBag.GroupStudentCounts = groupStudentCounts;

            return View(groups);
        }

        // GET: InternGroup/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internGroup = await _context.Interngroups
                .Include(g => g.Mentor)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (internGroup == null)
            {
                return NotFound();
            }

            // Get all students in this group
            var studentsInGroup = await _context.Users
                .Where(u => u.GroupId == id)
                .ToListAsync();

            // Get all tasks assigned to this group
            var groupTasks = await _context.Tasks
                .Include(t => t.Status)
                .Where(t => t.GroupId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var viewModel = new InternGroupViewModel
            {
                Group = internGroup,
                Students = studentsInGroup,
                Tasks = groupTasks
            };

            return View(viewModel);
        }

        // GET: InternGroup/Create
        public async Task<IActionResult> Create()
        {
            var mentorRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Mentor"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Get all mentors for dropdown
            var mentors = await _context.Users
                .Where(u => u.RoleId == mentorRoleId)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Mentors = new SelectList(mentors, "Id", "FullName");

            return View();
        }

        // POST: InternGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Field,IsActive,MentorId")] Interngroup internGroup)
        {
            if (ModelState.IsValid)
            {
                internGroup.CreatedAt = DateTime.Now;
                _context.Add(internGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var mentorRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Mentor"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Get all mentors for dropdown
            var mentors = await _context.Users
                .Where(u => u.RoleId == mentorRoleId)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Mentors = new SelectList(mentors, "Id", "FullName", internGroup.MentorId);

            return View(internGroup);
        }

        // GET: InternGroup/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internGroup = await _context.Interngroups.FindAsync(id);

            if (internGroup == null)
            {
                return NotFound();
            }

            var mentorRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Mentor"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Get all mentors for dropdown
            var mentors = await _context.Users
                .Where(u => u.RoleId == mentorRoleId)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Mentors = new SelectList(mentors, "Id", "FullName", internGroup.MentorId);

            return View(internGroup);
        }

        // POST: InternGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Field,IsActive,MentorId")] Interngroup internGroup)
        {
            if (id != internGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGroup = await _context.Interngroups.FindAsync(id);
                    existingGroup.Name = internGroup.Name;
                    existingGroup.Field = internGroup.Field;
                    existingGroup.IsActive = internGroup.IsActive;
                    existingGroup.MentorId = internGroup.MentorId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InternGroupExists(internGroup.Id))
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

            var mentorRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Mentor"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Get all mentors for dropdown
            var mentors = await _context.Users
                .Where(u => u.RoleId == mentorRoleId)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Mentors = new SelectList(mentors, "Id", "FullName", internGroup.MentorId);

            return View(internGroup);
        }

        // GET: InternGroup/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internGroup = await _context.Interngroups
                .Include(g => g.Mentor)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (internGroup == null)
            {
                return NotFound();
            }

            // Check if there are any students in this group
            var studentsInGroup = await _context.Users
                .Where(u => u.GroupId == id)
                .ToListAsync();

            ViewBag.HasStudents = studentsInGroup.Any();
            ViewBag.StudentCount = studentsInGroup.Count;

            return View(internGroup);
        }

        // POST: InternGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if there are any students in this group
            var hasStudents = await _context.Users
                .AnyAsync(u => u.GroupId == id);

            if (hasStudents)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete group with students. Please reassign or remove students first.");
                var internGroup = await _context.Interngroups
                    .Include(g => g.Mentor)
                    .FirstOrDefaultAsync(g => g.Id == id);
                ViewBag.HasStudents = true;
                var studentCount = await _context.Users
                    .CountAsync(u => u.GroupId == id);
                ViewBag.StudentCount = studentCount;
                return View(internGroup);
            }

            var group = await _context.Interngroups.FindAsync(id);
            _context.Interngroups.Remove(group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: InternGroup/ManageStudents/5
        public async Task<IActionResult> ManageStudents(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var internGroup = await _context.Interngroups
                .Include(g => g.Mentor)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (internGroup == null)
            {
                return NotFound();
            }

            var studentRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Intern"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Get current students in this group
            var studentsInGroup = await _context.Users
                .Where(u => u.GroupId == id && u.RoleId == studentRoleId)
                .ToListAsync();

            // Get available students (not assigned to any group or in other groups)
            var availableStudents = await _context.Users
                .Where(u => (u.GroupId == null || u.GroupId != id) && u.RoleId == studentRoleId)
                .ToListAsync();

            var viewModel = new GroupStudentManagementViewModel
            {
                GroupId = internGroup.Id,
                GroupName = internGroup.Name,
                MentorName = internGroup.Mentor?.FullName,
                StudentsInGroup = studentsInGroup,
                AvailableStudents = availableStudents
            };

            return View(viewModel);
        }

        // POST: InternGroup/ManageStudents/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageStudents(int id, int[] selectedStudents)
        {
            var internGroup = await _context.Interngroups.FindAsync(id);

            if (internGroup == null)
            {
                return NotFound();
            }

            var studentRoleId = await _context.Roles
                .Where(r => r.Name.Equals("Mentor"))
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            // Remove all students from this group
            var currentStudents = await _context.Users
                .Where(u => u.GroupId == id && u.RoleId == studentRoleId)
                .ToListAsync();

            foreach (var student in currentStudents)
            {
                student.GroupId = null;
            }

            // Assign selected students to this group
            if (selectedStudents != null && selectedStudents.Length > 0)
            {
                var students = await _context.Users
                    .Where(u => selectedStudents.Contains(u.Id) && u.RoleId == studentRoleId)
                    .ToListAsync();

                foreach (var student in students)
                {
                    student.GroupId = id;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool InternGroupExists(int id)
        {
            return _context.Interngroups.Any(e => e.Id == id);
        }
    }
}