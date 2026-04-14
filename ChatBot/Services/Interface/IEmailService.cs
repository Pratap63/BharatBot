namespace ChatBot.Services.Interface
{
    public interface IEmailService
    {
       public Task SendEmailAsync(string toEmail, string subject, string body);
    }
}