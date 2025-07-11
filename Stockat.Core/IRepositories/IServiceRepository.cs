﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Entities;

namespace Stockat.Core.IRepositories;

public interface IServiceRepository : IBaseRepository<Stockat.Core.Entities.Service>
{
    Task<IEnumerable<Stockat.Core.Entities.Service>> GetAllAvailableServicesWithSeller(int skip, int take, bool pendingOnly = false);
    public Task<Service> GetByIdWithSeller(int id);
    public Task<int> CountAllAvailableServicesAsync();


}
