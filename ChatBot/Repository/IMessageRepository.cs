public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<List<Message>> GetByChatIdAsync(int chatId);
}
