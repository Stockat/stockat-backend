using Microsoft.EntityFrameworkCore;
using Stockat.Core.Entities;
using Stockat.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using Stockat.Core.DTOs.ProductDTOs;

namespace Stockat.EF.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    protected StockatDBContext _context;
    protected IMapper _mapper;

    public ProductRepository(StockatDBContext context, IMapper mapper) : base(context)
    {
        _context = context;
        _mapper = mapper;
    }

    public new async Task<ProductDetailsDto> FindProductDetailsAsync(Expression<Func<Product, bool>> criteria, string[] includes = null)
    {
        IQueryable<Product> query = _context.Set<Product>();

        if (includes != null)
            foreach (var incluse in includes)
                query = query.Include(incluse);

        query = query.Where(criteria);

        return await query
             .ProjectTo<ProductDetailsDto>(_mapper.ConfigurationProvider)
             .SingleOrDefaultAsync();

    }
}
