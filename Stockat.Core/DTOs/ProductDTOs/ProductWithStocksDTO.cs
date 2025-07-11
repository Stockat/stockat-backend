using Stockat.Core.DTOs.StockDTOs;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.ProductDTOs;

public class ProductWithStocksDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int MinQuantity { get; set; }
    public ProductStatus ProductStatus { get; set; }
    public bool IsDeleted { get; set; }
    public bool CanBeRequested { get; set; }
    public string Location { get; set; }
    public string RejectionReason { get; set; }
    public string SellerName { get; set; }
    public string CategoryName { get; set; }
    public List<string> Images { get; set; }
    public List<StockDTO> Stocks { get; set; }
}
