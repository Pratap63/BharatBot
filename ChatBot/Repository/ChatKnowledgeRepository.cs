using ChatBot.Data;
using ChatBot.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Repository;

public class ChatKnowledgeRepository : IChatKnowledgeRepository
{
    private readonly AppDbContext _context;
    private readonly AppDbContext Bot_Context;

    public ChatKnowledgeRepository(AppDbContext context)
    {
        _context = context;
        Bot_Context = context;
    }

    public async Task<List<ChatKnowledge>> GetAllAsync()
    {
        return await _context.ChatKnowledge.ToListAsync();
    }
 

    public async Task<ChatKnowledge?> FindBestMatchAsync(string question)
    {
        question = question.Trim().ToLower();

        var records = await _context.ChatKnowledge
            .AsNoTracking()
            .ToListAsync();

        return records.FirstOrDefault(k =>
            question.Contains(k.Question.ToLower()) ||
            k.Keywords
                .ToLower()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Any(keyword => question.Contains(keyword.Trim()))
        );

    }

}
