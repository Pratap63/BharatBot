using ChatBot.Models;

namespace ChatBot.Helpers
{
    public static class SimilarityEngine
    {
        public static double CalculateScore(string query, ChatKnowledge knowledge)
        {
            var queryTokens = TextHelper.Tokenize(query);
            var keywordTokens = TextHelper.Tokenize(knowledge.Keywords);

            int matches = queryTokens.Count(q => keywordTokens.Contains(q));

            if (keywordTokens.Count == 0) return 0;

            return (double)matches / keywordTokens.Count;
        }
    }
}
