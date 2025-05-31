using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Exceptions;

public class NotFoundException: Exception
{
    public NotFoundException(string message)
        : base(message)
    { }
}


// if it going to be used as a abstract class use the below
//public abstract class NotFoundException : Exception
//{ 
//    protected NotFoundException(string message)
//        : base(message)
//    { }
//}
