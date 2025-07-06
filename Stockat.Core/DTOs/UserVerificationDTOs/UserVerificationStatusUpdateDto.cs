namespace Stockat.Core.DTOs.UserVerificationDTOs
{
    public class UserVerificationStatusUpdateDto
    {
        public string UserId { get; set; }
        public string Status { get; set; } // "Approved" or "Rejected"
        public string? Note { get; set; } // Optional note from admin
    }
}