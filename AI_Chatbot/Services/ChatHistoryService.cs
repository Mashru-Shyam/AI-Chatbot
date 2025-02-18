using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly AiChatbotDbContext context;

        public ChatHistoryService(AiChatbotDbContext context)
        {
            this.context = context;
        }
        public async Task AddChatHistory(int sessionId, string userMessage, string botResponse)
        {
            var chatHistory = new ChatHistory
            {
                SessionId = sessionId,
                UserMessage = userMessage,
                BotMessage = botResponse,
                Timestamp = DateTime.Now
            };

            await context.ChatHistory.AddAsync(chatHistory);
            await context.SaveChangesAsync();

        }

        public async Task<IEnumerable<ChatHistoryDto>> GetChatHistory(int sessionId)
        {
            var chatHistory = await context.ChatHistory
               .Where(a => a.SessionId == sessionId).OrderByDescending(a => a.Timestamp)
               .ToListAsync();

            var result = chatHistory.OrderByDescending(a => a.Timestamp).Take(5).Select(a => new ChatHistoryDto
            {
                UserMessage = a.UserMessage,
                BotMessage = a.BotMessage
            }).ToList();

            return result;

        }
    }
}
