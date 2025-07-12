using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.IServices;
using Stockat.Core;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {

        private readonly ILoggerManager _logger;
        private readonly IServiceManager _serviceManager;


        public AuditController(ILoggerManager logger, IServiceManager serviceManager)
        {
            _logger = logger;
            _serviceManager = serviceManager;
        }


        [HttpGet]
        public async Task<IActionResult> getAllOrdersAudit()
        {

            var res = _serviceManager.orderProductAuditService.getallAsync().Result;

            return Ok(res);

        }
    }
}
