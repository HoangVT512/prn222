using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternManagement.Models;
using InternManagement.Models.ViewModels;

namespace InternManagement.Controllers
{
    public class MentorManagementController : Controller
    {
        private readonly InternmanagementContext _context;

        public MentorManagementController(InternmanagementContext context)
        {
            _context = context;
        }

        // GET: MentorManagement
        public async Task<IActionResult> Index(string searchString)
        {
            var mentorRoleId = await _context.Roles
                .Where(r => r.Name == "MENTOR")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var mentorsQuery = _context.Users
                .Where(u => u.RoleId == mentorRoleId);

            if (!string.IsNullOrEmpty(searchString))
            {
                mentorsQuery = mentorsQuery.Where(m =>
                    m.FullName.Contains(searchString) ||
                    m.Email.Contains(searchString) ||
                    m.Username.Contains(searchString));
            }

            var mentors = await mentorsQuery
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(mentors);
        }

        // GET: MentorManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mentor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (mentor == null)
            {
                return NotFound();
            }

            // Get groups managed by this mentor
            var mentorGroups = await _context.Interngroups
                .Where(g => g.MentorId == id)
                .ToListAsync();

            // Get all students under this mentor's groups
            var mentorStudents = new List<User>();
            foreach (var group in mentorGroups)
            {
                var studentsInGroup = await _context.Users
                    .Where(u => u.GroupId == group.Id)
                    .ToListAsync();

                mentorStudents.AddRange(studentsInGroup);
            }

            var viewModel = new MentorViewModel
            {
                Mentor = mentor,
                Groups = mentorGroups,
                Students = mentorStudents
            };

            return View(viewModel);
        }

        // GET: MentorManagement/Create
        public async Task<IActionResult> Create()
        {
            var mentorRoleId = await _context.Roles
                .Where(r => r.Name == "MENTOR")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            ViewBag.MentorRoleId = mentorRoleId;

            return View();
        }

        // POST: MentorManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,FullName,Email,Phone,RoleId")] User mentor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mentor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var mentorRoleId = await _context.Roles
                .Where(r => r.Name == "MENTOR")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            ViewBag.MentorRoleId = mentorRoleId;

            return View(mentor);
        }

        // GET: MentorManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mentor = await _context.Users.FindAsync(id);

            if (mentor == null)
            {
                return NotFound();
            }

            return View(mentor);
        }

        // POST: MentorManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,FullName,Email,Phone")] User mentor)
        {
            if (id != mentor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMentor = await _context.Users.FindAsync(id);
                    existingMentor.Username = mentor.Username;
                    existingMentor.FullName = mentor.FullName;
                    existingMentor.Email = mentor.Email;
                    existingMentor.Phone = mentor.Phone;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(mentor.Id))
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

            return View(mentor);
        }

        // GET: MentorManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mentor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (mentor == null)
            {
                return NotFound();
            }

            return View(mentor);
        }

        // POST: MentorManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mentor = await _context.Users.FindAsync(id);

            // Check if mentor has any groups assigned
            var hasAssignedGroups = await _context.Interngroups
                .AnyAsync(g => g.MentorId == id);

            if (hasAssignedGroups)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete mentor with assigned groups. Please reassign the groups first.");
                return View(mentor);
            }

            _context.Users.Remove(mentor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: MentorManagement/AssignGroups/5
        public async Task<IActionResult> AssignGroups(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mentor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (mentor == null)
            {
                return NotFound();
            }

            var assignedGroups = await _context.Interngroups
                .Where(g => g.MentorId == id)
                .ToListAsync();

            var availableGroups = await _context.Interngroups
                .Where(g => g.MentorId == null || g.MentorId != id)
                .ToListAsync();

            var viewModel = new MentorGroupAssignmentViewModel
            {
                MentorId = mentor.Id,
                MentorName = mentor.FullName,
                AssignedGroups = assignedGroups,
                AvailableGroups = availableGroups
            };

            return View(viewModel);
        }

        // POST: MentorManagement/AssignGroups/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGroups(int id, int[] selectedGroups)
        {
            var mentor = await _context.Users.FindAsync(id);

            if (mentor == null)
            {
                return NotFound();
            }

            // Remove mentor from all previously assigned groups
            var currentAssignedGroups = await _context.Interngroups
                .Where(g => g.MentorId == id)
                .ToListAsync();

            foreach (var group in currentAssignedGroups)
            {
                group.MentorId = null;
            }

            // Assign mentor to selected groups
            if (selectedGroups != null)
            {
                foreach (var groupId in selectedGroups)
                {
                    var group = await _context.Interngroups.FindAsync(groupId);
                    if (group != null)
                    {
                        group.MentorId = id;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}