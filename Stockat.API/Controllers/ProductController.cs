using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.IServices;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController()]
    public class ProductController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceManager _serviceManager;

        public ProductController(ILoggerManager logger, IServiceManager serviceManager)
        {
            _logger = logger;
            _serviceManager = serviceManager;
        }


        [HttpGet]
        public async Task<IActionResult> getAllProductsPaginated(int size, int page)
        {

            var res = await _serviceManager.ProductService.getAllProductsPaginated(size, page);
            return Ok(res);
        }
    }
}
