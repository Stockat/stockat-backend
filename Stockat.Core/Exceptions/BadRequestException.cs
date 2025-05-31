namespace Stockat.Core.Exceptions;

public  class BadRequestException : Exception
{
    public BadRequestException(string message)
    : base(message)
    { }
}


//public abstract class BadRequestException : Exception
//{
//    protected BadRequestException(string message)
//    : base(message)
//    { }
//}