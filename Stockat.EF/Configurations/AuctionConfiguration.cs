using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF.Configurations
{
    public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            //1-1 Auction with AuctionOrder
            builder
                .HasOne(a => a.AuctionOrder)
                .WithOne(o => o.Auction)
                .HasForeignKey<AuctionOrder>(o => o.AuctionId)
                .OnDelete(DeleteBehavior.NoAction);

            //prevent deleting Product if Auction exists
            builder
                .HasOne(a => a.Product)
                .WithMany(p => p.Auctions)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            //prevent deleting User if Auction exists
            builder
            .HasOne(a => a.SellerUser)
            .WithMany(u => u.CreatedAuctions)
            .HasForeignKey(a => a.SellerId)
            .OnDelete(DeleteBehavior.NoAction);

            builder
            .HasOne(a => a.Stock)
            .WithMany(s => s.Auctions)
            .HasForeignKey(a => a.StockId)
            .OnDelete(DeleteBehavior.NoAction);


        }
    }

    public class AuctionOrderConfiguration : IEntityTypeConfiguration<AuctionOrder>
    {
        public void Configure(EntityTypeBuilder<AuctionOrder> builder)
        {
            //1-1 with AuctionRequest
            builder
            .HasOne(o => o.AuctionRequest)
            .WithOne(r => r.AuctionOrder)
            .HasForeignKey<AuctionOrder>(o => o.AuctionRequestId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }





}
