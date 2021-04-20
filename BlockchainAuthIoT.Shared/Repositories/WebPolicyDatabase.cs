using System.Net.Http;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Shared.Repositories
{
    public class WebPolicyDatabase : IPolicyDatabase
    {
        private readonly HttpClient _httpClient;

        public WebPolicyDatabase()
        {
            _httpClient = new();
        }

        public async Task<byte[]> GetPolicy(string location)
        {
            using var response = await _httpClient.GetAsync(location);
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
