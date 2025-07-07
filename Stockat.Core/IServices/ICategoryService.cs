using Stockat.Core.DTOs.ProductDTOs;
using Stockat.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs.CategoryDtos;

namespace Stockat.Core.IServices;

public interface ICategoryService
{
    Task<GenericResponseDto<IEnumerable<CategoryDto>>> getAllCategory();

}
