using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InternManagement.Models;
using System.Security.Claims;

namespace InternManagement.Controllers
{
    public class MessagingController : Controller
    {
        private readonly InternmanagementContext _context;

        public MessagingController(InternmanagementContext context)
        {
            _context = context;
        }

        // GET: Messaging
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();

            // MySQL-friendly query to get all conversations for current user with eager loading
            var userConversations = await _context.ConversationMembers
                .Where(cm => cm.UserId == currentUserId)
                .Include(cm => cm.Conversation)
                .Include(cm => cm.Conversation.ConversationMembers)
                .ThenInclude(m => m.User)
                .Include(cm => cm.Conversation.Messages)
                .ToListAsync();

            // Process results in memory to avoid complex MySQL translations
            var orderedConversations = userConversations
                .OrderByDescending(cm => cm.Conversation.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault()?.CreatedAt ?? DateTime.MinValue)
                .ToList();

            return View(orderedConversations);
        }

        // GET: Messaging/Chat/5
        public async Task<IActionResult> Chat(int id)
        {
            var currentUserId = GetCurrentUserId();

            // Check if user is a member of this conversation - simple query
            var isMember = await _context.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == id && cm.UserId == currentUserId);

            if (!isMember)
            {
                return NotFound();
            }

            // MySQL-friendly approach - separate eager loading for better performance
            var conversation = await _context.Conversations
                .Include(c => c.ConversationMembers)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversation == null)
            {
                return NotFound();
            }

            // Load messages separately and order in memory to avoid MySQL query issues
            var messages = await _context.Messages
                .Where(m => m.ConversationId == id)
                .Include(m => m.Sender)
                .ToListAsync();

            conversation.Messages = messages.OrderBy(m => m.CreatedAt).ToList();

            ViewBag.CurrentUserId = currentUserId;

            return View(conversation);
        }

        // GET: Messaging/New
        public async Task<IActionResult> New()
        {
            var currentUserId = GetCurrentUserId();

            // Simple query that works well in MySQL
            var users = await _context.Users
                .Where(u => u.Id != currentUserId && u.IsActive == true)
                .Include(u => u.Role)
                .ToListAsync();

            ViewBag.Users = users;

            return View();
        }

        // POST: Messaging/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(string name, bool isGroup, List<int> selectedUsers)
        {
            if (selectedUsers == null || !selectedUsers.Any())
            {
                ModelState.AddModelError("", "Please select at least one user");

                var users = await _context.Users
                    .Where(u => u.Id != GetCurrentUserId() && u.IsActive == true)
                    .Include(u => u.Role)
                    .ToListAsync();

                ViewBag.Users = users;

                return View();
            }

            var currentUserId = GetCurrentUserId();

            // For private chat, check if conversation already exists
            // Handle this more efficiently for MySQL
            if (!isGroup && selectedUsers.Count == 1)
            {
                var otherUserId = selectedUsers[0];

                // MySQL-friendly approach - find all private conversations first
                var userConversations = await _context.ConversationMembers
                    .Where(cm => cm.UserId == currentUserId)
                    .Select(cm => cm.ConversationId)
                    .ToListAsync();

                var otherUserConversations = await _context.ConversationMembers
                    .Where(cm => cm.UserId == otherUserId)
                    .Select(cm => cm.ConversationId)
                    .ToListAsync();

                // Find common conversations
                var commonConversationIds = userConversations.Intersect(otherUserConversations).ToList();

                if (commonConversationIds.Any())
                {
                    // Check which ones are private (not group) and have exactly 2 members
                    var privateConversations = await _context.Conversations
                        .Where(c => commonConversationIds.Contains(c.Id) && c.IsGroup == false)
                        .ToListAsync();

                    foreach (var conversation in privateConversations)
                    {
                        var memberCount = await _context.ConversationMembers
                            .CountAsync(cm => cm.ConversationId == conversation.Id);

                        if (memberCount == 2)
                        {
                            return RedirectToAction(nameof(Chat), new { id = conversation.Id });
                        }
                    }
                }
            }

            // Create new conversation
            var newConversation = new Conversation
            {
                Name = name,
                IsGroup = isGroup,
                CreatedAt = DateTime.Now
            };

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            // Add members
            var members = new List<ConversationMember>
            {
                new ConversationMember { ConversationId = newConversation.Id, UserId = currentUserId }
            };

            members.AddRange(selectedUsers.Select(userId => new ConversationMember
            {
                ConversationId = newConversation.Id,
                UserId = userId
            }));

            _context.ConversationMembers.AddRange(members);

            // Add initial message
            var message = new Message
            {
                ConversationId = newConversation.Id,
                SenderId = currentUserId,
                Content = isGroup ? "Created group chat" : "Started a conversation",
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Chat), new { id = newConversation.Id });
        }

        // POST: Messaging/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Chat), new { id = conversationId });
            }

            var currentUserId = GetCurrentUserId();

            // Simple query that works well in MySQL
            var isMember = await _context.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == currentUserId);

            if (!isMember)
            {
                return Forbid();
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var sender = await _context.Users.FindAsync(currentUserId);
                return Json(new
                {
                    id = message.Id,
                    senderId = message.SenderId,
                    senderName = sender.FullName,
                    senderPfp = sender.Pfp,
                    content = message.Content,
                    createdAt = message.CreatedAt.ToString("HH:mm")
                });
            }

            return RedirectToAction(nameof(Chat), new { id = conversationId });
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            // In a real application, get this from user claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int userId;

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                return userId;
            }

            // For development/demonstration purposes
            // In production, you should handle this case properly
            return 1; // Default to user ID 1 for testing
        }
    }
}