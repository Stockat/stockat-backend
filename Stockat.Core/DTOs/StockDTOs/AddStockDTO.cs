using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Entities;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.StockDTOs
{
    public class AddStockDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public StockStatus StockStatus { get; set; } = StockStatus.ForSale; // Default to ForSale
        public List<StockDetailsDTO> StockDetails { get; set; } = new List<StockDetailsDTO>();
    }

    public class StockDetailsDTO
    {
        public int FeatureId { get; set; }
        public int FeatureValueId { get; set; }
    }
}
