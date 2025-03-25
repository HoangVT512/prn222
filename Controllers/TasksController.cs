using InternManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    public class TasksController : Controller
    {
        private readonly InternmanagementContext _context;

        public TasksController(InternmanagementContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách nhiệm vụ
        public async Task<IActionResult> Index(string search, int? statusId)
        {
            var tasks = _context.Tasks.Include(t => t.Status).Include(t => t.Student).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                tasks = tasks.Where(t => t.Title.Contains(search));
            }

            if (statusId.HasValue)
            {
                tasks = tasks.Where(t => t.StatusId == statusId);
            }

            var statuses = await _context.Taskstatuses.ToListAsync();
            ViewBag.Statuses = statuses;

            return View(await tasks.ToListAsync());
        }

        // Hiển thị form tạo nhiệm vụ
        public async Task<IActionResult> Create()
        {
            ViewBag.Students = await _context.Users.Where(u => u.Role.Name == "Intern").ToListAsync();
            ViewBag.Statuses = await _context.Taskstatuses.ToListAsync();
            return View();
        }

        // Xử lý tạo nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InternManagement.Models.Task task, IFormFile? file)
        {
            // Xử lý upload file nếu có
            if (file != null && file.Length > 0)
            {
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Lưu đường dẫn file vào DB
                task.File = "/uploads/" + uniqueFileName;
            }
            task.StatusId = 3;
            task.MentorId = 1;
            _context.Add(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Cập nhật trạng thái nhiệm vụ
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            ViewBag.Statuses = await _context.Taskstatuses.ToListAsync();
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InternManagement.Models.Task task, IFormFile? file)
        {
            if (id != task.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Xử lý upload file nếu có
                if (file != null && file.Length > 0)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Cập nhật đường dẫn file trong DB
                    task.File = "/uploads/" + uniqueFileName;
                }

                _context.Update(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }


        // Đánh giá kết quả và tiến độ
        public async Task<IActionResult> Evaluate(int id)
        {
            var task = await _context.Tasks.Include(t => t.Student).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Evaluate(int id, int rating, string comment)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            var evaluation = new Evaluation
            {
                TaskId = id,
                //MentorId = GetCurrentUserId(), // Lấy ID người đánh giá (mentor)
                StudentId = task.StudentId ?? 0, // Lấy ID sinh viên nếu có
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Evaluations.Add(evaluation);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id });
        }
        public IActionResult Download(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return NotFound();

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath)) return BadRequest();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string contentType = "application/octet-stream";
            return File(fileBytes, contentType, Path.GetFileName(fullPath));
        }
    }

}
