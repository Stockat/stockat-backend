using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.StockDTOs
{
    public class UpdateStockDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        public List<StockDetailsDTO> StockDetails { get; set; } = new List<StockDetailsDTO>();
    }
}