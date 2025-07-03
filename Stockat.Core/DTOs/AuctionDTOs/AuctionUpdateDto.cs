using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionUpdateDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? StartingPrice { get; set; }

        [FutureDate]
        public DateTime? StartTime { get; set; }

        [FutureDate]
        public DateTime? EndTime { get; set; }

        public bool? IsClosed { get; set; }

        public int? ProductId { get; set; }
        public int? StockId { get; set; }


    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date > DateTime.UtcNow;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a future date";
        }
    }

}
