using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.AuctionDTOs
{
    public class AuctionCreateDto
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal StartingPrice { get; set; }

        [Required]
        public decimal IncrementUnit { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string SellerId { get; set; }
    }

}
