using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;


namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages.OrderByDescending(m => m.MessageSent)
                                    .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.UserName && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.Sender.UserName == messageParams.UserName && !m.SenderDeleted),
            _ => query.Where(m => m.Recipient.UserName == messageParams.UserName && m.DateRead == null && !m.RecipientDeleted)
        };

        var messages = query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);

        return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.pageNumber, messageParams.PageSize);
        
    }

    public async Task<IEnumerable<MessageDTO>> GetMessageThreads(string currentUserName, string recipientUserName)
    {
        var messages = await context.Messages
            .Include(s => s.Sender).ThenInclude(s => s.Photos)
            .Include(r => r.Recipient).ThenInclude(r => r.Photos)
            .Where(m => m.Recipient.UserName == currentUserName && !m.RecipientDeleted && m.Sender.UserName == recipientUserName || 
                        m.Sender.UserName == currentUserName && !m.SenderDeleted && m.Recipient.UserName == recipientUserName)
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUserName).ToList();

        if(unreadMessages.Count != 0) {
            unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDTO>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
