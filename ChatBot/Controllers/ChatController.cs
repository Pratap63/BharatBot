using ChatBot.Data;
using ChatBot.Helpers;
using ChatBot.Services;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Controllers
{
    
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IAiService _aiService;
        private readonly AppDbContext _db;
        private readonly DbConnector DbContent;

        public ChatController(
            IChatService chatService,
            IAiService aiService,
            AppDbContext db,
            DbConnector db2)
        {
            _chatService = chatService;
            _aiService = aiService;
            _db = db;
            DbContent = db2;
        }

        [Authorize]

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult About()
        {
            return View();
        }

        // =======================
        // Sidebar: Get all chats
        // =======================

        public async Task<IActionResult> GetChats()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            int userId = int.Parse(userIdClaim); // Convert if your DB UserId is int

            var chats = await _db.Chats
                .Where(c => c.UserId == userId)   // 🔥 filter by logged-in user
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.ChatId,
                    c.Title
                })
                .ToListAsync();

            return Json(chats);
        }


        // =======================
        // Get messages of a chat
        // =======================


        public async Task<IActionResult> GetMessages(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim); // Convert if your DB UserId is int

            var messages = await _db.Message
                .Where(m => m.ChatId == id && m.Chat.UserId == userId)
                .OrderBy(m => m.ChatId)
                .Select(m => new
                {
                    m.Role,
                    m.Content
                })
                .ToListAsync();

            return Json(messages);
        }


        // =======================
        // Create new chat
        // =======================
        [HttpPost]

        
        public async Task<IActionResult> CreateChat()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            int userId = int.Parse(userIdClaim); // Convert if your DB UserId is int

            var chat = new Chat
            {
                Title = "New Chat",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Chats.AddAsync(chat);
            await _db.SaveChangesAsync();

            return Json(new { chat.ChatId });
        }


        // =======================
        // Save message
        // =======================
        [HttpPost]
        public IActionResult SaveMessage(int chatId, string role, string content)
        {
            var message = new Message
            {
                ChatId = chatId,
                Role = role,
                Content = content
            };

            _db.Message.Add(message);

            if (role == "user")
            {
                var chat = _db.Chats.Find(chatId);
                if (chat != null && chat.Title == "New Chat")
                {
                    chat.Title = content.Length > 20
                        ? content.Substring(0, 20)
                        : content;
                }
            }

            _db.SaveChanges();
            return Ok();
        }

        // =======================
        // Rename chat
        // =======================
        [HttpPost]
        public async Task<IActionResult> RenameChat(int chatId, string title)
        {
            var chat = await _db.Chats.FindAsync(chatId);
            if (chat == null) return NotFound();

            chat.Title = title;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // =======================
        // Delete chat
        // =======================
        [HttpPost]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var chat = await _db.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null) return NotFound();

            _db.Message.RemoveRange(chat.Messages);
            _db.Chats.Remove(chat);
            await _db.SaveChangesAsync();

            return Ok();
        }

        // =======================
        // MAIN CHAT ENDPOINT
        // =======================
        [HttpGet]
        public async Task<IActionResult> AskGet(string question)
        {
            // 0️⃣ Validation
            if (string.IsNullOrWhiteSpace(question))
            {
                return Ok(new
                {
                    response = "<p>Please enter a valid question.</p>",
                    source = "system",
                    format = "html"
                });
            }

            var normalized = question.Trim().ToLowerInvariant();
            Console.WriteLine($"User Question: {normalized}");

            // 1️⃣ Greetings
            if (normalized is "hi" or "hello" or "hey" or "hy")
            {
                return Ok(new
                {
                    response = "<p>Hello! 👋 How can I help you today?</p>",
                    source = "system",
                    format = "html"
                });
            }

            // 2️⃣ Restricted personal questions
            if (normalized is "what is my name" or "who am i")
            {
                return Ok(new
                {
                    response = "<p>You haven’t shared your name with me yet 🙂</p>",
                    source = "system",
                    format = "html"
                });
            }

            // 3️⃣ Knowledge Base lookup
            var dbAnswer = await _chatService.GetAnswerAsync(normalized);
            Console.WriteLine($"DB Answer: {dbAnswer ?? "NULL"}");

            if (!string.IsNullOrWhiteSpace(dbAnswer))
            {
                var html = Markdown.ToHtml(dbAnswer);

                return Ok(new
                {
                    response = html,
                    source = "database",
                    format = "html"
                });
            }

            // 4️⃣ AI fallback
            var aiResponse = await _aiService.GenerateAnswerAsync(question);
            var aiHtml = Markdown.ToHtml(aiResponse);

            return Ok(new
            {
                response = aiHtml,
                source = "ai",
                format = "html"
            });
        }
    }
}
