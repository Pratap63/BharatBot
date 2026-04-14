using ChatBot.Models;

namespace ChatBot.Repository;

public interface IChatKnowledgeRepository
{
    Task<List<ChatKnowledge>> GetAllAsync();
    Task<ChatKnowledge?> FindBestMatchAsync(string question);
}
