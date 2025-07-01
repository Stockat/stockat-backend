using AutoMapper;
using Stockat.Core.DTOs.ChatDTOs;
using Stockat.Core.Entities.Chat;
using Stockat.Core.Entities;
using System.Linq;

namespace Stockat.Service.MappingProfiles;

/// <summary>
/// AutoMapper profile for chat entities and DTOs.
/// </summary>
public class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<User, UserChatInfoDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl));

        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
            .ForMember(dest => dest.Reactions, opt => opt.MapFrom(src => src.Reactions));

        CreateMap<ChatConversation, ChatConversationDto>()
            .ForMember(dest => dest.User1, opt => opt.MapFrom(src => src.User1))
            .ForMember(dest => dest.User2, opt => opt.MapFrom(src => src.User2))
            .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages))
            .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src =>
                src.Messages != null && src.Messages.Any()
                    ? src.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
                    : null
            ));

        CreateMap<MessageReaction, MessageReactionDto>();
    }
}