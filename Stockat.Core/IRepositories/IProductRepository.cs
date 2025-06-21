using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IRepositories;

public interface IProductRepository : IBaseRepository<Product>
{
    public new Task<ProductDetailsDto> FindProductDetailsAsync(Expression<Func<Product, bool>> criteria, string[] includes = null);
    public Task<bool> IsProductFoundAsync(Expression<Func<Product, bool>> criteria);
}
