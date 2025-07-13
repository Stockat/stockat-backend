using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.DriverDTOs
{
    public class DriverDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string CarType { get; set; }
        public string CarPlate { get; set; }
        public string CarColor { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Message { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
