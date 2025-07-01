using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
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
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IUserVerificationService> _userVerificationService;
    private readonly Lazy<IServiceService> _serviceService;
    private readonly Lazy<IServiceRequestService> _serviceRequestService;
    private readonly Lazy<IServiceRequestUpdateService> _serviceRequestUpdateService;
    private readonly Lazy<IProductService> _productService;

    private readonly Lazy<IAuctionService> _auctionService;
    private readonly Lazy<IAuctionBidRequestService> _auctionBidRequestService;
    private readonly Lazy<IAuctionOrderService> _auctionOrderService;

    private readonly Lazy<IUserService> _userService;

    public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _imageService = new Lazy<IImageService>(() => new ImageKitService(configuration));
        _emailService = new Lazy<IEmailService>(() => new EmailService(configuration));
        _productService = new Lazy<IProductService>(() => new ProductService(logger, mapper, repositoryManager, _imageService.Value));

        _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(logger, mapper, userManager, roleManager, configuration, _emailService.Value));

        // Service Feature
        _serviceService = new Lazy<IServiceService>(() => new ServiceService(logger, mapper, repositoryManager, _imageService.Value));
        _serviceRequestService = new Lazy<IServiceRequestService>(() => new ServiceRequestService(logger, mapper, repositoryManager));
        _serviceRequestUpdateService = new Lazy<IServiceRequestUpdateService>(() => new ServiceRequestUpdateService(logger, mapper, repositoryManager));

        // if you wanna use a lazy loading service in another service initilize it first before sending it to the other layer like i did in the _imageSerive and passed to the UserVerificationService
        _userVerificationService = new Lazy<IUserVerificationService>(() => new UserVerificationService(logger, mapper, configuration, _imageService.Value, repositoryManager, httpContextAccessor));
        
        //
        _auctionService = new Lazy<IAuctionService>(() => new AuctionService(mapper, logger, repositoryManager));
        _auctionBidRequestService = new Lazy<IAuctionBidRequestService>(() => new AuctionBidRequestService(repositoryManager, mapper));
        _auctionOrderService = new Lazy<IAuctionOrderService>(() => new AuctionOrderService(repositoryManager, mapper));

        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager, mapper, httpContextAccessor, _imageService.Value, userManager, _emailService.Value));
    }

    public IAuthenticationService AuthenticationService
    {
        get { return _authenticationService.Value; }
    }
    // we could use the expression embodied function instead like the below instead of the above
    //public IAuthenticationService AuthenticationService => _authenticationService.Value;

    public IImageService ImageService => _imageService.Value;

    public IEmailService EmailService => _emailService.Value;
    public IProductService ProductService => _productService.Value;

    public IAuctionService AuctionService => _auctionService.Value;
    public IAuctionBidRequestService AuctionBidRequestService => _auctionBidRequestService.Value;
    public IAuctionOrderService AuctionOrderService => _auctionOrderService.Value;

    public IUserVerificationService UserVerificationService => _userVerificationService.Value;
    public IServiceService ServiceService => _serviceService.Value;
    public IServiceRequestService ServiceRequestService => _serviceRequestService.Value;
    public IServiceRequestUpdateService ServiceRequestUpdateService => _serviceRequestUpdateService.Value;

    public IUserService UserService => _userService.Value;
}
