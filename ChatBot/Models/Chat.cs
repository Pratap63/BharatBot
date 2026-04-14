public class Chat
{
    public int ChatId { get; set; }

    public string Title { get; set; } = string.Empty;   // ✅ fix

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int UserId { get; set; }

    public User? User { get; set; }   // ✅ nullable navigation

    public ICollection<Message> Messages { get; set; } = new List<Message>();  // ✅ initialize
}
