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
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups.Include(c => c.Connections)
                                   .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
                                   .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await context.Groups.Include(c => c.Connections).FirstOrDefaultAsync(g=> g.Name == groupName);
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
        var query = context.Messages
            .Where(m => m.Recipient.UserName == currentUserName && !m.RecipientDeleted && m.Sender.UserName == recipientUserName || 
                        m.Sender.UserName == currentUserName && !m.SenderDeleted && m.Recipient.UserName == recipientUserName)
            .OrderBy(m => m.MessageSent)
            .AsQueryable();

        var unreadMessages = query.Where(m => m.DateRead == null && m.RecipientUserName == currentUserName).ToList();

        if(unreadMessages.Count != 0) {
            unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
        }

        return await query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }
}
