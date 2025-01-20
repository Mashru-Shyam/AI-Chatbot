using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class ConversationService : IConversationService
    {
        private readonly AiChatbotDbContext context;

        public ConversationService(AiChatbotDbContext context)
        {
        }
        public async Task AddConversationAsync(int userId, string intent, ICollection<Entity> entities, bool IsCompleted, string status)
        {
            var conversation = new Conversation
            {
                UserId = userId,
                Intent = intent,
                Entities = entities,
                IsCompleted = IsCompleted,
                Context = status
            };

            await context.Conversations.AddAsync(conversation);
            await context.SaveChangesAsync();
        }

        public async Task DeleteConversationAsync(int userId)
        {
            var conversation = await context.Conversations.FirstOrDefaultAsync(c => c.UserId == userId);
            context.Conversations.Remove(conversation);
            await context.SaveChangesAsync();
        }

        public async Task<Conversation> GetConversationAsync(int userId)
        {
            var conversation = await context.Conversations
                .Include(c => c.Entities)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return conversation;
        }

        public async Task UpdateConversationAsync(int userId, string intent, ICollection<Entity> entities, bool IsCompleted, string status)
        {
            var conversation = context.Conversations.FirstOrDefault(c => c.UserId == userId);
            conversation.Intent = intent;
            conversation.Entities = entities;
            conversation.IsCompleted = IsCompleted;
            conversation.Context = status;

            context.Conversations.Update(conversation);
            await context.SaveChangesAsync();

        }
    }
}
