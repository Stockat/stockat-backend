﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Stockat.Core;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using Stockat.Service.Services;

namespace Stockat.Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthenticationService> _authenticationService;
    private readonly Lazy<IImageService> _imageService;
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IUserVerificationService> _userVerificationService;
    private readonly Lazy<IProductService> _productService;
    public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper,
        UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor) //, IProductService productService
    {
        _imageService = new Lazy<IImageService>(() => new ImageKitService(configuration));
        _emailService = new Lazy<IEmailService>(() => new EmailService(configuration));
        _productService = new Lazy<IProductService>(() => new ProductService(logger, mapper, repositoryManager));


        _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(logger, mapper, userManager, roleManager, configuration, _emailService.Value));

        // if you wanna use a lazy loading service in another service initilize it first before sending it to the other layer like i did in the _imageSerive and passed to the UserVerificationService
        _userVerificationService = new Lazy<IUserVerificationService>(() => new UserVerificationService(logger, mapper, configuration, _imageService.Value, repositoryManager, httpContextAccessor));
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


    public IUserVerificationService UserVerificationService => _userVerificationService.Value;
}
