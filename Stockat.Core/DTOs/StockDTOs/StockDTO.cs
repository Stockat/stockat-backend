using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.StockDTOs
{
    public class StockDTO
    {
        public int Id {  get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public List<StockFeaturesDTO> StockFeatures { get; set; }
    }

    public class StockFeaturesDTO 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
