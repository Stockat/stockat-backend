using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ProductDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.IServices;

public interface IProductService
{
    Task<GenericResponseDto<PaginatedDto<IEnumerable<ProductHomeDto>>>> getAllProductsPaginated(int _size, int _page);
}
