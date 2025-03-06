using HomeHutBD.Data;
using HomeHutBD.Helpers;
using HomeHutBD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HomeHutBD.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ApplicationDbContext context, ILogger<ChatController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Get unique chat conversations grouped by property
            var uniqueChats = await _context.Chats
                .Where(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                .GroupBy(c => new { c.PropertyId, OtherUserId = c.SenderId == currentUserId ? c.ReceiverId : c.SenderId })
                .Select(g => new
                {
                    PropertyId = g.Key.PropertyId,
                    OtherUserId = g.Key.OtherUserId,
                    LastMessageTime = g.Max(c => c.Timestamp)
                })
                .OrderByDescending(x => x.LastMessageTime)
                .ToListAsync();

            var chatViewModels = new List<ChatViewModel>();

            foreach (var chat in uniqueChats)
            {
                var property = await _context.Properties.FindAsync(chat.PropertyId);
                var otherUser = await _context.Users.FindAsync(chat.OtherUserId);
                var lastMessage = await _context.Chats
                    .Where(c => c.PropertyId == chat.PropertyId &&
                           ((c.SenderId == currentUserId && c.ReceiverId == chat.OtherUserId) ||
                            (c.ReceiverId == currentUserId && c.SenderId == chat.OtherUserId)))
                    .OrderByDescending(c => c.Timestamp)
                    .FirstOrDefaultAsync();

                if (property != null && otherUser != null && lastMessage != null)
                {
                    chatViewModels.Add(new ChatViewModel
                    {
                        PropertyId = property.PropertyId,
                        PropertyTitle = property.Title,
                        OtherUserId = otherUser.UserId,
                        OtherUserName = otherUser.Username,
                        LastMessage = lastMessage.Message,
                        LastMessageTime = lastMessage.Timestamp,
                        IsLastMessageFromCurrentUser = lastMessage.SenderId == currentUserId
                    });
                }
            }

            return View(chatViewModels);
        }

        [Authorize]
        public async Task<IActionResult> Conversation(int propertyId, int userId)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            var property = await _context.Properties
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            var otherUser = await _context.Users.FindAsync(userId);

            if (property == null || otherUser == null)
            {
                return NotFound();
            }

            var messages = await _context.Chats
                .Where(c => c.PropertyId == propertyId &&
                           ((c.SenderId == currentUserId && c.ReceiverId == userId) ||
                            (c.ReceiverId == currentUserId && c.SenderId == userId)))
                .OrderBy(c => c.Timestamp)
                .ToListAsync();

            var viewModel = new ConversationViewModel
            {
                PropertyId = property.PropertyId,
                PropertyTitle = property.Title,
                PropertyImageUrl = property.ImageUrl,
                OtherUserId = otherUser.UserId,
                OtherUserName = otherUser.Username,
                Messages = messages,
                CurrentUserId = currentUserId
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int propertyId, int receiverId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                // Redirect back to the relevant page
                if (Request.Headers["Referer"].ToString().Contains("Properties/Details"))
                {
                    return RedirectToAction("Details", "Properties", new { id = propertyId });
                }
                return RedirectToAction("Conversation", new { propertyId, userId = receiverId });
            }

            int currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Don't allow chatting with yourself
            if (currentUserId == receiverId)
            {
                return RedirectToAction("Details", "Properties", new { id = propertyId });
            }

            var newMessage = new Chats
            {
                SenderId = currentUserId,
                ReceiverId = receiverId,
                PropertyId = propertyId,
                Message = message,
                Timestamp = DateTime.Now
            };

            _context.Chats.Add(newMessage);
            await _context.SaveChangesAsync();

            // Always redirect to the conversation page
            return RedirectToAction("Conversation", new { propertyId, userId = receiverId });
        }
    }

    public class ChatViewModel
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public bool IsLastMessageFromCurrentUser { get; set; }
    }

    public class ConversationViewModel
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string PropertyImageUrl { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public List<Chats> Messages { get; set; }
        public int CurrentUserId { get; set; }
    }
}