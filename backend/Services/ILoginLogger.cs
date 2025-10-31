using System.Threading.Tasks;

namespace backend.Services
{
    public interface ILoginLogger
    {
        Task LogAsync(string username, bool success, string? ip, string? userAgent);
        Task EnsureTableAsync();
    }
}

