using Stockat.Core.IServices;
using Stockat.Core.IServices.IAuctionServices;
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

    IEmailService EmailService { get; }

    IUserVerificationService UserVerificationService { get; }

    IServiceService ServiceService { get; }
    IServiceRequestService ServiceRequestService { get; }
    IServiceRequestUpdateService ServiceRequestUpdateService { get; }
    IProductService ProductService { get; }

    public IAuctionService AuctionService { get; }
    public IAuctionBidRequestService AuctionBidRequestService { get; }
    public IAuctionOrderService AuctionOrderService { get; }
    public ICategoryService CategoryService { get; }
    public ITagService TagService { get; }

    IUserService UserService { get; }
}
