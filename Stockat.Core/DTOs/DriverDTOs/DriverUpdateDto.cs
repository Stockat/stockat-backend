using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.DriverDTOs
{
    public class DriverUpdateDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string CarType { get; set; }
        [Required]
        public string CarPlate { get; set; }
        [Required]
        public string CarColor { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Message { get; set; }
    }
}
