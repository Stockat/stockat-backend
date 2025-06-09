namespace Stockat.Core.DTOs;

public class GenericResponseDto<T> where T : class
{
    public string Message { get; set; }
    public int Status { get; set; }

    public string? RedirectUrl { get; set; }
    public T Data { get; set; }
}

public class PaginatedDto<T> where T : class
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;
    public int Count { get; set; }
    public T PaginatedData { get; set; }
}

