namespace ChatBot.Helpers
{
    public static class TextHelper
    {
        public static List<string> Tokenize(string text)
        {
            return text
                .ToLower()
                .Split(new[] { ' ', ',', '.', '?', '!' },
                       StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
    }
}
