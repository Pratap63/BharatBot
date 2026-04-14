namespace ChatBot.Models;

public class ChatKnowledge
{
    public int Id { get; set; }

    public required string Question { get; set; }
    public required string Answer { get; set; }
    public required string Keywords { get; set; }
}
