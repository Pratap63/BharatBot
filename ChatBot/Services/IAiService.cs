namespace ChatBot.Services
{
    public interface IAiService
    {
        Task<string> GenerateAnswerAsync(string question);
    }
}
