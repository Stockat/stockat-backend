using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.EF.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.EF;

public class StockatDBContext: IdentityDbContext<User>
{
    public StockatDBContext(DbContextOptions options): base(options) 
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        // modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        //  modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
    }
}
