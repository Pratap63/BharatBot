using System;

namespace ChatbotWebApp.Services
{
    public class ChatbotService : IChatbotService
    {
        public string GetResponse(string userMessage)
        {
            userMessage = userMessage.ToLower();

            if (userMessage.Contains("hello") || userMessage.Contains("hi"))
                return "Hello! How can I help you today? 😊";

            if (userMessage.Contains("time"))
                return $"Current time is {DateTime.Now.ToShortTimeString()}";

            if (userMessage.Contains("bye"))
                return "Goodbye! Have a nice day 👋";

            return "Sorry, I didn’t understand that. Can you rephrase?";
        }
    }
}
