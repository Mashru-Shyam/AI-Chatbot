using AI_Chatbot.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using AI_Chatbot.DTOs;

namespace AI_Chatbot.Services
{
    public class EntityExtractionService : IEntityExtractionService
    {
        public Dictionary<string, List<string>> ExtractEntities(ChatRequestDto chatRequest)
        {
            string emailPattern = @"[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+"; // Email regex
            string otpPattern = @"\b\d{6}\b";
            string datePattern = @"\b(?:\d{1,2}(?:st|nd|rd|th)?(?:\s)?(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|January|February|March|April|May|June|July|August|September|October|November|December)\s?\d{0,4})|\b(?:\d{4}-\d{2}-\d{2})\b"; // Date regex
            string timePattern = @"\b(?:[01]?\d|2[0-3]):[0-5]\d(?:\s?(?:AM|PM|am|pm))?\b|\b(?:[01]?\d|2[0-3])(?:\s?(?:AM|PM|am|pm))\b"; // Time regex

            var entities = new Dictionary<string, List<string>>
            {
                { "email", new List<string>() },
                { "otp", new List<string>() },
                { "date", new List<string>() },
                { "time", new List<string>() }
            };

            entities["email"].AddRange(Regex.Matches(chatRequest.Message, emailPattern, RegexOptions.IgnoreCase).Select(m => m.Value));
            entities["otp"].AddRange(Regex.Matches(chatRequest.Message, otpPattern).Select(m => m.Value));
            entities["date"].AddRange(Regex.Matches(chatRequest.Message, datePattern, RegexOptions.IgnoreCase).Select(m => m.Value));
            entities["time"].AddRange(Regex.Matches(chatRequest.Message, timePattern, RegexOptions.IgnoreCase).Select(m => m.Value));

            return entities;
        }
    }
}
