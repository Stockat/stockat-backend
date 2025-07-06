using System.Threading.Tasks;

namespace Stockat.Core.IServices;

public interface IAIService
{
    Task<string> GenerateResponseAsync(string userMessage, object contextData);
} 