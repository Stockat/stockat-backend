using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Exceptions
{
    public class NullObjectParameterException :BadRequestException
    {
        public NullObjectParameterException(string parameterName) : base($"Parameter '{parameterName}' cannot be null.") { }
    }
}
