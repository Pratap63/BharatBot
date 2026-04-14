public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 🔐 Forgot password
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
}
