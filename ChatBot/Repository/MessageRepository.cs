using ChatBot.Data;
using Microsoft.EntityFrameworkCore;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Message message)
    {
        _context.Message.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Message>> GetByChatIdAsync(int chatId)
    {
        return await _context.Message
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}
