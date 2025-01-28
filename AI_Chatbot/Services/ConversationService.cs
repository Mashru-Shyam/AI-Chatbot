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
            this.context = context;
        }

        public async Task AddConversationAsync(int sessionId, string intent, ICollection<Entity> entities, bool IsCompleted, string status)
        {
            var conversation = new Conversation
            {
                SessionId = sessionId,
                Intent = intent,
                IsCompleted = IsCompleted,
                Context = status,
                Entities = entities
            };

            await context.Conversations.AddAsync(conversation);
            await context.SaveChangesAsync();
        }

        public async Task DeleteConversationAsync(int sessionId)
        {
            var conversation = await context.Conversations.FindAsync(sessionId);
            if (conversation == null)
            {
                return;
            }
            context.Conversations.Remove(conversation);
            await context.SaveChangesAsync();
        }

        public async Task<Conversation> GetConversationAsync(int sessionId)
        {
            var conversation = await context.Conversations
                .Include(c => c.Entities)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            return conversation;
        }

        public async Task UpdateConversationAsync(int sessionId, string intent = null, ICollection<Entity> entities = null, bool IsCompleted = false, string status = null)
        {
            var conversation = await GetConversationAsync(sessionId);
            if (conversation == null)
            {
                await AddConversationAsync(sessionId, intent, entities, IsCompleted, status);
                return;
            }
            if (intent != null)
            {
                conversation.Intent = intent;
            }
            if (entities != null && entities.Any())
            {
                conversation.Entities.Clear();
                foreach (var entity in entities)
                {
                    conversation.Entities.Add(entity);
                }
            }
            if (IsCompleted)
            {
                conversation.IsCompleted = IsCompleted;
            }
            if (status != null)
            {
                conversation.Context = status;
            }
            context.Conversations.Update(conversation);
            await context.SaveChangesAsync();
        }
    }
}
