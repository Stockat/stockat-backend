using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Entities;

namespace Stockat.Core.DTOs.StockDTOs
{
    public class AddStockDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class StockDetailsDTO
    {
        public int FeatureId { get; set; }
        public int FeatureValueId { get; set; }
    }
}
