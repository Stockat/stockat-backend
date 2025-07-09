
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using Stockat.Infrastructure.Services;
using Stockat.Core.IServices.IAuctionServices;
using Stockat.Service.Services;
using Stockat.Service.Services.AuctionServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthenticationService> _authenticationService;
    private readonly Lazy<IImageService> _imageService;
    private readonly Lazy<IFileService> _fileService;
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IUserVerificationService> _userVerificationService;
    private readonly Lazy<IUserPunishmentService> _userPunishmentService;
    private readonly Lazy<IServiceService> _serviceService;
    private readonly Lazy<IServiceRequestService> _serviceRequestService;
    private readonly Lazy<IServiceRequestUpdateService> _serviceRequestUpdateService;
    private readonly Lazy<IProductService> _productService;
    private readonly Lazy<ICategoryService> _categoryService;
    private readonly Lazy<ITagService> _tagService;
    private readonly Lazy<IStockService> _stockService;
    private readonly Lazy<IOrderService> _orderService;
    private readonly Lazy<IChatService> _chatService;

    private readonly Lazy<IAuctionService> _auctionService;
    private readonly Lazy<IAuctionBidRequestService> _auctionBidRequestService;
    private readonly Lazy<IAuctionOrderService> _auctionOrderService;

    private readonly Lazy<IUserService> _userService;

    private readonly Lazy<IChatHistoryService> _chatHistoryService;
    private readonly Lazy<IAIService> _aiService;
    private readonly Lazy<IAnalyticsService> _analyticsService;
    private readonly Lazy<IServiceEditRequestService> _serviceEditRequestService;

    public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _imageService = new Lazy<IImageService>(() => new ImageKitService(configuration));
        _emailService = new Lazy<IEmailService>(() => new EmailService(configuration));
        _productService = new Lazy<IProductService>(() => new ProductService(logger, mapper, repositoryManager, _imageService.Value, httpContextAccessor));
        _fileService = new Lazy<IFileService>(() => new CloudinaryFileService(configuration));

        _chatService = new Lazy<IChatService>(() => new ChatService(repositoryManager, mapper, _imageService.Value, _fileService.Value, configuration));

        // Initialize UserService first since other services depend on it
        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager, mapper, httpContextAccessor, _imageService.Value, userManager, _emailService.Value));

        _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(logger, mapper, userManager, roleManager, configuration, _emailService.Value, _chatService.Value, repositoryManager, httpContextAccessor));
        // Stock Service
        _stockService = new Lazy<IStockService>(() => new StockService(logger, mapper, repositoryManager, httpContextAccessor));

        // Order Service
        _orderService = new Lazy<IOrderService>(() => new OrderService(logger, mapper, repositoryManager, httpContextAccessor, _emailService.Value));


        _serviceService = new Lazy<IServiceService>(() => new ServiceService(logger, mapper, repositoryManager, _imageService.Value));
        _serviceRequestService = new Lazy<IServiceRequestService>(() => new ServiceRequestService(logger, mapper, repositoryManager, _emailService.Value, _userService.Value, _serviceEditRequestService.Value));
        _serviceRequestUpdateService = new Lazy<IServiceRequestUpdateService>(() => new ServiceRequestUpdateService(logger, mapper, repositoryManager, _emailService.Value));

        // UserVerificationService with email and user service dependencies
        _userVerificationService = new Lazy<IUserVerificationService>(() => new UserVerificationService(logger, mapper, configuration, _imageService.Value, repositoryManager, httpContextAccessor, _emailService.Value, _userService.Value));

        // UserPunishmentService with dependencies
        _userPunishmentService = new Lazy<IUserPunishmentService>(() => new UserPunishmentService(logger, mapper, repositoryManager, httpContextAccessor, _emailService.Value, _userService.Value));

        // Stock Service
        _stockService = new Lazy<IStockService>(() => new StockService(logger, mapper, repositoryManager, httpContextAccessor));

        // Auction Services
        _auctionService = new Lazy<IAuctionService>(() => new AuctionService(mapper, logger, repositoryManager));
        _auctionBidRequestService = new Lazy<IAuctionBidRequestService>(() => new AuctionBidRequestService(repositoryManager, mapper));
        _auctionOrderService = new Lazy<IAuctionOrderService>(() => new AuctionOrderService(repositoryManager, mapper));

        _categoryService = new Lazy<ICategoryService>(() => new CategoryService(logger, mapper, repositoryManager));
        _tagService = new Lazy<ITagService>(() => new TagService(logger, mapper, repositoryManager));

        _chatHistoryService = new Lazy<IChatHistoryService>(() => new ChatHistoryService(repositoryManager, mapper));

        _aiService = new Lazy<IAIService>(() => new AIService(this, logger));
        _analyticsService = new Lazy<IAnalyticsService>(() => new AnalyticsService(repositoryManager, mapper, logger));

        _serviceEditRequestService = new Lazy<IServiceEditRequestService>(() => new ServiceEditRequestService(logger, mapper, repositoryManager, _imageService.Value, _emailService.Value));
    }

    public IAuthenticationService AuthenticationService
    {
        get { return _authenticationService.Value; }
    }
    // we could use the expression embodied function instead like the below instead of the above
    //public IAuthenticationService AuthenticationService => _authenticationService.Value;

    public IImageService ImageService => _imageService.Value;
    public IFileService FileService => _fileService.Value;

    public IEmailService EmailService => _emailService.Value;
    public IProductService ProductService => _productService.Value;

    public IStockService StockService => _stockService.Value;

    public IOrderService OrderService => _orderService.Value;

    public IAuctionService AuctionService => _auctionService.Value;
    public IAuctionBidRequestService AuctionBidRequestService => _auctionBidRequestService.Value;

    public IServiceService ServiceService => _serviceService.Value;
    public IServiceRequestService ServiceRequestService => _serviceRequestService.Value;
    public IServiceRequestUpdateService ServiceRequestUpdateService => _serviceRequestUpdateService.Value;
    public IUserService UserService => _userService.Value;

    public IChatService ChatService => _chatService.Value;

    public IAuctionOrderService AuctionOrderService => _auctionOrderService.Value;

    public ICategoryService CategoryService => _categoryService.Value;
    public ITagService TagService => _tagService.Value;

    public IUserVerificationService UserVerificationService => _userVerificationService.Value;



    public IUserPunishmentService UserPunishmentService => _userPunishmentService.Value;

    public IChatHistoryService ChatHistoryService => _chatHistoryService.Value;
    public IAnalyticsService AnalyticsService => _analyticsService.Value;
    public IAIService AIService => _aiService.Value;
    public IServiceEditRequestService ServiceEditRequestService => _serviceEditRequestService.Value;
}
