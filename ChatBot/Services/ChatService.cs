using ChatBot.Repository;

namespace ChatBot.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatKnowledgeRepository _repo;
        private readonly IMessageRepository _messageRepo;

        public ChatService(IChatKnowledgeRepository repo, IMessageRepository messageRepo)
        {
            _repo = repo;
            _messageRepo = messageRepo;
        }

        public async Task<string?> GetAnswerAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var match = await _repo.FindBestMatchAsync(question);
            var answer = match?.Answer;

            // ✅ Fine-tune / format the answer before returning
            return answer != null ? FormatAnswer(answer) : null;
        }

        // Private helper method inside the service
        private string FormatAnswer(string answer)
        {
            // Ensure first sentence is a summary
            var sentences = answer.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length > 2)
            {
                return string.Join(". ", sentences.Take(2)) + ".";
            }
            return answer;
        }
    }
}
