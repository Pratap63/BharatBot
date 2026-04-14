public class Message
{
    public int MessageId { get; set; }
    public int ChatId { get; set; }

    public string Role { get; set; }   // "user" or "bot"
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Chat? Chat { get; set; }
}
