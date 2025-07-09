using Microsoft.EntityFrameworkCore.Storage;
using AutoMapper;
using Stockat.Core;
using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using Stockat.Core.IRepositories;
using Stockat.Core.IServices;
using Stockat.EF.Repositories;

namespace Stockat.EF;

public class RepositoryManager : IRepositoryManager
{
    private readonly StockatDBContext _context;
    private readonly Lazy<IBaseRepository<UserVerification>> _userVerificationRepo;
    private readonly Lazy<IBaseRepository<UserPunishment>> _userPunishmentRepo;
    private readonly Lazy<IBaseRepository<Auction>> _AuctionRepo;
    private readonly Lazy<IBaseRepository<Stock>> _StockRepo;
    private readonly Lazy<IBaseRepository<AuctionBidRequest>> _auctionBidRequestRepo;
    private readonly Lazy<IBaseRepository<AuctionOrder>> _auctionOrderRepo;
    private readonly Lazy<IBaseRepository<Category>> _CategoryRepo;
    private readonly Lazy<IBaseRepository<Tag>> _TagRepo;


    private IDbContextTransaction _transaction;

    //public RepositoryManager(StockatDBContext context)
    private readonly Lazy<ProductRepository> _productRepository;
    private readonly Lazy<IBaseRepository<Stock>> _stockRepository;
    private readonly Lazy<IBaseRepository<StockDetails>> _stockDetailsRepository;
    private readonly Lazy<OrderRepository> _OrderRepo;
    private readonly IMapper _mapper;

    private readonly Lazy<ServiceRepository> _serviceRepo;
    private readonly Lazy<IBaseRepository<ServiceRequest>> _serviceRequestRepo;
    private readonly Lazy<IBaseRepository<ServiceRequestUpdate>> _serviceRequestUpdateRepo;


    private readonly Lazy<IBaseRepository<ChatConversation>> _chatConversationRepo;
    private readonly Lazy<IBaseRepository<ChatMessage>> _chatMessageRepo;
    private readonly Lazy<IBaseRepository<MessageReadStatus>> _messageReadStatusRepo;
    private readonly Lazy<IBaseRepository<MessageReaction>> _messageReactionRepo;



    private readonly Lazy<IUserRepository> _userRepo;
    private readonly Lazy<IChatBotMessageRepository> _chatBotMessageRepository;

    private readonly Lazy<IBaseRepository<ServiceEditRequest>> _serviceEditRequestRepo;
    public RepositoryManager(StockatDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;

        _userVerificationRepo = new Lazy<IBaseRepository<UserVerification>>(() => new BaseRepository<UserVerification>(_context));
        _userPunishmentRepo = new Lazy<IBaseRepository<UserPunishment>>(() => new BaseRepository<UserPunishment>(_context));
        _AuctionRepo = new Lazy<IBaseRepository<Auction>>(() => new BaseRepository<Auction>(_context));
        _StockRepo = new Lazy<IBaseRepository<Stock>>(() => new BaseRepository<Stock>(_context));
        _auctionBidRequestRepo = new Lazy<IBaseRepository<AuctionBidRequest>>(() => new BaseRepository<AuctionBidRequest>(_context));
        _auctionOrderRepo = new Lazy<IBaseRepository<AuctionOrder>>(() => new BaseRepository<AuctionOrder>(_context));
        _CategoryRepo = new Lazy<IBaseRepository<Category>>(() => new BaseRepository<Category>(_context));
        _TagRepo = new Lazy<IBaseRepository<Tag>>(() => new BaseRepository<Tag>(_context));

        _userRepo = new Lazy<IUserRepository>(() => new UserRepository(_context));
        _productRepository = new Lazy<ProductRepository>(() => new ProductRepository(_context, _mapper));

        _serviceRepo = new Lazy<ServiceRepository>(() => new ServiceRepository(_context));

        _serviceRequestRepo = new Lazy<IBaseRepository<ServiceRequest>>(() => new BaseRepository<ServiceRequest>(_context));
        _serviceRequestUpdateRepo = new Lazy<IBaseRepository<ServiceRequestUpdate>>(() => new BaseRepository<ServiceRequestUpdate>(_context));
        _stockRepository = new Lazy<IBaseRepository<Stock>>(() => new BaseRepository<Stock>(_context));
        _stockDetailsRepository = new Lazy<IBaseRepository<StockDetails>>(() => new BaseRepository<StockDetails>(_context));
        _OrderRepo = new Lazy<OrderRepository>(() => new OrderRepository(_context, _mapper));

        _chatConversationRepo = new Lazy<IBaseRepository<ChatConversation>>(() => new BaseRepository<ChatConversation>(_context));
        _chatMessageRepo = new Lazy<IBaseRepository<ChatMessage>>(() => new BaseRepository<ChatMessage>(_context));
        _messageReadStatusRepo = new Lazy<IBaseRepository<MessageReadStatus>>(() => new BaseRepository<MessageReadStatus>(_context));
        _messageReactionRepo = new Lazy<IBaseRepository<MessageReaction>>(() => new BaseRepository<MessageReaction>(_context));

        _chatBotMessageRepository = new Lazy<IChatBotMessageRepository>(() => new ChatBotMessageRepository(_context));

        _serviceEditRequestRepo = new Lazy<IBaseRepository<ServiceEditRequest>>(() => new BaseRepository<ServiceEditRequest>(_context));
    }

    public IBaseRepository<UserVerification> UserVerificationRepo => _userVerificationRepo.Value;
    public IBaseRepository<UserPunishment> UserPunishmentRepo => _userPunishmentRepo.Value;
    public IBaseRepository<Auction> AuctionRepo => _AuctionRepo.Value;
    public IBaseRepository<Stock> StockRepo => _StockRepo.Value;
    public IBaseRepository<AuctionBidRequest> AuctionBidRequestRepo => _auctionBidRequestRepo.Value;
    public IBaseRepository<AuctionOrder> AuctionOrderRepo => _auctionOrderRepo.Value;
    public IBaseRepository<Category> CategoryRepo => _CategoryRepo.Value;
    public IBaseRepository<Tag> TagRepo => _TagRepo.Value;


    public IServiceRepository ServiceRepo => _serviceRepo.Value;
    public IBaseRepository<ServiceRequest> ServiceRequestRepo => _serviceRequestRepo.Value;
    public IBaseRepository<ServiceRequestUpdate> ServiceRequestUpdateRepo => _serviceRequestUpdateRepo.Value;
    public IProductRepository ProductRepository => _productRepository.Value;

    public IUserRepository UserRepo => _userRepo.Value;
    public IBaseRepository<StockDetails> StockDetailsRepo => _stockDetailsRepository.Value;
    public IOrderRepository OrderRepo => _OrderRepo.Value;

    public IBaseRepository<ChatConversation> ChatConversationRepo => _chatConversationRepo.Value;
    public IBaseRepository<ChatMessage> ChatMessageRepo => _chatMessageRepo.Value;
    public IBaseRepository<MessageReadStatus> MessageReadStatusRepo => _messageReadStatusRepo.Value;
    public IBaseRepository<MessageReaction> MessageReactionRepo => _messageReactionRepo.Value;

    public IChatBotMessageRepository ChatBotMessageRepository => _chatBotMessageRepository.Value;

    public IBaseRepository<ServiceEditRequest> ServiceEditRequestRepo => _serviceEditRequestRepo.Value;

    public int Complete()
    {
        return _context.SaveChanges(); // returns no. of rows affected
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task DisposeAsync()
    {
        _context.DisposeAsync();
    }

    public async Task BeginTransactionAsync() =>
        _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }
}
