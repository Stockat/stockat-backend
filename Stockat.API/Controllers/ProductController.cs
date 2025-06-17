using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController()]
    public class ProductController : ControllerBase
    {
        //private readonly ILogger _logger;
        //private readonly IServiceManager _serviceManager;

        //public ProductController(ILogger logger, IServiceManager serviceManager)
        //{
        //    _logger = logger;
        //    _serviceManager = serviceManager;
        //}


        //[HttpGet]
        //public IActionResult getAllProductsPaginated()
        //{

        //    var res = _serviceManager.ProductService.getAllProductsPaginated();
        //    return Ok(res);
        //}
    }
}
