using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Entities;

namespace Stockat.Core.Entities
{
    public class Driver
    {
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Driver Name is Required")]
        [MinLength(3, ErrorMessage = "Driver Name Length Must Be Greater than or equal 2 char")]
        [MaxLength(50, ErrorMessage = "Driver Name Length Must Be less than or equal 50 char")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Driver Phone is Required")]
        [MinLength(11, ErrorMessage = "Driver Phone Length Must Be Greater than or equal 11 char")]
        [MaxLength(15, ErrorMessage = "Driver Phone Length Must Be less than or equal 15 char")]
        public string Phone { get; set; } = string.Empty;

        // Car Type
        [Required(ErrorMessage = "Car Type is Required")]
        public string CarType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Car Plate is Required")]
        public string CarPlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Car Color is Required")]
        public string CarColor { get; set; } = string.Empty;

        // current location of the driver
        public string Longitude { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;

        // Last Message
        [MaxLength(500, ErrorMessage = "Last Message Length Must Be less than or equal 500 char")]
        public string Message { get; set; } = string.Empty;

        // Last Update Time
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        // Foreign Key
        // Assigned Order Id
        public int? AssignedOrderId { get; set; } = null;


        // Navigation Properties
        public virtual OrderProduct? AssignedOrder { get; set; } // Nullable to allow for unassigned drivers
    }
}
