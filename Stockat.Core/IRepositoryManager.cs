﻿using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using Stockat.Core.IRepositories;

namespace Stockat.Core;

public interface IRepositoryManager
{
    IBaseRepository<UserVerification> UserVerificationRepo { get; }
    IBaseRepository<UserPunishment> UserPunishmentRepo { get; }
    IBaseRepository<Stock> StockRepo { get; }
    IBaseRepository<Auction> AuctionRepo { get; }
    IBaseRepository<AuctionBidRequest> AuctionBidRequestRepo { get; }
    IBaseRepository<AuctionOrder> AuctionOrderRepo { get; }

    IServiceRepository ServiceRepo { get; }
    IBaseRepository<ServiceRequest> ServiceRequestRepo { get; }
    IBaseRepository<ServiceRequestUpdate> ServiceRequestUpdateRepo { get; }
    IBaseRepository<Category> CategoryRepo { get; }
    IBaseRepository<Tag> TagRepo { get; }



    IProductRepository ProductRepository { get; }
    IOrderRepository OrderRepo { get; }
    IBaseRepository<StockDetails> StockDetailsRepo { get; }
    IBaseRepository<OrderProductAudit> OrderProductAudit { get; }

    //IBaseRepository<OrderProduct> OrderRepo { get; }

    IBaseRepository<Driver> DriverRepo { get; }


    IUserRepository UserRepo { get; }


    IBaseRepository<ChatConversation> ChatConversationRepo { get; }
    IBaseRepository<ChatMessage> ChatMessageRepo { get; }
    IBaseRepository<MessageReadStatus> MessageReadStatusRepo { get; }
    IBaseRepository<MessageReaction> MessageReactionRepo { get; }
    IChatBotMessageRepository ChatBotMessageRepository { get; }

    IBaseRepository<ServiceEditRequest> ServiceEditRequestRepo { get; }
    IReviewRepository ReviewRepo { get; }

    int Complete();
    void Dispose();


    Task<int> CompleteAsync();


    Task DisposeAsync();

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
