using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using Stockat.Core.IRepositories;

namespace Stockat.Core;

public interface IRepositoryManager
{
    IBaseRepository<UserVerification> UserVerificationRepo { get; }
    IBaseRepository<Service> ServiceRepo { get; }
    IBaseRepository<ServiceRequest> ServiceRequestRepo { get; }
    IBaseRepository<ServiceRequestUpdate> ServiceRequestUpdateRepo { get; }
    IProductRepository ProductRepository { get; }


    IBaseRepository<User> UserRepo { get; }


    IBaseRepository<ChatConversation> ChatConversationRepo { get; }
    IBaseRepository<ChatMessage> ChatMessageRepo { get; }
    IBaseRepository<MessageReadStatus> MessageReadStatusRepo { get; }
    IBaseRepository<MessageReaction> MessageReactionRepo { get; }
    int Complete();
    void Dispose();


    Task<int> CompleteAsync();


    Task DisposeAsync();
}
