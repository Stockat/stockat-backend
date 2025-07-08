using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Entities;

namespace Stockat.Core.DTOs.ProductDTOs
{
    public class ProductWithFeaturesDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int MinQuantity { get; set; }

        public string SellerId { get; set; } = string.Empty;

        public string SellerName { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        public List<FeatureWithValuesDTO> Features { get; set; } = new List<FeatureWithValuesDTO>();
    }

    public class FeatureWithValuesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<FeatureValueDTO> Values { get; set; } = new List<FeatureValueDTO>();
    }

    public class FeatureValueDTO 
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
