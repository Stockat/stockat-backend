using Stockat.Core.DTOs.FeatureValueDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.FeatureDtos;

public class AddFeatureDto
{
    public string Name { get; set; } = string.Empty;
    public ICollection<AddFeatureValuesDto> FeatureValues { get; set; } = new List<AddFeatureValuesDto>();
}


