using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Stockat.Core.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    [NotMapped]
    public bool IsApproved => UserVerification?.Status == VerificationStatus.Approved; // to be reviewed --> defaults to false; will be set to true only after National ID is validated


    //  Navigation properties
    public virtual UserVerification UserVerification { get; set; } // make it virtual in case we used lazy loading
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Product> Products { get; set; }
    public virtual ICollection<OrderProduct> SellerOrderProducts { get; set; }
    public virtual ICollection<OrderProduct> BuyerOrderProducts { get; set; }

    //partial 1-M with Auction 
    public ICollection<Auction> CreatedAuctions { get; set; } = new List<Auction>();

    public ICollection<AuctionBidRequest> AuctionBidRequests { get; set; }
}

public class UserVerification
{
    public int Id { get; set; }

    [RegularExpression(@"^[2-3]\d{13}$", ErrorMessage = "National ID must be 14 digits and start with 2 or 3.")]
    public string NationalId { get; set; }

    public string ImageId { get; set; }
    public string ImageURL { get; set; } // 1 to 1  total

    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    public DateTime CreatedAt { get; set; }     // or sort according to the upload
    // the updatedAt property will be updated whenever the nationalId or the imageId is updated
    // and that will trigger the status to be pending again 
    public DateTime? UpdatedAt { get; set; }     // to sort according to the updated

    // foreign key and navigation to User
    public string UserId { get; set; }
    public User User { get; set; }


}

public enum VerificationStatus
{
    Pending,
    Approved,
    Rejected
}



