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

        //Adding a new Conversation
        public async Task AddConversationAsync(int sessionId, string intent, ICollection<Entity>? entities, bool IsCompleted, string status)
        {
            var conversation = new Conversation
            {
                SessionId = sessionId,
                Intent = intent,
                IsCompleted = IsCompleted,
                Context = status,
                Entities = entities ?? []
            };

            await context.Conversations.AddAsync(conversation);
            await context.SaveChangesAsync();
        }

        //Delete a Conversation
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

        //Delete Entities
        public async Task DeleteEntitiesAsync(int sessionId)
        {
            var conversation = await context.Conversations
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            if (conversation == null)
            {
                return;
            }
            var conversationId = conversation.ConversationId;
            var entities = context.Entities.Where(e => e.ConversationId == conversationId);
            context.Entities.RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        //Retrive Conversation
        public async Task<Conversation> GetConversationAsync(int sessionId)
        {
            var conversation = await context.Conversations
                .Include(c => c.Entities)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            return conversation ?? new Conversation();
        }
        
        //Update a Conversation
        public async Task UpdateConversationAsync(int sessionId, string intent = "None", ICollection<Entity>? entities = null, bool IsCompleted = false, string status = "Start")
        {
            var conversation = await GetConversationAsync(sessionId);
            if (conversation.ConversationId == 0)
            {
                await AddConversationAsync(sessionId, intent, entities, IsCompleted, status);
                return;
            }
            if (intent != "None")
            {
                conversation.Intent = intent;
            }
            if (entities != null && entities.Count > 0)
            {
                //conversation.Entities.Clear();
                foreach (var entity in entities)
                {
                    conversation.Entities.Add(entity);
                }
            }
            conversation.IsCompleted = IsCompleted;
            if (status != "Start")
            {
                conversation.Context = status;
            }
            context.Conversations.Update(conversation);
            await context.SaveChangesAsync();
        }
    }
}
