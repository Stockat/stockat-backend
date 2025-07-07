using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.Entities.Chat;
using Stockat.EF.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF;

public class StockatDBContext : IdentityDbContext<User>
{
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<ServiceRequestUpdate> ServiceRequestUpdates { get; set; }


    public virtual DbSet<Feature> Features { get; set; }
    public virtual DbSet<FeatureValue> FeatureValues { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<StockDetails> StockDetails { get; set; }
    public virtual DbSet<Stock> Stocks { get; set; }
    public virtual DbSet<UserVerification> UserVerification { get; set; }
    public virtual DbSet<Auction> Auction { get; set; }
    public virtual DbSet<AuctionBidRequest> AuctionBidRequest { get; set; }
    public virtual DbSet<AuctionOrder> AuctionOrder { get; set; }
    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<OrderProduct> OrderProduct { get; set; }



    public DbSet<ChatConversation> ChatConversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<MessageReadStatus> MessageReadStatuses { get; set; }
    public StockatDBContext(DbContextOptions options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.ApplyConfiguration(new RoleConfiguration());
        //modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        //modelBuilder.ApplyConfiguration(new ServiceRequestConfiguration());
        //modelBuilder.ApplyConfiguration(new ServiceRequestUpdateConfiguration());
        //modelBuilder.ApplyConfiguration(new FeatureConfiguration());
        //modelBuilder.ApplyConfiguration(new FeatureValueConfiguration());
        //modelBuilder.ApplyConfiguration(new ProductConfiguration());
        //modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        //modelBuilder.ApplyConfiguration(new StockConfiguration());
        //modelBuilder.ApplyConfiguration(new UserVerificationConfiguration());
        //modelBuilder.ApplyConfiguration(new AuctionConfiguration());
        //modelBuilder.ApplyConfiguration(new AuctionOrderConfiguration());
        //modelBuilder.ApplyConfiguration(new OrderProductConfiguration());
        //modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // better performance
    }
}
