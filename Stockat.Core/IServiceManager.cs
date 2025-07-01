using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core;

public interface IServiceManager
{
    IAuthenticationService AuthenticationService { get; }
    IImageService ImageService { get; }
    IFileService FileService { get; }

    IEmailService EmailService { get; }

    IUserVerificationService UserVerificationService { get; }

    IServiceService ServiceService { get; }
    IServiceRequestService ServiceRequestService { get; }
    IServiceRequestUpdateService ServiceRequestUpdateService { get; }
    IProductService ProductService { get; }

    IUserService UserService { get; }

    IChatService ChatService { get; }
}
