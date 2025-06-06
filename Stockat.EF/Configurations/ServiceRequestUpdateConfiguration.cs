using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;

namespace Stockat.EF.Configurations
{
    public class ServiceRequestUpdateConfiguration : IEntityTypeConfiguration<ServiceRequestUpdate>
    {
        public void Configure(EntityTypeBuilder<ServiceRequestUpdate> builder)
        {

            // ServiceRequest - ServiceRequestUpdate (1:N)
            builder
                .HasOne(sru => sru.ServiceRequest)
                .WithMany(sr => sr.RequestUpdates)
                .HasForeignKey(sru => sru.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);


            builder
                .Property(e => e.Status).HasConversion<string>();
        }
    }
    
}
