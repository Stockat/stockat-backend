using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

public class ChatHistoryService : IChatHistoryService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    // In-memory storage for chatbot messages (in production, you might want to use a separate table)
    private static readonly Dictionary<string, List<ChatBotMessageDto>> _chatbotMessages = new();
    private static readonly object _lockObject = new object();

    public ChatHistoryService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task SaveMessageAsync(string userId, string role, string message)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

        if (string.IsNullOrEmpty(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));

        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        if (message.Length > 2000)
            throw new ArgumentException("Message is too long. Maximum 2000 characters allowed.", nameof(message));

        // For chatbot, we'll store messages in memory for simplicity
        // In a production environment, you might want to create a separate ChatBotMessage entity
        
        var chatMessageDto = new ChatBotMessageDto
        {
            MessageText = message,
            SenderId = role == "user" ? userId : "system",
            SentAt = DateTime.UtcNow,
            Role = role == "user" ? "user" : "assistant"
        };

        lock (_lockObject)
        {
            if (!_chatbotMessages.ContainsKey(userId))
            {
                _chatbotMessages[userId] = new List<ChatBotMessageDto>();
            }

            _chatbotMessages[userId].Add(chatMessageDto);

            // Keep only the last 100 messages per user to prevent memory issues
            if (_chatbotMessages[userId].Count > 100)
            {
                _chatbotMessages[userId] = _chatbotMessages[userId].Skip(_chatbotMessages[userId].Count - 100).ToList();
            }
        }
    }

    public async Task<IEnumerable<ChatBotMessageDto>> GetChatHistoryAsync(string userId, int limit = 50)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

        if (limit <= 0 || limit > 100)
            throw new ArgumentException("Limit must be between 1 and 100.", nameof(limit));

        lock (_lockObject)
        {
            if (!_chatbotMessages.ContainsKey(userId))
            {
                return new List<ChatBotMessageDto>();
            }

            var messages = _chatbotMessages[userId]
                .OrderBy(m => m.SentAt)
                .Take(limit)
                .ToList();

            return messages;
        }
    }

    public async Task ClearChatHistoryAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

        lock (_lockObject)
        {
            if (_chatbotMessages.ContainsKey(userId))
            {
                _chatbotMessages[userId].Clear();
            }
        }
    }

    public async Task SaveChatBotMessageAsync(string userId, string role, string message)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
        if (string.IsNullOrEmpty(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        if (message.Length > 2000)
            throw new ArgumentException("Message is too long. Maximum 2000 characters allowed.", nameof(message));

        var chatBotMessage = new ChatBotMessage
        {
            UserId = userId,
            Role = role == "user" ? "user" : "assistant",
            Content = message,
            Timestamp = DateTime.UtcNow
        };
        await _repositoryManager.ChatBotMessageRepository.AddAsync(chatBotMessage);
        await _repositoryManager.CompleteAsync();
    }

    public async Task<IEnumerable<ChatBotMessageDto>> GetChatBotHistoryAsync(string userId, int limit = 50)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
        if (limit <= 0 || limit > 100)
            throw new ArgumentException("Limit must be between 1 and 100.", nameof(limit));

        var messages = await _repositoryManager.ChatBotMessageRepository.FindAllAsync(
            m => m.UserId == userId,
            orderBy: m => m.Timestamp,
            orderByDirection: "ASC",
            take: limit,
            skip:0
        );
        return messages.Select(m => new ChatBotMessageDto
        {
            MessageText = m.Content,
            SenderId = m.Role == "user" ? userId : "system",
            SentAt = m.Timestamp,
            Role = m.Role
        }).ToList();
    }

    public async Task ClearChatBotHistoryAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
        await _repositoryManager.ChatBotMessageRepository.DeleteAsync(m => m.UserId == userId);
        await _repositoryManager.CompleteAsync();
    }
} 