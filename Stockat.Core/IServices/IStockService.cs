using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.StockDTOs;

namespace Stockat.Core.IServices
{
    public interface IStockService
    {
        Task<GenericResponseDto<AddStockDTO>> AddStockAsync(AddStockDTO stockDto);
    }
}
