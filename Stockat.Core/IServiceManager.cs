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
    IFileService FileService { get; }

    IEmailService EmailService { get; }

    IUserVerificationService UserVerificationService { get; }
    IUserPunishmentService UserPunishmentService { get; }

    IServiceService ServiceService { get; }
    IServiceRequestService ServiceRequestService { get; }
    IServiceRequestUpdateService ServiceRequestUpdateService { get; }
    IProductService ProductService { get; }
    IStockService StockService { get; }
    IOrderService OrderService { get; }
    IDriverService DriverService { get; }
    IOrderProductAuditService orderProductAuditService { get; }

    public IAuctionService AuctionService { get; }
    public IAuctionBidRequestService AuctionBidRequestService { get; }
    public IAuctionOrderService AuctionOrderService { get; }
    public ICategoryService CategoryService { get; }
    public ITagService TagService { get; }

    IUserService UserService { get; }

    IChatService ChatService { get; }
    IChatHistoryService ChatHistoryService { get; }
    IAIService AIService { get; }
    IAnalyticsService AnalyticsService { get; }
    IServiceEditRequestService ServiceEditRequestService { get; }
    IReviewService ReviewService { get; }

    IOpenAIService OpenAIService { get;  }
}
