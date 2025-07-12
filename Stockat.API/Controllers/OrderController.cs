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
    // Update Order Status

    [HttpPut("request/{id}")]
    public IActionResult UpdateRequestToPendingBuyerStatus(int id, [FromBody] UpdateReqDto updateReqDto)
    {
        // Validate the input
        if (updateReqDto.Status == null)
        {
            _logger.LogError("UpdateOrderStatus: Status is null.");
            return BadRequest("Order status is required.");
        }
        try
        {
            // Call the service to update the order status
            var response = _serviceManager.OrderService.UpdateRequestOrderStatusAsync(updateReqDto).Result;
            // Return the response
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"UpdateRequestToPendingBuyerStatus: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the Req order status.");
        }
        return Ok();
    }


    // Cancel Order On Payment Failure
    [HttpPut("cancel/{sessionId}")]
    public IActionResult CancelOrderOnPaymentFailure(string sessionId)
    {
        // Validate the input
        if (string.IsNullOrEmpty(sessionId))
        {
            _logger.LogError("CancelOrderOnPaymentFailure: Session ID is null or empty.");
            return BadRequest("Session ID is required.");
        }
        try
        {
            // Call the service to cancel the order on payment failure
            var response = _serviceManager.OrderService.CancelOrderOnPaymentFailureAsync(sessionId).Result;
            // Return the response
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"CancelOrderOnPaymentFailure: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while canceling the order on payment failure.");
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
    [HttpPost("request/stripe")]
    public IActionResult updateRequestWithStripe([FromBody] UpdateRequestDTO requestDto)
    {
        // Validate the input
        if (requestDto == null)
        {
            _logger.LogError("UpdateRequestDTO: Request DTO is null.");
            return BadRequest("Full request data is required.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError("UpdateRequestDTO: Invalid model state.");
            return BadRequest(ModelState);
        }

        try
        {
            // Call the service to add the request
            var response = _serviceManager.OrderService.AddStripeWithRequestAsync(requestDto).Result;
            // Return the response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"updateRequestWithStripe: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while Updating the request.");
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
    [HttpGet("analysis/orderPayment")]
    public async Task<IActionResult> GetOrderByPaymentTypeAsync([FromQuery] OrderType? type, [FromQuery] ReportMetricType metricType)
    {
        var res = _serviceManager.OrderService.CalculateOrdersVsPaymentStatusAsync(type, metricType).Result;
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
    public async Task<IActionResult> CalculateOrderVsStatus(
        [FromQuery] OrderType? type, [FromQuery] OrderStatus? status,
        [FromQuery] ReportMetricType metricType, [FromQuery] Time? time)
    {
        object res;
        switch (time)
        {
            case Time.Yearly:
                res = _serviceManager.OrderService.CalculateYearlyRevenueOrderVsStatus(type, status, metricType);
                break;
            case Time.Monthly:
                res = _serviceManager.OrderService.CalculateMonthlyRevenueOrderVsStatus(type, status, metricType);
                break;
            case Time.Weekly:
                res = _serviceManager.OrderService.CalculateWeeklyRevenueOrderVsStatus(type, status, metricType);
                break;
            default:
                return BadRequest("Invalid time filter");
        }

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
    [AllowAnonymous]
    [HttpGet("analysis/OrderSummary")]
    public async Task<IActionResult> CalculateOrderSummary()
    {

        var res = await _serviceManager.OrderService.OrderSummaryCalc();
        return Ok(res);
    }

    // Update Driver for Order
    [HttpPut("{orderId}/driver")]
    public async Task<IActionResult> UpdateOrderDriver(int orderId, [FromBody] string driverId)
    {
        if (string.IsNullOrEmpty(driverId))
        {
            _logger.LogError("UpdateOrderDriver: Driver ID is null or empty.");
            return BadRequest("Driver ID is required.");
        }
        try
        {
            var response = await _serviceManager.OrderService.UpdateOrderDriverAsync(orderId, driverId);
            return StatusCode(response.Status, response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"UpdateOrderDriver: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order's driver.");
        }
    }




}




