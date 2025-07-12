using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.DriverDTOs
{
    public class DriverStatusUpdateDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Longitude { get; set; }
        [Required]
        public string Latitude { get; set; }
        public string Message { get; set; }
    }
}
