
using System.Security.Claims;
using InternManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManagement.Controllers
{
    //[Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly InternmanagementContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(InternmanagementContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = 1;

            var student = await _context.Users
                .Include(u => u.Group)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var currentUserId = 1;

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.User model, IFormFile profilePicture)
        {
            var currentUserId = 1;

            var student = await _context.Users.FindAsync(currentUserId);

            if (student == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update allowed fields
                student.FullName = model.FullName;
                student.Phone = model.Phone;

                // Handle profile picture upload
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePicture.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(fileStream);
                    }

                    // Store relative path
                    student.Pfp = "/uploads/profiles/" + uniqueFileName;
                }

                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }
    }
}