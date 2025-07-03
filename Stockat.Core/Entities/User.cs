using Microsoft.AspNetCore.Identity;
using Stockat.Core.Entities.Chat;
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

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? AboutMe { get; set; }

    public string? ProfileImageId { get; set; }
    public string? ProfileImageUrl { get; set; }


    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    [NotMapped]
    public bool IsApproved => UserVerification?.Status == VerificationStatus.Approved; // to be reviewed --> defaults to false; will be set to true only after National ID is validated
    public bool IsDeleted {  get; set; } = false;

    //  Navigation properties
    public virtual UserVerification UserVerification { get; set; } // make it virtual in case we used lazy loading
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Product> Products { get; set; }
    public virtual ICollection<OrderProduct> SellerOrderProducts { get; set; }
    public virtual ICollection<OrderProduct> BuyerOrderProducts { get; set; }

    //partial 1-M with Auction 
    public ICollection<Auction> CreatedAuctions { get; set; } = new List<Auction>();

    public ICollection<AuctionBidRequest> AuctionBidRequests { get; set; }


    // Chat system navigation
    public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatConversation> ConversationsAsUser1 { get; set; } = new List<ChatConversation>();
    public virtual ICollection<ChatConversation> ConversationsAsUser2 { get; set; } = new List<ChatConversation>();

    public virtual ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();

    public virtual ICollection<MessageReadStatus> MessageReadStatuses { get; set; } = new List<MessageReadStatus>();
    [NotMapped]
    public IEnumerable<ChatConversation> AllConversations => ConversationsAsUser1.Concat(ConversationsAsUser2);


}

public class UserVerification
{
    [Key] // will use the user primary key since this is 1:1 relation and a weak entity 
    // if it was 1:m we would add and Id property and the primary key would be composite
    public string UserId { get; set; }

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
    public User User { get; set; }


}

public enum VerificationStatus
{
    Pending,
    Approved,
    Rejected
}



