using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.IServices;

namespace Stockat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {

        
        private readonly ILoggerManager _logger;
        private readonly IServiceManager _serviceManager;
        public DriverController(ILoggerManager logger, IServiceManager serviceManager)
        {
            _logger = logger;
            _serviceManager = serviceManager;
        }

        [HttpPost]
        public IActionResult getLoc([FromBody] LocationDTO x)
        {
            // Return a success response
            Console.WriteLine(x.Longitude+x.Latitude);
            return Ok(new { message = x });
           
        }

        // Add Driver
        [HttpPost("add")]
        public async Task<IActionResult> AddDriver([FromBody] Stockat.Core.DTOs.DriverDTOs.DriverCreateDto dto)
        {
            var res = await _serviceManager.DriverService.AddDriverAsync(dto);
            return Ok(res);
        }

        // Update Driver
        [HttpPut("update")]
        public async Task<IActionResult> UpdateDriver([FromBody] Stockat.Core.DTOs.DriverDTOs.DriverUpdateDto dto)
        {
            var res = await _serviceManager.DriverService.UpdateDriverAsync(dto);
            return Ok(res);
        }

        // Update Driver Status (location, message, last update time)
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateDriverStatus([FromBody] Stockat.Core.DTOs.DriverDTOs.DriverStatusUpdateDto dto)
        {
            var res = await _serviceManager.DriverService.UpdateDriverStatusAsync(dto);
            return Ok(res);
        }

        // Get driver by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(string id)
        {
            var res = await _serviceManager.DriverService.GetDriverByIdAsync(id);
            return Ok(res);
        }

        // Get all drivers
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            var res = await _serviceManager.DriverService.GetAllDriversAsync();
            return Ok(res);
        }
    }
}
