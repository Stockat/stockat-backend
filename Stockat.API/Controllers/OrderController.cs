using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockat.Core.IServices;
using Stockat.Core;
using Stockat.Core.DTOs.OrderDTOs;
using Stockat.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Stripe.Checkout;
using Stockat.Core.DTOs;
using Stripe;
using Stockat.Core.DTOs.OrderDTOs.OrderAnalysisDto;

namespace Stockat.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{

    private readonly ILoggerManager _logger;
    private readonly IServiceManager _serviceManager;

    public OrderController(ILoggerManager logger, IServiceManager serviceManager)
    {
        _logger = logger;
        _serviceManager = serviceManager;
    }

    // Add Order
    [HttpPost]
    public IActionResult AddOrder([FromBody] AddOrderDTO orderDto)
    {
        // Validate the input
        if (orderDto == null)
        {
            _logger.LogError("AddOrder: Order DTO is null.");
            return BadRequest("Full order data is required.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError("AddOrder: Invalid model state.");
            return BadRequest(ModelState);
        }

        try
        {
            // Call the service to add the order
            var domain = $"{Request.Scheme}://{Request.Host}/";
            var response = _serviceManager.OrderService.AddOrderAsync(orderDto, domain).Result;
            // Return the response
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"AddOrder: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the order.");
        }
    }




    // Update Order Status
    [HttpPut("{id}")]
    public IActionResult UpdateOrderStatus(int id, [FromBody] OrderStatus status)
    {
        // Validate the input
        if (status == null)
        {
            _logger.LogError("UpdateOrderStatus: Status is null.");
            return BadRequest("Order status is required.");
        }
        try
        {
            // Call the service to update the order status
            var response = _serviceManager.OrderService.UpdateOrderStatusAsync(id, status).Result;
            // Return the response
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"UpdateOrderStatus: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order status.");
        }
    }

    // Get All Orders For Seller
    [HttpGet("seller")]
    public IActionResult GetAllOrdersForSeller()
    {
        try
        {
            // Call the service to get all orders for the seller
            var response = _serviceManager.OrderService.GetAllSellerOrdersAsync().Result;
            // Return the response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAllOrdersForSeller: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving orders.");
        }
    }

    [HttpGet("seller/req")]
    public IActionResult GetAllRequestOrdersForSeller()
    {
        try
        {
            // Call the service to get all orders for the seller
            var response = _serviceManager.OrderService.GetAllSellerRequestOrdersAsync().Result;
            // Return the response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAllSellerRequestOrdersAsync: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving Request orders.");
        }
    }


    // Get All Orders For Admin
    [HttpGet("admin")]
    public IActionResult GetAllOrdersandRequestForAdmin()
    {
        try
        {
            // Call the service to get all orders for the seller
            var response = _serviceManager.OrderService.GetAllOrdersandRequestforAdminAsync().Result;
            // Return the response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAllOrdersForSeller: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving orders For Admin.");
        }
    }

    // User
    // Get All Orders For User
    [HttpGet("user")]
    public IActionResult GetAllOrdersForBuyer()
    {
        try
        {
            // Call the service to get all orders for the seller
            var response = _serviceManager.OrderService.GetAllBuyerOrdersAsync().Result;
            // Return the response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAllOrdersForSeller: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving Buyer orders.");
        }
    }

    [HttpGet("user/req")]
    public IActionResult GetAllRequestOrdersForBuyer()
    {
        try
        {
            // Call the service to get all orders for the seller
            var response = _serviceManager.OrderService.GetAllBuyerRequestOrdersAsync().Result;
            // Return the response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAllSellerRequestOrdersAsync: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving Buyer Request orders.");
        }
    }

    // Add Request
    [HttpPost("request")]
    public IActionResult AddRequest([FromBody] AddRequestDTO requestDto)
    {
        // Validate the input
        if (requestDto == null)
        {
            _logger.LogError("AddRequest: Request DTO is null.");
            return BadRequest("Full request data is required.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError("AddRequest: Invalid model state.");
            return BadRequest(ModelState);
        }

        try
        {
            // Call the service to add the request
            var response = _serviceManager.OrderService.AddRequestAsync(requestDto).Result;
            // Return the response
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"AddRequest: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the request.");
        }
    }


    // Analysis 
    [AllowAnonymous]
    [HttpGet("analysis/orderCount")]
    public async Task<IActionResult> GetOrderCountsByTypeAsync()
    {
        var res = _serviceManager.OrderService.GetOrderCountsByTypeAsync().Result;
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpGet("analysis/orderSales")]
    public async Task<IActionResult> GetTotalSalesByOrderTypeAsync()
    {
        var res = _serviceManager.OrderService.GetTotalSalesByOrderTypeAsync().Result;

        return Ok(res);
    }
    [AllowAnonymous]
    [HttpGet("analysis/OrdersVsStatus")]
    public async Task<IActionResult> CalculateOrderVsStatus(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {
        //var res = _serviceManager.OrderService.CalculateMonthlyRevenueOrderVsStatus(type, status, metricType);
        var res = _serviceManager.OrderService.CalculateWeeklyRevenueOrderVsStatus(type, status, metricType);
        //var res = _serviceManager.OrderService.CalculateYearlyRevenueOrderVsStatus(type, status, metricType);

        return Ok(res);
    }

    [AllowAnonymous]
    [HttpGet("analysis/TopProductOrder")]
    public async Task<IActionResult> CalculateTopProductOrder(OrderType? type, OrderStatus? status, ReportMetricType metricType)
    {

        //var res = _serviceManager.OrderService.GetTopProductPerYearAsync(type, status, metricType);
        var res = _serviceManager.OrderService.GetTopProductPerMonthAsync(type, status, metricType);
        // var res = _serviceManager.OrderService.GetTopProductPerWeekAsync(type, status, metricType);

        return Ok(res);
    }



}




