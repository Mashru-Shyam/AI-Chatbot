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
                Entities = entities,
                IsCompleted = IsCompleted,
                Context = status
            };

            await context.Conversations.AddAsync(conversation);
            await context.SaveChangesAsync();
        }

        public async Task DeleteConversationAsync(int sessionId)
        {
            var conversation = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == sessionId);
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

        public async Task UpdateConversationAsync(int sessionId, string? intent = null, ICollection<Entity>? entities = null, bool? IsCompleted = null, string? status = null)
        {
            var conversation = await GetConversationAsync(sessionId);
            if (conversation != null)
            {
                if (intent != null)
                {
                    conversation.Intent = intent;
                }

                if (entities != null)
                {
                    conversation.Entities.Clear();
                    foreach (var entity in entities)
                    {
                        conversation.Entities.Add(entity);
                    }
                }

                if (IsCompleted != null)
                {
                    conversation.IsCompleted = IsCompleted.Value;
                }

                if (status != null)
                {
                    conversation.Context = status;
                }

                context.Conversations.Update(conversation);
                await context.SaveChangesAsync();
                return;
            }

            await AddConversationAsync(sessionId, intent, entities, IsCompleted.Value, status);
            return;
        }
    }
}
